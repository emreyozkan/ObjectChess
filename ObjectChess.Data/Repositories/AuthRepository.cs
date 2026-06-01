using System;
using MySql.Data.MySqlClient;
using ObjectChess.Business.Interfaces;

namespace ObjectChess.Data.Repositories
{
    // This class handles database operations related to authentication
    public class AuthRepository : IAuthRepository
    {
        private readonly string _connectionString;

        // Constructor: gets database connection string
        public AuthRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Check if an email already exists in database
        public bool CheckIfEmailExists(string email)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                // Count users with same email
                string query = "SELECT COUNT(1) FROM Players WHERE Email = @Email;";
                
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);

                    // ExecuteScalar returns single value (count)
                    int count = Convert.ToInt32(command.ExecuteScalar());

                    // If count > 0, email already exists
                    return count > 0;
                }
            }
        }

        // Register a new user in database
        public void RegisterUser(string fullName, string email, string passwordHash)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                // Insert new user into Players table
                string query = "INSERT INTO Players (FullName, Email, PasswordHash) VALUES (@FullName, @Email, @PasswordHash);";
                
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FullName", fullName);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@PasswordHash", passwordHash);

                    // Execute insert query
                    command.ExecuteNonQuery();
                }
            }
        }

        // Try to get user data from database
        // out parameters: return multiple values safely
        public bool TryGetUserData(string email, out string? passwordHash, out string? fullName)
        {
            passwordHash = null;
            fullName = null;

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                // Get password hash and full name for login
                string query = "SELECT PasswordHash, FullName FROM Players WHERE Email = @Email;";
                
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        // If user exists, read data
                        if (reader.Read())
                        {
                            passwordHash = reader["PasswordHash"] != DBNull.Value
                                ? reader["PasswordHash"].ToString()
                                : null;

                            fullName = reader["FullName"] != DBNull.Value
                                ? reader["FullName"].ToString()
                                : null;

                            return true;
                        }
                    }
                }
            }

            // If no user found
            return false;
        }
    }
}