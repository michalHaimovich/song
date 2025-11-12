using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
 using SongNameSpace.Models;
using SongHomeWork.service;
using WEBAPI.interfaces;

namespace SongHomeWork.Controllers{
    [ApiController]
    [Route("[controller]")]
     public class SongController : ControllerBase{
         
        Isong service;

        public SongController(Isong service){
            this.service=service;
        }

         [HttpGet]
        public IEnumerable<Song> Get()
        {
          return  service.Get();
        }


        [HttpGet("{id}")]
        public ActionResult<Song> Get(int id)
        {
            var song=service.Get(id);
            if(song == null)
                return NotFound();
            return song;

        }

        [HttpPost] 
        public ActionResult Create(Song song)
        {
            service.Create(song);
            return NoContent();

        }

        [HttpPut("{id}")]
        public ActionResult update(int id, Song song){
           int i= service.update(id,song);
            if(i==0)
             return BadRequest();
            if(i==1)
                return NotFound();
            return NoContent(); 
        }

        [HttpDelete("{id}")]
        public ActionResult delete(int id){
           bool flag= service.delete(id);
            if(!flag)
                return NotFound();
            return NoContent();
        }


     }

}