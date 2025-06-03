using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;

namespace FoodDeliverySystem
{
    class Program
    {
        static List<Client> clients = new List<Client>();
        static List<Restaurant> restaurants = new List<Restaurant>();
        static List<Courier> couriers = new List<Courier>();
        static List<Order> orders = new List<Order>();
        static List<Payment> payments = new List<Payment>();

        static void Main(string[] args)
        {
            LoadOrdersFromFile();

            Console.Title = "Millfood - Система доставки еды";
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║        MILLFOOD СИСТЕМА ДОСТАВКИ     ║");
            Console.WriteLine("╚══════════════════════════════════════╝");
            Console.ResetColor();

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine("\n=== ГЛАВНОЕ МЕНЮ ===");
                Console.ResetColor();
                Console.WriteLine("1. Добавить клиента");
                Console.WriteLine("2. Добавить ресторан");
                Console.WriteLine("3. Добавить курьера");
                Console.WriteLine("4. Создать заказ");
                Console.WriteLine("5. Просмотреть заказы");
                Console.WriteLine("6. Изменить статус заказа");
                Console.WriteLine("7. Назначить курьера");
                Console.WriteLine("8. Показать статистику");
                Console.WriteLine("9. Выход");
                Console.Write("Ваш выбор: ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": AddClient(); break;
                    case "2": AddRestaurant(); break;
                    case "3": AddCourier(); break;
                    case "4": CreateOrder(); break;
                    case "5": ShowOrders(); break;
                    case "6": UpdateOrderStatus(); break;
                    case "7": AssignCourier(); break;
                    case "8": ShowStatistics(); break;
                    case "9": return;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Неверный ввод.");
                        Console.ResetColor();
                        break;
                }
            }
        }

        static void AddClient()
        {
            Console.Write("ФИО: ");
            string name = Console.ReadLine();
            Console.Write("Телефон: ");
            string phone = Console.ReadLine();
            Console.Write("Адрес: ");
            string address = Console.ReadLine();
            clients.Add(new Client { Id = Guid.NewGuid(), Name = name, Phone = phone, Address = address });
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Клиент добавлен.");
            Console.ResetColor();
        }

        static void AddRestaurant()
        {
            Console.Write("Название ресторана: ");
            string name = Console.ReadLine();
            Console.Write("Адрес: ");
            string address = Console.ReadLine();
            Console.Write("Контактные данные: ");
            string contact = Console.ReadLine();

            var menu = new List<Dish>();
            Console.WriteLine("Добавьте блюда (пустое название — завершение):");
            while (true)
            {
                Console.Write("Блюдо: ");
                string dishName = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(dishName)) break;
                Console.Write("Категория: ");
                string category = Console.ReadLine();
                Console.Write("Цена: ");
                decimal price = decimal.Parse(Console.ReadLine());
                menu.Add(new Dish { Id = Guid.NewGuid(), Name = dishName, Category = category, Price = price });
            }

