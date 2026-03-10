using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace SongApi.Hubs 
{
    [Authorize] 
    public class ActivityHub : Hub
    {
        // הוספנו את הפנקס של המורה
        public static readonly ConcurrentDictionary<string, List<string>> UserConnections = new();

        public override async Task OnConnectedAsync()
        {
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            if (role == "admin")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }

            var userId = Context.UserIdentifier;
            if (userId != null)
            {
                var connectionId = Context.ConnectionId;
                UserConnections.AddOrUpdate(
                    userId,
                    new List<string> { connectionId },
                    (key, existingList) => { lock (existingList) { existingList.Add(connectionId); } return existingList; });
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (userId != null)
            {
                var connectionId = Context.ConnectionId;
                if (UserConnections.TryGetValue(userId, out var existingList))
                {
                    lock (existingList) 
                    { 
                        existingList.Remove(connectionId); 
                        if (existingList.Count == 0) UserConnections.TryRemove(userId, out _);
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}