using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SongApi.Models;
using SongApi.Services;
using SongApi.interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Token.Services;

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

}