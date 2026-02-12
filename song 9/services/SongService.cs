using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using System.Net;
using SongNameSpace.Models;
using WEBAPI.interfaces;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace SongHomeWork.service{

      public class SongService : Isong{
        
        public List<Song>? ls {get; }
        private string filePath;
        public SongService(IWebHostEnvironment webHost){
            this.filePath=Path.Combine(webHost.ContentRootPath,"data","song.json"); //using arelative location
              using (var jsonFile = File.OpenText(filePath))
            {
                var content = jsonFile.ReadToEnd();
                ls = JsonSerializer.Deserialize<List<Song>>(content,
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

        //need to be changed
        public  List<Song> Get()
        {
            return ls;
        }



        public  Song Get(int id)
        {
           

            return ls.FirstOrDefault(m=>m.Id==id)!;
        }


        public  void Create(Song song)
        {
            song.Id=ls.Max(m=>m.Id)+1;
            ls.Add(song);
            saveToFile();
        }

        public  int update(int id, Song song){
            if(id!= song.Id)
                return 0;
            var index=ls.FindIndex(p=>p.Id==id);
            if(index==-1)
                return 1;
            ls[index]=song;
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

     public static class SongServiceExtention
     {
        public static void addSongService(this IServiceCollection service){
            service.AddSingleton<Isong, SongService>();
        }
     }

}