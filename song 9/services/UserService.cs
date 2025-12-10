using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using UserNameSpace.Models;
using MyIuser.interfaces;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;


namespace MyUserSe.Service;

      public class UserService : Iuser{
        
        private List<User> ls {get; }
        private string filePath;
        public UserService(IWebHostEnvironment webHost){
             this.filePath=Path.Combine(webHost.ContentRootPath,"data","user.json");
              using (var jsonFile = File.OpenText(filePath))
            {
                var content = jsonFile.ReadToEnd();
                ls = JsonSerializer.Deserialize<List<User>>(content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
        }

           private void saveToFile()
        {
            var text = JsonSerializer.Serialize(ls);
            File.WriteAllText(filePath, text);
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
            saveToFile();
        }

        public  int update(int id, User user){
            if(id!= user.Id)
                return 0;
            var index=ls.FindIndex(p=>p.Id==id);
            if(index==-1)
                return 1;
            ls[index]=user;
            saveToFile();
            return 2;
        }

        public  bool delete(int id){
             var index=ls.FindIndex(p=>p.Id==id);
             if(index==-1)
                return false;
             else{ 
                ls.RemoveAt(index);
                saveToFile();
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

