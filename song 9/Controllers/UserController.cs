using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using UserNameSpace.Models;
using MyIuser.interfaces;
using MyUserSe.Service;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text;
using Token.Services;

namespace MyUser.Controllers
{
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


        [HttpPost]
        [Route("[action]")]
        [AllowAnonymous]
        public ActionResult<String> Login([FromBody] User user)
        {
            if (user.name == null || user.Password == null)
                return BadRequest();

            if (user.name == "michal" && user.Password == "0684")
            {
                var claims = new List<Claim>
                {
                new Claim("username", user.name),
                new Claim("userID", user.Id.ToString()),
                new Claim(ClaimTypes.Role, "admin")
                };

                var token = TokenService.GetToken(claims);

                return new OkObjectResult(TokenService.WriteToken(token));
            }

            bool containsTarget = false;
            List<User> users = service.Get();

            foreach (User u in users)
            {
                if (u.name == user.name && u.Password == user.Password)
                {
                    containsTarget = true;
                    break;
                }
            }

            if (containsTarget == false)
            {
                return Unauthorized();
            }
            else
            {
                var claims = new List<Claim>
                {
                new Claim("username", user.name),
                new Claim("userID", user.Id.ToString()),
                new Claim(ClaimTypes.Role, "user")
                };

                var token = TokenService.GetToken(claims);

                return new OkObjectResult(TokenService.WriteToken(token));
            }
        }

        [HttpGet]
        [Authorize(Roles = "admin,user")] 
        public ActionResult<IEnumerable<User>> Get()
        {
            // 1. שליפת התפקיד וה-ID מתוך הטוקן
            var userRole = this.User.FindFirst(ClaimTypes.Role)?.Value;
            var tokenUserIdStr = this.User.FindFirst("userID")?.Value;

            // 2. בדיקה: אם זה אדמין, נחזיר את כל המשתמשים
            if (userRole == "admin")
            {
                return service.Get(); // מחזיר את כל הרשימה
            }

            // 3. אם זה משתמש רגיל, נחזיר רק אותו
            if (int.TryParse(tokenUserIdStr, out int userId))
            {
                var singleUser = service.Get(userId);

                if (singleUser == null)
                {
                    return NotFound(); // המשתמש שרשום בטוקן לא נמצא במסד הנתונים
                }
                return new List<User> { singleUser };
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
        [Authorize(Roles = "admin")]
        public ActionResult update(int id, User user)
        {
            int i = service.update(id, user);
            if (i == 0)
                return BadRequest();
            if (i == 1)
                return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public ActionResult delete(int id)
        {
            bool flag = service.delete(id);
            if (!flag)
                return NotFound();
            return NoContent();
        }


    }

}