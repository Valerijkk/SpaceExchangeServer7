using SpaceExchangeServer.Models;
using System;

namespace SpaceExchangeServer.Services
{
    // Здесь простая логика биржи "в памяти":
    // - Список кораблей
    // - Список ресурсов + цены
    // - Покупка/продажа
    // - Обновление цен
    public class ExchangeService
    {
        private object _lock = new();
        private Dictionary<string, Ship> _ships = new();      // sessionId -> Ship
        private Dictionary<string, int> _resourcePrices = new(); // "QuantumOre" -> текущая цена

        public ExchangeService()
        {
            // Изначально несколько ресурсов
            _resourcePrices["QuantumOre"] = 200;
            _resourcePrices["SpaceMineral"] = 150;
            _resourcePrices["GalacticGas"] = 80;
        }

        public void RegisterShip(string sessionId, string shipName)
        {
            lock (_lock)
            {
                if (!_ships.ContainsKey(sessionId))
                {
                    var ship = new Ship
                    {
                        SessionId = sessionId,
                        Name = shipName
                    };
                    _ships[sessionId] = ship;
                }
            }
        }

        public TradeResult BuyResource(string? sessionId, string? resource, int amount)
        {
            if (string.IsNullOrWhiteSpace(sessionId) ||
                string.IsNullOrWhiteSpace(resource) ||
                amount <= 0) 
            {
                return new TradeResult 
                { 
                    Success = false, 
                    Message = "Неверные данные" 
                };
            }

            lock (_lock)
            {
                if (!_ships.TryGetValue(sessionId, out var ship))
                {
                    return new TradeResult { Success = false, Message = "Сессия не найдена" };
                }
                if (!_resourcePrices.TryGetValue(resource, out int price))
                {
                    return new TradeResult { Success = false, Message = "Ресурс не существует" };
                }

                int totalCost = price * amount;
                if (ship.Credits < totalCost)
                {
                    return new TradeResult
                    {
                        Success = false,
                        Message = $"Недостаточно кредитов. Нужно {totalCost}, у вас {ship.Credits}"
                    };
                }

                // Покупка
                ship.Credits -= totalCost;
                if (!ship.Inventory.ContainsKey(resource))
                    ship.Inventory[resource] = 0;
                ship.Inventory[resource] += amount;

                return new TradeResult
                {
                    Success = true,
                    Message = $"Куплено {amount} ед. {resource}",
                    NewBalance = ship.Credits,
                    Inventory = new Dictionary<string, int>(ship.Inventory)
                };
            }
        }

        public TradeResult SellResource(string? sessionId, string? resource, int amount)
        {
            if (string.IsNullOrWhiteSpace(sessionId) ||
                string.IsNullOrWhiteSpace(resource) ||
                amount <= 0)
            {
                return new TradeResult
                {
                    Success = false,
                    Message = "Неверные данные"
                };
            }

            lock (_lock)
            {
                if (!_ships.TryGetValue(sessionId, out var ship))
                {
                    return new TradeResult { Success = false, Message = "Сессия не найдена" };
                }
                if (!ship.Inventory.ContainsKey(resource) || ship.Inventory[resource] < amount)
                {
                    return new TradeResult { Success = false, Message = "Недостаточно ресурса на борту" };
                }
                if (!_resourcePrices.TryGetValue(resource, out int price))
                {
                    return new TradeResult { Success = false, Message = "Ресурс не существует" };
                }

                // Продажа
                ship.Inventory[resource] -= amount;
                int totalEarn = price * amount;
                ship.Credits += totalEarn;

                return new TradeResult
                {
                    Success = true,
                    Message = $"Продано {amount} ед. {resource}",
                    NewBalance = ship.Credits,
                    Inventory = new Dictionary<string, int>(ship.Inventory)
                };
            }
        }

        public Dictionary<string, int> UpdateResourcePrices()
        {
            lock (_lock)
            {
                // Простейшее случайное изменение цен
                var rnd = new Random();
                foreach (var key in _resourcePrices.Keys.ToList())
                {
                    int oldPrice = _resourcePrices[key];
                    int delta = rnd.Next(-10, 11); // от -10 до +10
                    int newPrice = Math.Max(1, oldPrice + delta); // Не меньше 1
                    _resourcePrices[key] = newPrice;
                }
                // Возвращаем копию текущих цен
                return new Dictionary<string, int>(_resourcePrices);
            }
        }
    }
}
