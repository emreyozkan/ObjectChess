using System;
using ObjectChess.Business.Interfaces;
using ObjectChess.Business.Security;

namespace ObjectChess.Business.Services
{
    // This service handles authentication logic (register + login)
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;

        // Constructor: repository is injected using dependency injection
        public AuthService(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        // Register a new user
        public void Register(string fullName, string email, string password)
        {
            // Validate input values
            if (string.IsNullOrWhiteSpace(fullName) || 
                string.IsNullOrWhiteSpace(email) || 
                string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Inputs cannot be empty.");
            }

            // Check if email already exists in database
            bool emailExists = _authRepository.CheckIfEmailExists(email);
            
            if (emailExists)
            {
                throw new ArgumentException("This email is already registered.");
            }

            // Hash password before saving (never store plain password)
            string passwordHash = PasswordHasher.HashPassword(password);

            // Save user to database
            _authRepository.RegisterUser(fullName, email, passwordHash);
        }

        // Login user and return fullName if success, otherwise null
        public string? Login(string email, string password)
        {
            string? storedHash;
            string? fullName;

            // Get user data from database
            bool userFound = _authRepository.TryGetUserData(email, out storedHash, out fullName);
            
            // If user not found or password hash is missing
            if (!userFound || string.IsNullOrEmpty(storedHash))
            {
                return null; 
            }

            // Verify password using hashing function
            bool isPasswordValid = PasswordHasher.VerifyPassword(password, storedHash);
            
            // If password is correct, login success
            if (isPasswordValid)
            {
                return fullName;
            }

            // Otherwise login fails
            return null;
        }
    }
}