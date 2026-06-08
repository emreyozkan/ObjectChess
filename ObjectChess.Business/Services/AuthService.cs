using ObjectChess.Business.Interfaces;
using ObjectChess.Business.Models;

namespace ObjectChess.Business.Services;

public class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IPasswordPolicy passwordPolicy) : IAuthService
{
    public void Register(string fullName, string email, string password)
    {
        // None of the fields are allowed to be empty
        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("All fields are required.");
        }

        // Check the password is strong enough before doing anything else
        if (!passwordPolicy.IsValid(password, out string? policyError))
        {
            throw new ArgumentException(policyError);
        }

        // Make sure nobody already registered with this email
        if (userRepository.EmailExists(email))
        {
            throw new ArgumentException("This email is already registered.");
        }

        // Build the new user and trim the spaces off the name and email
        UserModel user = new()
        {
            FullName = fullName.Trim(),
            Email = email.Trim(),
            // Never store the real password and only keep the hashed version
            PasswordHash = passwordHasher.Hash(password)
        };

        userRepository.CreateUser(user);
    }

    public UserModel? Login(string email, string password)
    {
        // Look up the user by their email first
        UserModel? user = userRepository.GetByEmail(email);

        // No user with that email or no stored hash means login fails
        if (user is null || string.IsNullOrEmpty(user.PasswordHash))
        {
            return null;
        }

        // Only return the user if the typed password matches the stored hash
        return passwordHasher.Verify(password, user.PasswordHash) ? user : null;
    }
}
