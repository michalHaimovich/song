using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using KsPizza.Models;
using KsPizza.Services;

namespace KsPizza.Controllers;

[ApiController]
[Route("[controller]")]
public class LoginController : ControllerBase
{
    public LoginController() { }

    [HttpPost]
    public ActionResult<String> Login(User User)
    {
        var dt = DateTime.Now;

        if (User.Username != "test"
        || User.Password != $"t{dt.Year}#{dt.Day}!")
        {
            return Unauthorized();
        }

        var claims = new List<Claim>
        {
            new Claim("id", "11235813"),
            new Claim("username", "test"),
        };

        var token = PizzaTokenService.GetToken(claims);

        return new OkObjectResult(PizzaTokenService.WriteToken(token));
    }
}