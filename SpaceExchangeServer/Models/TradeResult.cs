namespace SpaceExchangeServer.Models
{
    public class TradeResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public int NewBalance { get; set; }
        public Dictionary<string, int>? Inventory { get; set; }
    }
}