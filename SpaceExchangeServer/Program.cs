using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using SpaceExchangeServer.Models;
using SpaceExchangeServer.Services;

namespace SpaceExchangeServer
{
    internal class Program
    {
        // Порты
        private const int TcpPort = 5001;       // регистрация
        private const int UdpPort = 5002;       // цены в реальном времени
        private const int WebSocketPort = 5003; // события аукциона

        private static TcpListener? _tcpListener;
        private static UdpClient? _udpClient;
        private static IPEndPoint? _udpBroadcastEndpoint;

        private static HttpListener? _wsListener;
        private static List<WebSocket> _wsConnections = new();

        // Хранилище (память) для кораблей, ресурсов и сделок
        private static ExchangeService _exchangeService = new();

        static void Main(string[] args)
        {
            Console.WriteLine("=== SpaceExchangeServer запущен ===");

            // 1) Запуск TCP-сервера (регистрация/авторизация)
            StartTcpServer();

            // 2) Запуск UDP-сервера для рассылки «быстрых данных» (цены)
            StartUdp();

            // 3) Запуск WebSocket-сервера для событий аукциона
            StartWebSocket();

            // Периодические задачи: обновление цен, рассылка событий
            StartPeriodicTasks();

            Console.WriteLine("Нажмите Enter для завершения...");
            Console.ReadLine();
            StopAll();
        }

        private static void StartTcpServer()
        {
            _tcpListener = new TcpListener(IPAddress.Any, TcpPort);
            _tcpListener.Start();
            Console.WriteLine($"TCP-сервер слушает порт {TcpPort}.");

            // Асинхронно принимаем клиентов
            _tcpListener.BeginAcceptTcpClient(OnTcpClient, null);
        }

        private static void OnTcpClient(IAsyncResult ar)
        {
            try
            {
                var client = _tcpListener!.EndAcceptTcpClient(ar);
                Console.WriteLine("[TCP] Принято новое соединение.");
                _tcpListener.BeginAcceptTcpClient(OnTcpClient, null);

                // Обрабатываем клиента
                HandleTcpClient(client);
            }
            catch
            {
                // Игнорируем исключения при остановке
            }
        }

        private static async void HandleTcpClient(TcpClient tcp)
        {
            try
            {
                using var ns = tcp.GetStream();
                byte[] buf = new byte[1024];
                int read = await ns.ReadAsync(buf, 0, buf.Length);
                if (read > 0)
                {
                    string shipName = Encoding.UTF8.GetString(buf, 0, read).Trim();

                    // Регистрируем корабль, получаем sessionId
                    string sessionId = Guid.NewGuid().ToString();
                    _exchangeService.RegisterShip(sessionId, shipName);

                    // Возвращаем sessionId
                    byte[] resp = Encoding.UTF8.GetBytes(sessionId);
                    await ns.WriteAsync(resp, 0, resp.Length);

                    Console.WriteLine($"[TCP] Корабль '{shipName}' зарегистрирован -> sessionId={sessionId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TCP] Ошибка: {ex.Message}");
            }
        }

        private static void StartUdp()
        {
            _udpClient = new UdpClient();
            _udpClient.EnableBroadcast = true;
            _udpBroadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, UdpPort);
            Console.WriteLine($"UDP-сервер готов вещать на порт {UdpPort} (broadcast).");
        }

        private static void StartWebSocket()
        {
            _wsListener = new HttpListener();

            // Добавляем оба варианта (при желании можно оставить только один):
            _wsListener.Prefixes.Add($"http://localhost:{WebSocketPort}/ws/");
            _wsListener.Prefixes.Add($"http://127.0.0.1:{WebSocketPort}/ws/");

            // Запуск
            _wsListener.Start();
            Console.WriteLine($"WebSocket-сервер слушает на порту {WebSocketPort} (http://localhost:{WebSocketPort}/ws/).");

            // Начинаем ожидать подключений
            _wsListener.BeginGetContext(OnWsConnection, null);
        }

        private static void OnWsConnection(IAsyncResult ar)
        {
            if (_wsListener == null) return;
            HttpListenerContext ctx;
            try
            {
                ctx = _wsListener.EndGetContext(ar);
                // Снова начинаем слушать
                _wsListener.BeginGetContext(OnWsConnection, null);
            }
            catch
            {
                return;
            }

            if (!ctx.Request.IsWebSocketRequest)
            {
                ctx.Response.StatusCode = 400;
                ctx.Response.Close();
                return;
            }

            // Принимаем WebSocket
            HandleWebSocketClient(ctx);
        }

