
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using UserNameSpace.Models;
using WEBAPI.interfaces;


namespace SongHomeWork.Services
{
    public class ActiveUserService : IActiveUser
    {
        public User ActiveUser { get; private set; }
        
        public ActiveUserService(IHttpContextAccessor context)
        {
            var userClaims = context?.HttpContext?.User;
            var userId = userClaims?.FindFirst("userID") 
                       ?? userClaims?.FindFirst("Id") 
                       ?? userClaims?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            var userName = userClaims?.FindFirst("username") 
                       ?? userClaims?.FindFirst(System.Security.Claims.ClaimTypes.Name);
            if (userId != null)
            {
                ActiveUser = new User
                {
                    Id = int.Parse(userId.Value),
                    name = userName?.Value ?? "",
                    Role = context?.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? ""
                };
            }
        }

    }

    public static partial class ActiveuserExtensions
    {
        public static IServiceCollection AddActiveUser(this IServiceCollection services)
        {
            services.AddScoped<IActiveUser, ActiveUserService>();
            return services;
        }
    }
}