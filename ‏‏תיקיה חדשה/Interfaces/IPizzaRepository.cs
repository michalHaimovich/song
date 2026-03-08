using System.Collections.Generic;
using KsPizza.Models;

namespace KsPizza.Interfaces
{
    public interface IPizzaRepository
    {
        List<Pizza> GetAll();
        Pizza Get(int id);
        void Add(Pizza pizza);
        void Delete(int id);
        void Update(Pizza pizza);
        int Count { get; }
    }
}
