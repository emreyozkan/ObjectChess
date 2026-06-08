using ObjectChess.Business.Models;

namespace ObjectChess.Business.Interfaces;

public interface IAuthService
{
    void Register(string fullName, string email, string password);
    UserModel? Login(string email, string password);
}
