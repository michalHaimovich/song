using Microsoft.AspNetCore.Mvc;
using SongApi.Models;
using SongApi.interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Token.Services;
using Google.Apis.Auth;
using System.Threading.Tasks;

namespace SongApi.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class LogginController : ControllerBase
{

    Iuser service;
    public LogginController(Iuser service)
    {
        this.service = service;
    }

    [HttpPost]
    [Route("[action]")]
    [AllowAnonymous]
    public ActionResult<String> Login([FromBody] User user)
    {
        if (user.name == null || user.Password == null)
            return BadRequest();

        List<User> users = service.Get();

        foreach (User u in users)
        {
            if (u.name == user.name && u.Password == user.Password)
            {
                List<Claim> claims;
                if (u.Role == "admin")
                {
                    claims = new List<Claim>
                     {
                         new Claim("username", u.name),
                         new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                         new Claim(ClaimTypes.Role, "admin")
                    };
                }
                else
                {
                    claims = new List<Claim>
                    {
                         new Claim("username", user.name),
                         new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                     new Claim(ClaimTypes.Role, "user")
                    };
                }

                var token = TokenService.GetToken(claims);

                return new OkObjectResult(TokenService.WriteToken(token));
            }
        }
        return Unauthorized();
    }
    public class GoogleLoginRequest
    {
        public string? Credential { get; set; }
    }
    [HttpPost]
    [Route("[action]")]
    [AllowAnonymous]
    public async Task<ActionResult<String>> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Credential))
        {
            return BadRequest("Token is missing");
        }

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(request.Credential);
            
            string googleEmail = payload.Email;
            string googleName = payload.Name ?? (payload.GivenName + " " + payload.FamilyName).Trim();

            List<User> users = service.Get();
            User existingUser = users.FirstOrDefault(u => u.name == googleEmail || u.name == googleName);

            string role = "user"; 
            string finalName = googleName;
            string finalId; // יכיל את ה-ID המספרי התקין

            if (existingUser != null)
            {
                role = existingUser.Role; 
                finalName = existingUser.name;
                finalId = existingUser.Id.ToString();
            }
            else 
            {
                // משתמש חדש: נוסיף אותו למסד הנתונים עם סיסמה זמנית
                var newUser = new User
                {
                    Id = 0, // ייקבע על ידי המאגר
                    name = googleName,
                    Password = "google" + Guid.NewGuid().ToString().Substring(0, 8), // סיסמה אקראית
                    Role = "user"
                };
                service.Create(newUser);
                finalId = newUser.Id.ToString();
                finalName = newUser.name;
            }

            var claims = new List<Claim>
            {
                new Claim("username", finalName),
                new Claim(ClaimTypes.NameIdentifier, finalId), // עכשיו זה מספר תקין!
                new Claim(ClaimTypes.Role, role) 
            };

            var token = TokenService.GetToken(claims);
            return new OkObjectResult(TokenService.WriteToken(token));
        }
        catch (InvalidJwtException)
        {
            return Unauthorized("Invalid Google token.");
        }
    }
}
