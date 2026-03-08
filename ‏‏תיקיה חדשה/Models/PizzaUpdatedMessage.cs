using System;

namespace KsPizza.Models
{
    public class PizzaUpdatedMessage
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PizzaName { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
