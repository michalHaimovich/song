using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using SongApi.Models;
using SongApi.interfaces;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;


namespace SongApi.Services;

      public class UserService : Iuser{
        
        private IGenericRepository<User> userRepo;
        private IGenericRepository<Song> songService;
        private IActiveUser activeUser;

        public UserService(IGenericRepository<User> userRepo, IGenericRepository<Song> songService, IActiveUser activeUser){
             this.userRepo = userRepo;
             this.songService = songService;
             this.activeUser = activeUser;
        }  

        public  List<User> Get()
        {
            return userRepo.Get();
        }

        public  User Get(int id)
        {
            return userRepo.Get(id);
        }

        public  void Create(User user)
        {
            userRepo.Create(user);
        }

        public  int Update(int id, User user){
            var existing = userRepo.Get(id);
            if (existing == null) return 1;

            var userRole = activeUser.ActiveUser.Role;
            var tokenUserId = activeUser.ActiveUser.Id;

            if (userRole != "admin" && tokenUserId != id)
            {
                return 0; // Forbid equivalent
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(user.name)) existing.name = user.name;
            if (!string.IsNullOrEmpty(user.Password)) existing.Password = user.Password;
            if (!string.IsNullOrEmpty(user.Role))
            {
                if (userRole == "admin")
                {
                    existing.Role = user.Role;
                }
                else
                {
                    existing.Role = "user";
                }
            }
            else if (userRole != "admin")
            {
                existing.Role = "user";
            }

            return userRepo.Update(id, existing);
        }

        public  bool Delete(int id){
             var songsToDelete = songService.Get().Where(s => s.userId == id).ToList();
             foreach (var song in songsToDelete)
             {
                 songService.Delete(song.Id);
             }
             return userRepo.Delete(id);
        }

     }

     public static class UserServiceExtention
     {
        public static void addUserService(this IServiceCollection service){
            service.AddScoped<Iuser, UserService>();
        }
     }

