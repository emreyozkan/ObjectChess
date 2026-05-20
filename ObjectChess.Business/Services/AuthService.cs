using System;
using ObjectChess.Business.Interfaces;
using ObjectChess.Business.Security;

namespace ObjectChess.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;

        public AuthService(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public void Register(string fullName, string email, string password)
        {
            bool emailExists = _authRepository.CheckIfEmailExists(email);
            
            if (emailExists)
            {
                throw new ArgumentException("This email is already registered.");
            }

            string passwordHash = PasswordHasher.HashPassword(password);
            _authRepository.RegisterUser(fullName, email, passwordHash);
        }

        public string? Login(string email, string password)
        {
            string? storedHash;
            string? fullName;

            bool userFound = _authRepository.TryGetUserData(email, out storedHash, out fullName);
            
            if (!userFound || string.IsNullOrEmpty(storedHash))
            {
                return null; 
            }

            bool isPasswordValid = PasswordHasher.VerifyPassword(password, storedHash);
            
            if (isPasswordValid)
            {
                return fullName;
            }

            return null;
        }
    }
}