using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using System.Net;
using SongNameSpace.Models;
using WEBAPI.interfaces;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace SongHomeWork.service;


public class SongService : Isong
{
    IActiveUser activeUser;

    ISongReposetory songRepository;

    public SongService(ISongReposetory songRepository, IActiveUser activeUser)
    {
        this.songRepository = songRepository;
        this.activeUser = activeUser;
    }

    public List<Song> Get()
    {
        var userRole = activeUser.ActiveUser.Role;
        if (userRole == "admin")
        {
            return songRepository.Get();
        }
        var userId = activeUser.ActiveUser.Id;
        var userSongs = songRepository.Get().Where(s => s.userId == userId).ToList();
        return userSongs;
    }




    public Song Get(int id)
    {
        var song = songRepository.Get(id);
        if (song != null && song.userId != activeUser.ActiveUser.Id && activeUser.ActiveUser.Role != "admin")
        {
            return null;
        }
        return song;
    }


    public void Create(Song song)
    {
        if (activeUser.ActiveUser.Role != "admin" && song.userId != activeUser.ActiveUser.Id)
        {
            throw new UnauthorizedAccessException("You are not allowed to create a song for another user.");
        }
        songRepository.Create(song);
    }

    public int update(int id, Song song)
    {
        if (activeUser.ActiveUser.Role != "admin" && song.userId != activeUser.ActiveUser.Id)
        {
            throw new UnauthorizedAccessException("You are not allowed to update a song for another user.");
        }
        return songRepository.update(id, song);
    }

    public bool delete(int id)
    {
        var song = songRepository.Get(id);
        if (song == null)
            return false;
        if (activeUser.ActiveUser.Role != "admin" && song.userId != activeUser.ActiveUser.Id)
        {
            return false;
        }
        return songRepository.delete(id);
    }

}
public static class SongServiceExtention
{
    public static void addSongService(this IServiceCollection service)
    {
        service.AddScoped<Isong, SongService>();
    }
}

