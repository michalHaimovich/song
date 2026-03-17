
namespace SongApi.interfaces;

public interface IGenericRepository<T> : Icrud<T> where T : class
{
}