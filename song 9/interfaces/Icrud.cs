namespace SongApi.interfaces;
public interface Icrud<T>
{
      List<T> Get();

      T? Get(int id);

      void Create(T item);

       int Update(int id, T item);

       bool Delete(int id);
}