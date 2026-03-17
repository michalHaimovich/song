using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using System.Net;
using SongApi.Models;
using SongApi.interfaces;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace SongApi.Services;
public class GenericRepository<T> : IGenericRepository<T> where T : class
{

    public List<T> ls { get; }
    private string filePath;
    public GenericRepository(IWebHostEnvironment webHost)
    {
        this.filePath = Path.Combine(webHost.ContentRootPath, "data", $"{typeof(T).Name.ToLower()}.json"); //using arelative location
        using (var jsonFile = File.OpenText(filePath))
        {
            var content = jsonFile.ReadToEnd();
            ls = JsonSerializer.Deserialize<List<T>>(content,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<T>();
        }
    }
    private void saveToFile()
    {
        var text = JsonSerializer.Serialize(ls);
        File.WriteAllText(filePath, text);
    }
    public List<T> Get()
    {

        List<T> items = [.. ls];
        return items;


    }
    public T Get(int id)
    {
        var idProp = typeof(T).GetProperty("Id");
        if (idProp == null) return null!;
        return ls.FirstOrDefault(m => (int)idProp.GetValue(m)! == id)!;
    }
    public void Create(T item)
    {
        var idProp = typeof(T).GetProperty("Id");
        if (idProp != null)
        {
            if (ls == null || ls.Count == 0)
            {
                idProp.SetValue(item, 1);
            }
            else
            {
                var maxId = ls.Max(m => (int)idProp.GetValue(m)!);
                idProp.SetValue(item, maxId + 1);
            }
        }
        ls.Add(item);
        saveToFile();
    }

    public int Update(int id, T item)
    {
        var idProp = typeof(T).GetProperty("Id");
        if (idProp == null) return 0;
        if (id != (int)idProp.GetValue(item)!) return 0;
        var index = ls.FindIndex(p => (int)idProp.GetValue(p)! == id);
        if (index == -1)
            return 1;
        ls[index] = item;
        saveToFile();
        return 2;

    }

    public bool Delete(int id)
    {
        var idProp = typeof(T).GetProperty("Id");
        if (idProp == null) return false;
        var index = ls.FindIndex(p => (int)idProp.GetValue(p)! == id);
        if (index == -1)
            return false;
        else
        {
            ls.RemoveAt(index);
            saveToFile();
            return true;
        }
    }

}

public static class GenericRepositoryExtention
{
    public static void addGenericRepository<T>(this IServiceCollection service) where T : class
    {
        service.AddSingleton<IGenericRepository<T>, GenericRepository<T>>();
    }
}

