using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SongApi.Models;
using SongApi.interfaces;
using SongApi.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text;
using Token.Services;

namespace SongApi.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class UserController : ControllerBase
{

    Iuser service;

    public UserController(Iuser service)
    {
        this.service = service;
    }


    [HttpGet]
    [Authorize(Roles = "admin")]
    public ActionResult<IEnumerable<User>> GetAll()
    {
        return service.Get();
    }

    [HttpGet("me")]
    [Authorize(Roles = "admin,user")]
    public ActionResult<User> GetCurrentUser()
    {
        var tokenUserIdStr = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(tokenUserIdStr, out int userId))
        {
            var singleUser = service.Get(userId);
            if (singleUser == null)
               return NotFound(); // המשתמש שרשום בטוקן לא נמצא במסד הנתונים
            return singleUser;
        }
        // במקרה שהטוקן לא מכיל ID תקין
        return BadRequest();
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "admin")]
    public ActionResult<User> Get(int id)
    {
        var user = service.Get(id);
        if (user == null)
            return NotFound();
        return user;

    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public ActionResult Create(User user)
    {
        service.Create(user);
        return NoContent();

    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin,user")]
    public ActionResult update(int id, User user)
    {
        int i = service.Update(id, user);

        if (i == 0) return Forbid();
        if (i == 1) return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public ActionResult delete(int id)
    {
        bool flag = service.Delete(id);
        if (!flag)
            return NotFound();
        return NoContent();
    }


}

