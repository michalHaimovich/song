using SongNameSpace.Models;

namespace WEBAPI.interfaces;
 public interface Isong  
 {
      List<Song> Get();

      Song Get(int id);

      void Create(Song song);

       int update(int id, Song song);

       bool delete(int id);

 }