using ObjectChess.Business.Interfaces;
using ObjectChess.Business.Models;

namespace ObjectChess.Tests.Fakes;

public class FakeUserRepository : IUserRepository
{
    private readonly Dictionary<string, UserModel> _users = [];
    private int _nextId = 1;

    public bool EmailExists(string email)
    {
        return _users.ContainsKey(email);
    }

    public void CreateUser(UserModel user)
    {
        user.UserId = _nextId++;
        _users[user.Email] = user;
    }

    public UserModel? GetByEmail(string email)
    {
        return _users.TryGetValue(email, out UserModel? user) ? user : null;
    }
}
