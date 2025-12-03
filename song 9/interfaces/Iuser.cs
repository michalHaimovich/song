using UserNameSpace.Models;

namespace MyIuser.interfaces;
 public interface Iuser
 {
      List<User> Get();

      User Get(int id);

      void Create(User user);

       int update(int id, User user);

       bool delete(int id);

 }