using ObjectChess.Business.Models;

namespace ObjectChess.Business.Interfaces;

public interface IUserRepository
{
    bool EmailExists(string email);
    void CreateUser(UserModel user);
    UserModel? GetByEmail(string email);
}
