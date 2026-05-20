using System;

namespace ObjectChess.Business.Interfaces
{
    public interface IAuthService
    {
        void Register(string fullName, string email, string password);
        string? Login(string email, string password);
    }
}