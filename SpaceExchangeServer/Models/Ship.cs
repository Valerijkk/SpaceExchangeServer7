namespace SpaceExchangeServer.Models
{
    public class Ship
    {
        public string SessionId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, int> Inventory { get; set; } = new(); // Ресурсы на борту
        public int Credits { get; set; } = 1000; // Условные деньги
    }
}