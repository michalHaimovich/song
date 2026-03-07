using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SongNameSpace.Models;
using SongHomeWork.service;
using WEBAPI.interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SongHomeWork.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class SongController : ControllerBase
{

    Isong service;

    public SongController(Isong service)
    {
        this.service = service;
    }

    [HttpGet]
    [Authorize(Roles = "admin,user")]
    public ActionResult<IEnumerable<Song>> Get()
    {
        return service.Get();
    }


    [HttpGet("{id}")]
    [Authorize(Roles = "admin,user")]
    public ActionResult<Song> Get(int id)
    {
        var song = service.Get(id);
        if (song == null)
            return NotFound();
        return song;

    }

    [HttpPost]
    [Authorize(Roles = "admin,user")]
    public ActionResult Create(Song song)
    {
        try
        {
            service.Create(song);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        return NoContent();

    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin,user")]
    public ActionResult update(int id, Song song)
    {
        int i;
        try
        {
            i = service.update(id, song);
            if (i == 0)
                return BadRequest();
            if (i == 1)
                return NotFound();

        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        return NoContent();


    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin,user")]
    public ActionResult delete(int id)
    {
        bool flag = service.delete(id);
        if (!flag)
            return NotFound();
        return NoContent();
    }


}

