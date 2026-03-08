using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SongApi.Models;

namespace SongApi.interfaces;

public interface IActiveUser
{
    User ActiveUser { get; }
}