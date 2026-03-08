using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SongApi.Hubs 
{
    [Authorize] // חסימה הרמטית: אי אפשר לפתוח חיבור WebSocket בלי טוקן תקין
    public class ActivityHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            // שולפים את התפקיד של המשתמש מתוך הטוקן שלו
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            // אם הוא מנהל, אנחנו מכניסים את החיבור הספציפי שלו לקבוצה מיוחדת
            if (role == "admin")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }

            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}