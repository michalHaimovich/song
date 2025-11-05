using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
 using SongNameSpace.Models;
using SongHomeWork.service;

namespace SongHomeWork.Controllers{
    [ApiController]
    [Route("[controller]")]
     public class SongController : ControllerBase{

        public SongController(){
            

        }

         [HttpGet]
        public IEnumerable<Song> Get()
        {
          return  SongService.Get();
        }


        [HttpGet("{id}")]
        public ActionResult<Song> Get(int id)
        {
            var song=SongService.Get(id);
            if(song == null)
                return NotFound();
            return song;

        }

        [HttpPost] 
        public ActionResult Create(Song song)
        {
            SongService.Create(song);
            return NoContent();

        }

        [HttpPut("{id}")]
        public ActionResult update(int id, Song song){
           int i= SongService.update(id,song);
            if(i==0)
             return BadRequest();
            if(i==1)
                return NotFound();
            return NoContent(); 
        }

        [HttpDelete("{id}")]
        public ActionResult delete(int id){
           bool flag= SongService.delete(id);
            if(!flag)
                return NotFound();
            return NoContent();
        }


     }

}