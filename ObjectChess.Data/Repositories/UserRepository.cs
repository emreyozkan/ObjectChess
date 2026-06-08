using MySql.Data.MySqlClient;
using ObjectChess.Business.Interfaces;
using ObjectChess.Business.Models;

namespace ObjectChess.Data.Repositories;

public class UserRepository(string connectionString) : IUserRepository
{
    // Checks if a user already signed up with this email
    public bool EmailExists(string email)
    {
        // Open a fresh connection to the database
        using MySqlConnection connection = new(connectionString);
        connection.Open();

        // @Email is a parameter and not glued straight into the query text
        // This is the thing that stops SQL injection
        using MySqlCommand command = new(
            "SELECT COUNT(1) FROM Users WHERE Email = @Email;", connection);
        command.Parameters.AddWithValue("@Email", email);

        // A count above zero means the email is already taken
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    // Saves a brand new user into the Users table
    public void CreateUser(UserModel user)
    {
        using MySqlConnection connection = new(connectionString);
        connection.Open();

        using MySqlCommand command = new(
            "INSERT INTO Users (FullName, Email, PasswordHash) VALUES (@FullName, @Email, @PasswordHash);",
            connection);
        command.Parameters.AddWithValue("@FullName", user.FullName);
        command.Parameters.AddWithValue("@Email", user.Email);
        command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);

        command.ExecuteNonQuery();
    }

    // Finds the one user that matches this email
    public UserModel? GetByEmail(string email)
    {
        using MySqlConnection connection = new(connectionString);
        connection.Open();

        using MySqlCommand command = new(
            "SELECT UserID, FullName, Email, PasswordHash FROM Users WHERE Email = @Email;",
            connection);
        command.Parameters.AddWithValue("@Email", email);

        using MySqlDataReader reader = command.ExecuteReader();
        // No row means there is no user with this email so give back null
        if (!reader.Read())
        {
            return null;
        }

        // Copy each column from the row into a UserModel
        return new UserModel
        {
            UserId = reader.GetInt32("UserID"),
            FullName = reader.GetString("FullName"),
            Email = reader.GetString("Email"),
            PasswordHash = reader.GetString("PasswordHash")
        };
    }
}
