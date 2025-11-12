using System.Collections.Generic;
using System.Linq;
using SongNameSpace.Models;
using WEBAPI.interfaces;

namespace SongHomeWork.service{

      public class SongService : Isong{
        
        public List<Song> ls;

        public SongService(){
           this.ls=new List<Song>{
                new Song(){Id=1,name="ytrtyrft",composer="fds"},
                new Song(){Id=2,name="dsasd",composer="fdsrew"},
                new Song(){Id=3,name="jhyt",composer="sgsd"}, 
                new Song(){Id=4,name="fds",composer="jyht"},
                new Song(){Id=5,name="ytrtysdrft",composer="rywe"},
                new Song(){Id=6,name="gfd",composer="mjyt"},
                new Song(){Id=7,name="a ",composer="fds"}
        };

        }

        public  List<Song> Get()
        {
            return ls;
        }



        public  Song Get(int id)
        {
           

            return ls.FirstOrDefault(m=>m.Id==id);
        }


        public  void Create(Song song)
        {
            song.Id=ls.Max(m=>m.Id)+1;
            ls.Add(song);
        }

        public  int update(int id, Song song){
            if(id!= song.Id)
                return 0;
            var index=ls.FindIndex(p=>p.Id==id);
            if(index==-1)
                return 1;
            ls[index]=song;
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

     public static class SongServiceExtention
     {
        public static void addSongService(this IServiceCollection service){
            service.AddSingleton<Isong, SongService>();
        }
     }

}