using System;
using System.Collections.Generic;
using System.Linq;
using KsPizza.Hubs;
using KsPizza.Interfaces;
using KsPizza.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace KsPizza.Services
{
    public class PizzaService : IPizzaService
    {
        private readonly IHubContext<ActivityHub> hubContext;
        private readonly IPizzaRepository repository;
        private readonly IRabbitMqService rabbitMqService;
        private readonly int activeUserId;
        private readonly string activeUsername;

        public PizzaService(IPizzaRepository repository, IActiveUser activeUser, IHubContext<ActivityHub> hubContext, IRabbitMqService rabbitMqService)
        {
            this.repository = repository;
            this.hubContext = hubContext;
            this.rabbitMqService = rabbitMqService;
            var user = activeUser.ActiveUser;
            if (user is null)
                throw new System.InvalidOperationException("Active user is required");
            this.activeUserId = user.Id;
            this.activeUsername = user.Username;
        }

        public List<Pizza> GetAll()
            => repository
                .GetAll()
                .Where(p => p.UserId == activeUserId)
                .ToList();

        public Pizza Get(int id)
        {
            var pizza = repository.Get(id);
            return pizza?.UserId == activeUserId ? pizza : null;
        }

        public void Add(Pizza pizza)
        {
            pizza.UserId = activeUserId;
            repository.Add(pizza);
            BroadcastActivity("added", pizza);
        }

        public void Delete(int id)
        {
            var pizza = Get(id);
            if (pizza is null)
                return;

            if (pizza.UserId != activeUserId)
                return;

            repository.Delete(id);
            BroadcastActivity("deleted", pizza);
        }

        public void Update(Pizza pizza)
        {
            var existing = repository.Get(pizza.Id);
            if (existing?.UserId != activeUserId)
                return;

            pizza.UserId = activeUserId;
            repository.Update(pizza);
            QueueActivityBroadcast(pizza);
        }

        private void BroadcastActivity(string action, Pizza pizza)
        {
            hubContext.Clients.All.SendAsync("ReceiveActivity", activeUsername, action, pizza.Name);
        }

        private void QueueActivityBroadcast(Pizza pizza)
        {
            var message = new PizzaUpdatedMessage
            {
                UserId = activeUserId,
                Username = activeUsername,
                PizzaName = pizza.Name,
                Timestamp = DateTime.UtcNow
            };

            rabbitMqService.PublishPizzaUpdated(message).Wait();
        }

        public int Count => GetAll().Count;
    }

    public static partial class KsPizzaExtensions
    {
        public static IServiceCollection AddPizza(this IServiceCollection services)
        {
            services.AddSingleton<IPizzaRepository, PizzaRepository>();
            services.AddScoped<IPizzaService, PizzaService>();
            return services;
        }
    }
}