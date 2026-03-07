using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using UserNameSpace.Models;

namespace WEBAPI.interfaces;

public interface IActiveUser
{
    User ActiveUser { get; }
}