        private static async void HandleWebSocketClient(HttpListenerContext ctx)
        {
            try
            {
                var wsCtx = await ctx.AcceptWebSocketAsync(null);
                var ws = wsCtx.WebSocket;
                lock (_wsConnections)
                {
                    _wsConnections.Add(ws);
                }
                Console.WriteLine("[WebSocket] Клиент подключился.");

                // Принимаем от клиента запросы (на покупку/продажу, т.п.)
                await ReceiveWebSocket(ws);

                lock (_wsConnections)
                {
                    _wsConnections.Remove(ws);
                }
                Console.WriteLine("[WebSocket] Клиент отключился.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WebSocket] Ошибка: " + ex.Message);
            }
        }

        private static async Task ReceiveWebSocket(WebSocket ws)
        {
            var buffer = new byte[4096];
            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", CancellationToken.None);
                    break;
                }

                // Получаем строку
                string msg = Encoding.UTF8.GetString(buffer, 0, result.Count).Trim();
                Console.WriteLine("[WebSocket] Получено: " + msg);

                // Пример: { "Action": "Buy", "SessionId": "...", "Resource": "QuantumOre", "Amount": 10 }
                try
                {
                    var request = JsonSerializer.Deserialize<AuctionRequest>(msg);
                    if (request != null)
                    {
                        switch (request.Action)
                        {
                            case "Buy":
                                var buyRes = _exchangeService.BuyResource(request.SessionId, request.Resource, request.Amount);
                                string buyJson = JsonSerializer.Serialize(buyRes);
                                await ws.SendAsync(Encoding.UTF8.GetBytes(buyJson),
                                    WebSocketMessageType.Text, true, CancellationToken.None);
                                break;

                            case "Sell":
                                var sellRes = _exchangeService.SellResource(request.SessionId, request.Resource, request.Amount);
                                string sellJson = JsonSerializer.Serialize(sellRes);
                                await ws.SendAsync(Encoding.UTF8.GetBytes(sellJson),
                                    WebSocketMessageType.Text, true, CancellationToken.None);
                                break;
                        }
                    }
                }
                catch (Exception parseEx)
                {
                    Console.WriteLine("[WebSocket] Ошибка парсинга: " + parseEx.Message);
                    // Можно отправить клиенту сообщение об ошибке
                }
            }
        }

        private static void StartPeriodicTasks()
        {
            // 1) Каждые 1-5 минут генерируем «события дня» / «аукционные события»
            _ = Task.Run(async () =>
            {
                var rnd = new Random();
                while (true)
                {
                    int delaySec = rnd.Next(60, 301); // 1..5 минут
                    await Task.Delay(TimeSpan.FromSeconds(delaySec));
                    string eventMsg = $"Событие дня: Уникальный лот №{rnd.Next(1000, 9999)}!";
                    BroadcastWebSocketEvent(eventMsg);
                }
            });

            // 2) Каждые 30 секунд обновляем цены (внутри биржи) и рассылаем по UDP
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(30));
                    var updated = _exchangeService.UpdateResourcePrices();
                    var json = JsonSerializer.Serialize(updated);
                    byte[] data = Encoding.UTF8.GetBytes(json);
                    _udpClient?.Send(data, data.Length, _udpBroadcastEndpoint!);
                    Console.WriteLine($"[UDP] Обновление цен: {json}");
                }
            });
        }

        private static void BroadcastWebSocketEvent(string message)
        {
            Console.WriteLine("[WebSocket] Broadcast: " + message);
            var bytes = Encoding.UTF8.GetBytes(message);
            lock (_wsConnections)
            {
                foreach (var ws in _wsConnections)
                {
                    if (ws.State == WebSocketState.Open)
                    {
                        ws.SendAsync(new ArraySegment<byte>(bytes),
                                     WebSocketMessageType.Text,
                                     true,
                                     CancellationToken.None);
                    }
                }
            }
        }

        private static void StopAll()
        {
            _tcpListener?.Stop();
            _udpClient?.Dispose();
            _wsListener?.Stop();

            lock (_wsConnections)
            {
                foreach (var ws in _wsConnections)
                {
                    if (ws.State == WebSocketState.Open)
                    {
                        ws.CloseAsync(WebSocketCloseStatus.NormalClosure,
                                      "Server shutdown",
                                      CancellationToken.None).Wait();
                    }
                }
            }
        }
    }

    // Формат входящих запросов при торговых операциях
    public class AuctionRequest
    {
        public string? Action { get; set; }     // "Buy" / "Sell"
        public string? SessionId { get; set; }
        public string? Resource { get; set; }
        public int Amount { get; set; }
    }
}
