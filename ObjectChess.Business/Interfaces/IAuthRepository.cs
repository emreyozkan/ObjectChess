using System;

namespace ObjectChess.Business.Interfaces
{
    public interface IAuthRepository
    {
        bool CheckIfEmailExists(string email);
        void RegisterUser(string fullName, string email, string passwordHash);
        bool TryGetUserData(string email, out string? passwordHash, out string? fullName);
    }
}