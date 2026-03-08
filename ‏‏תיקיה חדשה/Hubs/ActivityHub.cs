using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace KsPizza.Hubs;

[Authorize]
public class ActivityHub : Hub
{
    public async Task BroadcastActivity(string username, string action, string pizzaName)
    {
        await Clients.All.SendAsync("ReceiveActivity", username, action, pizzaName);
    }
}
