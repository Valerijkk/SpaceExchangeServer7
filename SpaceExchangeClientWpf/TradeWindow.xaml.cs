using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows;

namespace SpaceExchangeClientWpf
{
    public partial class TradeWindow : Window
    {
        private ClientWebSocket _ws;
        private string _sessionId;

        public TradeWindow(ClientWebSocket ws, string sessionId)
        {
            InitializeComponent();
            _ws = ws;
            _sessionId = sessionId;
        }

        private async void BtnBuy_Click(object sender, RoutedEventArgs e)
        {
            if (_ws.State != WebSocketState.Open)
            {
                Log("WebSocket не в состоянии Open");
                return;
            }

            string resource = TxtResource.Text.Trim();
            if (!int.TryParse(TxtAmount.Text.Trim(), out int amount) || amount <= 0)
            {
                Log("Неверное количество!");
                return;
            }

            var req = new
            {
                Action = "Buy",
                SessionId = _sessionId,
                Resource = resource,
                Amount = amount
            };
            string json = JsonSerializer.Serialize(req);
            await _ws.SendAsync(Encoding.UTF8.GetBytes(json), WebSocketMessageType.Text,
                true, CancellationToken.None);
            Log($"Отправлен запрос на покупку: {resource}, кол={amount}");
        }

        private async void BtnSell_Click(object sender, RoutedEventArgs e)
        {
            if (_ws.State != WebSocketState.Open)
            {
                Log("WebSocket не в состоянии Open");
                return;
            }

            string resource = TxtResource.Text.Trim();
            if (!int.TryParse(TxtAmount.Text.Trim(), out int amount) || amount <= 0)
            {
                Log("Неверное количество!");
                return;
            }

            var req = new
            {
                Action = "Sell",
                SessionId = _sessionId,
                Resource = resource,
                Amount = amount
            };
            string json = JsonSerializer.Serialize(req);
            await _ws.SendAsync(Encoding.UTF8.GetBytes(json), WebSocketMessageType.Text,
                true, CancellationToken.None);
            Log($"Отправлен запрос на продажу: {resource}, кол={amount}");
        }

        private void Log(string text)
        {
            TxtTradeLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {text}\n");
            TxtTradeLog.ScrollToEnd();
        }
    }
}
