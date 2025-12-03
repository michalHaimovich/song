using System.Collections.Generic;
using System.Linq;
using UserNameSpace.Models;
using MyIuser.interfaces;

namespace MyUserSe.Service;

      public class UserService : Iuser{
        
        public List<User> ls;

        public UserService(){
           this.ls=new List<User>{
                new User(){Id=1,name="michal",age=13},
                new User(){Id=2,name="Gitty",age=13},
                new User(){Id=3,name="jhyt",age=73}, 
                new User(){Id=4,name="fds",age=18},
                new User(){Id=5,name="ytrtysdrft",age=23},
                new User(){Id=6,name="gfd",age=16},
                new User(){Id=7,name="a ",age=34}
        };

        }

        public  List<User> Get()
        {
            return ls;
        }



        public  User Get(int id)
        {
           

            return ls.FirstOrDefault(m=>m.Id==id)!;
        }


        public  void Create(User user)
        {
            user.Id=ls.Max(m=>m.Id)+1;
            ls.Add(user);
        }

        public  int update(int id, User user){
            if(id!= user.Id)
                return 0;
            var index=ls.FindIndex(p=>p.Id==id);
            if(index==-1)
                return 1;
            ls[index]=user;
            return 2;
        }

        public  bool delete(int id){
             var index=ls.FindIndex(p=>p.Id==id);
             if(index==-1)
                return false;
             else{ 
                ls.RemoveAt(index);
                return true;
            }
        }

     }

     public static class UserServiceExtention
     {
        public static void addUserService(this IServiceCollection service){
            service.AddSingleton<Iuser, UserService>();
        }
     }

