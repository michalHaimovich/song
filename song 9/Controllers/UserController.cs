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

namespace MyUser.Controllers{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
     public class UserController : ControllerBase{
         
        Iuser service;

        public UserController(Iuser service){
            this.service=service;
        }


        [HttpPost]
        [Route("[action]")]
        public ActionResult<String> Login([FromBody] User user)
        {
            if(user.name == null || user.Password == null)
                return BadRequest();
            
            if(user.name == "michal"&& user.Password == "0684")
            {
                var claims = new List<Claim>
                {
                new Claim("username", user.name),
                new Claim("userID", user.Id.ToString()),
                new Claim("userType", "admin")
                };
               
                var token = TokenService.GetToken(claims);
                
                return new OkObjectResult(TokenService.WriteToken(token));
            }
            
            bool containsTarget = false;
            List<User> users= service.Get();

            foreach (User u in users)
            {
                if (u.name ==user.name && u.Password == user.Password )
                {
                    containsTarget = true;
                    break; 
                }
            }   

            if(containsTarget== false)
            { 
                return Unauthorized();
            }
            else{
                var claims = new List<Claim>
                {
                new Claim("username", user.name),
                new Claim("userID", user.Id.ToString()),
                new Claim("userType", "user")
                };
               
                var token = TokenService.GetToken(claims);
                
                return new OkObjectResult(TokenService.WriteToken(token));
            }                 
        }


        [HttpGet]
        [Authorize(Roles = "admin")]
        public IEnumerable<User> Get()
        {
          return service.Get();
        }


        [HttpGet("{id}")]
        [Authorize(Roles = "admin")]    
        public ActionResult<User> Get(int id)
        {
            var user=service.Get(id);
            if(user == null)
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
        public ActionResult update(int id, User user){
           int i= service.update(id,user);
            if(i==0)
             return BadRequest();
            if(i==1)
                return NotFound();
            return NoContent(); 
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public ActionResult delete(int id){
           bool flag= service.delete(id);
            if(!flag)
                return NotFound();
            return NoContent();
        }


     }

}