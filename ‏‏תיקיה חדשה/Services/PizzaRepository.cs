using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using KsPizza.Interfaces;
using KsPizza.Models;
using Microsoft.AspNetCore.Hosting;

namespace KsPizza.Services
{
    public class PizzaRepository : IPizzaRepository
    {
        private readonly List<Pizza> pizzas;
        private readonly string filePath;

        public PizzaRepository(IWebHostEnvironment webHost)
        {
            filePath = Path.Combine(webHost.ContentRootPath, "Data", "Pizza.json");
            using var jsonFile = File.OpenText(filePath);
            pizzas = JsonSerializer.Deserialize<List<Pizza>>(jsonFile.ReadToEnd(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<Pizza>();
        }

        private void Save() => File.WriteAllText(filePath, JsonSerializer.Serialize(pizzas));

        public List<Pizza> GetAll() => pizzas;

        public Pizza Get(int id) => pizzas.FirstOrDefault(p => p.Id == id);

        public void Add(Pizza pizza)
        {
            pizza.Id = pizzas.Count == 0 ? 1 : pizzas.Max(p => p.Id) + 1;;
            pizzas.Add(pizza);
            Save();
        }

        public void Delete(int id)
        {
            var pizza = Get(id);
            if (pizza is null)
                return;

            pizzas.Remove(pizza);
            Save();
        }

        public void Update(Pizza pizza)
        {
            var index = pizzas.FindIndex(p => p.Id == pizza.Id);
            if (index == -1)
                return;

            pizzas[index] = pizza;
            Save();
        }

        public int Count => pizzas.Count;
    }
}
