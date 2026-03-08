using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using System.Net;
using SongApi.Models;
using SongApi.interfaces;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR; // חסר לך ה-using הזה
using SongApi.Hubs; // כדי להכיר את ActivityHub

namespace SongApi.Services;

public class SongService : Isong
{
    IActiveUser activeUser;

    ISongReposetory songRepository;

    IHubContext<ActivityHub> hubContext;

    public SongService(ISongReposetory songRepository, IActiveUser activeUser, IHubContext<ActivityHub> hubContext)
    {
        this.songRepository = songRepository;
        this.activeUser = activeUser;
        this.hubContext = hubContext;
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
    public Song? Get(int id)
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
        BroadcastActivity("created", song);
    }
    public int Update(int id, Song song)
    {
        if (activeUser.ActiveUser.Role != "admin" && song.userId != activeUser.ActiveUser.Id)
        {
            throw new UnauthorizedAccessException("You are not allowed to update a song for another user.");
        }
        BroadcastActivity("updated", song);
        return songRepository.Update(id, song);
    }
    public bool Delete(int id)
    {
        var song = songRepository.Get(id);
        if (song == null)
            return false;
        if (activeUser.ActiveUser.Role != "admin" && song.userId != activeUser.ActiveUser.Id)
        {
            return false;
        }
        BroadcastActivity("deleted", song);
        return songRepository.Delete(id);
    }

    // הפונקציה הפרטית החדשה שלנו שמנהלת את חוקי השידור
  private void BroadcastActivity(string actionName, Song song)
{
    var performerId = activeUser.ActiveUser.Id;
    var performerName = activeUser.ActiveUser.name;
    var ownerId = song.userId;
    var songName = song.name;

    // 1. הודעה לבעל השיר (למי שהשיר שייך לו)
    string personalMessage;
    if (performerId == ownerId)
    {
        personalMessage = $"You successfully {actionName} your song '{songName}'.";
    }
    else
    {
        personalMessage = $"Admin '{performerName}' {actionName} your song '{songName}'.";
    }
    
    // שידור לבעל השיר
    hubContext.Clients.User(ownerId.ToString())
        .SendAsync("ReceivePersonalActivity", personalMessage, song);


    // 2. החדש: הודעה למנהל שמבצע את הפעולה (אם הוא משנה שיר של מישהו אחר)
    if (performerId != ownerId)
    {
        string performerMessage = $"You successfully {actionName} user {ownerId}'s song '{songName}'.";
        
        // שידור חזרה למנהל שלחץ על הכפתור
        hubContext.Clients.User(performerId.ToString())
            .SendAsync("ReceivePersonalActivity", performerMessage, song);
    }


    // 3. הודעה לשאר המנהלים במערכת (Admins)
    string adminMessage;
    if (performerId == ownerId)
    {
        adminMessage = $"User '{performerName}' {actionName} their own song '{songName}'.";
    }
    else
    {
        adminMessage = $"Admin '{performerName}' {actionName} user {ownerId}'s song '{songName}'.";
    }
    
    // שידור לקבוצת המנהלים (נשלח את ה-performerId כדי שהמבצע יסנן אותה ב-JS)
    hubContext.Clients.Group("Admins")
        .SendAsync("ReceiveGlobalActivity", adminMessage, song, performerId);
}
}
public static class SongServiceExtention
{
    public static void addSongService(this IServiceCollection service)
    {
        service.AddScoped<Isong, SongService>();
    }
}

