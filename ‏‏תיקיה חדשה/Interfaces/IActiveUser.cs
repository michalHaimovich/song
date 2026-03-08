using KsPizza.Models;
using Microsoft.AspNetCore.Http;


namespace KsPizza.Interfaces
{
    public interface IActiveUser
    {
        User ActiveUser { get; }
    }
}