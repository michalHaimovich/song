using KsPizza.Models;
using System.Collections.Generic;

namespace KsPizza.Interfaces
{
    public interface IPizzaService
    {
        List<Pizza> GetAll();
        Pizza Get(int id);
        void Add(Pizza pizza);
        void Delete(int id);
        void Update(Pizza pizza);
        int Count {get;}
    }
}