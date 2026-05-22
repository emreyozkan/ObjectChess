using System.Collections.Generic;
using ObjectChess.Business.Interfaces;

namespace ObjectChess.Tests.Fakes
{
    public class FakeAuthRepository : IAuthRepository
    {
        private readonly List<string> _emails = new List<string>();
        private readonly Dictionary<string, (string FullName, string PasswordHash)> _users = new Dictionary<string, (string, string)>();

        public bool CheckIfEmailExists(string email)
        {
            return _emails.Contains(email);
        }

        public void RegisterUser(string fullName, string email, string passwordHash)
        {
            _emails.Add(email);
            _users[email] = (fullName, passwordHash);
        }

        public bool TryGetUserData(string email, out string? passwordHash, out string? fullName)
        {
            if (_users.ContainsKey(email))
            {
                fullName = _users[email].FullName;
                passwordHash = _users[email].PasswordHash;
                return true;
            }

            fullName = null;
            passwordHash = null;
            return false;
        }
    }
}