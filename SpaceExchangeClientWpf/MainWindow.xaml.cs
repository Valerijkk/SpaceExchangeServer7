using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using System.Windows.Threading;

namespace SpaceExchangeClientWpf
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<ServerInfo> _servers = new();
        private string _sessionId = "";
        private UdpClient? _udp;
        private ClientWebSocket? _ws;

        public MainWindow()
        {
            InitializeComponent();
            ListServers.ItemsSource = _servers;
        }

        private void BtnAddServer_Click(object sender, RoutedEventArgs e)
        {
            var si = new ServerInfo
            {
                Ip = TxtIp.Text.Trim(),
                TcpPort = int.Parse(TxtTcp.Text),
                UdpPort = int.Parse(TxtUdp.Text),
                WsPort = int.Parse(TxtWs.Text)
            };
            _servers.Add(si);
        }

        private void BtnRemoveServer_Click(object sender, RoutedEventArgs e)
        {
            var selected = (ServerInfo)ListServers.SelectedItem;
            if (selected != null) _servers.Remove(selected);
        }

        private async void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            var selected = (ServerInfo)ListServers.SelectedItem;
            if (selected == null) return;

            // 1) TCP -> получаем sessionId
            await ConnectTcp(selected);

            // 2) UDP -> прослушка цен
            StartUdp(selected);

            // 3) WebSocket -> события + торговые запросы
            await ConnectWebSocket(selected);

            AddEvent("Подключение к серверу завершено.");
        }

        private async Task ConnectTcp(ServerInfo si)
        {
            try
            {
                using var tcp = new TcpClient();
                await tcp.ConnectAsync(si.Ip, si.TcpPort);

                var ns = tcp.GetStream();
                string shipName = "MyStarship"; // можно делать отдельным TextBox
                byte[] data = Encoding.UTF8.GetBytes(shipName);
                await ns.WriteAsync(data, 0, data.Length);

                byte[] buf = new byte[1024];
                int read = await ns.ReadAsync(buf, 0, buf.Length);
                _sessionId = Encoding.UTF8.GetString(buf, 0, read);
                AddEvent($"TCP: sessionId = {_sessionId}");
            }
            catch (Exception ex)
            {
                AddEvent($"Ошибка TCP: {ex.Message}");
            }
        }

        private void StartUdp(ServerInfo si)
        {
            try
            {
                if (_udp != null)
                {
                    _udp.Close();
                    _udp = null;
                }
                _udp = new UdpClient(si.UdpPort);

                _ = Task.Run(async () =>
                {
                    while (_udp != null)
                    {
                        try
                        {
                            var res = await _udp.ReceiveAsync();
                            var msg = Encoding.UTF8.GetString(res.Buffer);
                            Dispatcher.Invoke(() =>
                            {
                                AddEvent($"UDP: {msg}");
                            });
                        }
                        catch { break; }
                    }
                });

                AddEvent("UDP-подключение запущено.");
            }
            catch (Exception ex)
            {
                AddEvent($"Ошибка UDP: {ex.Message}");
            }
        }

        private async Task ConnectWebSocket(ServerInfo si)
        {
            try
            {
                if (_ws != null && _ws.State == WebSocketState.Open)
                {
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                }
                _ws = new ClientWebSocket();
                var uri = new Uri($"ws://{si.Ip}:{si.WsPort}/ws/");
                await _ws.ConnectAsync(uri, CancellationToken.None);

                AddEvent("WebSocket: подключено.");

                // Принимаем события
                _ = Task.Run(async () =>
                {
                    var buffer = new byte[4096];
                    while (_ws != null && _ws.State == WebSocketState.Open)
                    {
                        try
                        {
                            var res = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                            if (res.MessageType == WebSocketMessageType.Close)
                            {
                                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                                break;
                            }
                            var msg = Encoding.UTF8.GetString(buffer, 0, res.Count);
                            Dispatcher.Invoke(() => AddEvent($"WebSocket: {msg}"));
                        }
                        catch
                        {
                            break;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                AddEvent($"Ошибка WebSocket: {ex.Message}");
            }
        }

        private void BtnClearEvents_Click(object sender, RoutedEventArgs e)
        {
            TxtEvents.Clear();
        }

        private void BtnOpenTrade_Click(object sender, RoutedEventArgs e)
        {
            if (_ws == null || _ws.State != WebSocketState.Open)
            {
                AddEvent("WebSocket не подключен!");
                return;
            }
            var tradeWin = new TradeWindow(_ws, _sessionId);
            tradeWin.Show();
        }

        private void AddEvent(string text)
        {
            string line = $"[{DateTime.Now:HH:mm:ss}] {text}";
            TxtEvents.AppendText(line + Environment.NewLine);
            TxtEvents.ScrollToEnd();
            Console.WriteLine(line);
        }
    }
}