            restaurants.Add(new Restaurant { Id = Guid.NewGuid(), Name = name, Address = address, ContactInfo = contact, Menu = menu });
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Ресторан добавлен.");
            Console.ResetColor();
        }

        static void AddCourier()
        {
            Console.Write("ФИО курьера: ");
            string name = Console.ReadLine();
            Console.Write("Контакт: ");
            string contact = Console.ReadLine();
            Console.Write("Транспортное средство: ");
            string vehicle = Console.ReadLine();
            couriers.Add(new Courier { Id = Guid.NewGuid(), Name = name, ContactInfo = contact, Vehicle = vehicle, Status = CourierStatus.Свободен });
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Курьер добавлен.");
            Console.ResetColor();
        }

        static void CreateOrder()
        {
            if (!clients.Any() || !restaurants.Any())
            {
                Console.WriteLine("Необходимо добавить клиентов и рестораны перед оформлением заказа.");
                return;
            }

            Console.WriteLine("Выберите клиента:");
            for (int i = 0; i < clients.Count; i++)
                Console.WriteLine($"{i + 1}. {clients[i].Name} ({clients[i].Phone})");
            int clientIndex = int.Parse(Console.ReadLine()) - 1;
            var client = clients[clientIndex];

            Console.WriteLine("Выберите ресторан:");
            for (int i = 0; i < restaurants.Count; i++)
                Console.WriteLine($"{i + 1}. {restaurants[i].Name}");
            int restIndex = int.Parse(Console.ReadLine()) - 1;
            var restaurant = restaurants[restIndex];

            var selectedDishes = new List<Dish>();
            Console.WriteLine("Выберите блюда (введите номера, пустая строка — завершение):");
            for (int i = 0; i < restaurant.Menu.Count; i++)
                Console.WriteLine($"{i + 1}. {restaurant.Menu[i].Name} — {restaurant.Menu[i].Price} руб.");
            while (true)
            {
                string dishInput = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(dishInput)) break;
                int dishIndex = int.Parse(dishInput) - 1;
                selectedDishes.Add(restaurant.Menu[dishIndex]);
            }

            Console.Write("Введите время доставки (например, 18:30): ");
            string deliveryTime = Console.ReadLine();

            var order = new Order
            {
                Id = Guid.NewGuid(),
                Client = client,
                Restaurant = restaurant,
                Dishes = selectedDishes,
                Date = DateTime.Now,
                DeliveryTime = deliveryTime,
                Status = OrderStatus.Принят
            };

            orders.Add(order);
            SaveOrdersToFile();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Заказ создан. Ожидает назначения курьера.");
            Console.ResetColor();
        }

        static void AssignCourier()
        {
            var pendingOrders = orders.Where(o => o.Courier == null).ToList();
            if (!pendingOrders.Any())
            {
                Console.WriteLine("Нет заказов без курьера.");
                return;
            }

            Console.WriteLine("Выберите заказ:");
            for (int i = 0; i < pendingOrders.Count; i++)
                Console.WriteLine($"{i + 1}. Заказ клиента {pendingOrders[i].Client.Name}");
            int orderIndex = int.Parse(Console.ReadLine()) - 1;

            Console.WriteLine("Выберите курьера:");
            for (int i = 0; i < couriers.Count; i++)
                Console.WriteLine($"{i + 1}. {couriers[i].Name} — {couriers[i].Status}");
            int courierIndex = int.Parse(Console.ReadLine()) - 1;

            pendingOrders[orderIndex].Courier = couriers[courierIndex];
            couriers[courierIndex].Status = CourierStatus.Занят;
            SaveOrdersToFile();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Курьер назначен.");
            Console.ResetColor();
        }

        static void UpdateOrderStatus()
        {
            if (!orders.Any())
            {
                Console.WriteLine("Нет заказов.");
                return;
            }

            Console.WriteLine("Выберите заказ:");
            for (int i = 0; i < orders.Count; i++)
                Console.WriteLine($"{i + 1}. {orders[i].Client.Name} — {orders[i].Status}");
            int index = int.Parse(Console.ReadLine()) - 1;

            Console.WriteLine("Статусы: 1 — Принят, 2 — Готовится, 3 — Доставляется, 4 — Выполнен");
            int status = int.Parse(Console.ReadLine());
            orders[index].Status = (OrderStatus)(status - 1);
            SaveOrdersToFile();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Статус обновлён.");
            Console.ResetColor();
        }

        static void ShowOrders()
        {
            foreach (var o in orders)
            {
                Console.WriteLine($"{o.Date}: Клиент {o.Client.Name}, Ресторан: {o.Restaurant.Name}, Статус: {o.Status}");
            }
        }

        static void ShowStatistics()
        {
            Console.WriteLine("Статистика:");
            int completed = orders.Count(o => o.Status == OrderStatus.Выполнен);
            Console.WriteLine($"Выполнено заказов: {completed}");

            var topDishes = orders.SelectMany(o => o.Dishes)
                .GroupBy(d => d.Name)
                .OrderByDescending(g => g.Count())
                .Take(3);

            Console.WriteLine("Популярные блюда:");
            foreach (var dish in topDishes)
                Console.WriteLine($"{dish.Key} — {dish.Count()} раз");
        }

        static void SaveOrdersToFile()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            string json = JsonSerializer.Serialize(orders, options);
            File.WriteAllText("orders.txt", json);
        }

        static void LoadOrdersFromFile()
        {
            if (File.Exists("orders.txt"))
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles
                };

                string json = File.ReadAllText("orders.txt");
                orders = JsonSerializer.Deserialize<List<Order>>(json, options) ?? new List<Order>();
            }
        }
    }

    class Client
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }

    class Restaurant
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string ContactInfo { get; set; }
        public List<Dish> Menu { get; set; }
    }

    class Dish
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
    }

    class Order
    {
        public Guid Id { get; set; }
        public Client Client { get; set; }
        public Restaurant Restaurant { get; set; }
        public List<Dish> Dishes { get; set; }
        public Courier Courier { get; set; }
        public DateTime Date { get; set; }
        public string DeliveryTime { get; set; }
        public OrderStatus Status { get; set; }
    }

    class Courier
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ContactInfo { get; set; }
        public string Vehicle { get; set; }
        public CourierStatus Status { get; set; }
    }

    class Payment
    {
        public Guid Id { get; set; }
        public Order Order { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; }
        public string Status { get; set; }
    }

    enum OrderStatus { Принят, Готовится, Доставляется, Выполнен }
    enum CourierStatus { Свободен, Занят }
}
