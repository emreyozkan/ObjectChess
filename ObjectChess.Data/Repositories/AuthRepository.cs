using System;
using MySql.Data.MySqlClient;
using ObjectChess.Business.Interfaces;

namespace ObjectChess.Data.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly string _connectionString;

        public AuthRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool CheckIfEmailExists(string email)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(1) FROM Players WHERE Email = @Email;";
                
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public void RegisterUser(string fullName, string email, string passwordHash)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Players (FullName, Email, PasswordHash) VALUES (@FullName, @Email, @PasswordHash);";
                
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FullName", fullName);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                    command.ExecuteNonQuery();
                }
            }
        }

        public bool TryGetUserData(string email, out string? passwordHash, out string? fullName)
        {
            passwordHash = null;
            fullName = null;

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT PasswordHash, FullName FROM Players WHERE Email = @Email;";
                
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            passwordHash = reader["PasswordHash"] != DBNull.Value ? reader["PasswordHash"].ToString() : null;
                            fullName = reader["FullName"] != DBNull.Value ? reader["FullName"].ToString() : null;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}