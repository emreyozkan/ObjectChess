using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using ObjectChess.Business.Models;
using ObjectChess.Business.Interfaces;

namespace ObjectChess.Data.Repositories
{
    // This class is responsible for all database operations about matches
    public class MatchRepository : IMatchRepository
    {
        private readonly string _connectionString;

        public MatchRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int GetTotalMatchCount(string playerName)
        {
            using MySqlConnection connection = new MySqlConnection(_connectionString);
            connection.Open();

            // Count all games where player is either white or black
            string query = @"SELECT COUNT(*) FROM Games g 
                             JOIN Players w ON g.WhitePlayerID = w.PlayerID 
                             JOIN Players b ON g.BlackPlayerID = b.PlayerID 
                             WHERE w.FullName = @Name OR b.FullName = @Name;";

            using MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Name", playerName);

            object result = command.ExecuteScalar();

            // If no result, return 0 to avoid errors
            if (result == null || result == DBNull.Value)
                return 0;

            return Convert.ToInt32(result);
        }

        public List<MatchModel> GetPagedMatches(string playerName, int page, int pageSize)
        {
            List<MatchModel> matches = new List<MatchModel>();

            using MySqlConnection connection = new MySqlConnection(_connectionString);
            connection.Open();

            // Get matches for a player with pagination
            string query = @"SELECT g.GameID, w.FullName AS WhitePlayer, b.FullName AS BlackPlayer, 
                                    win.FullName AS Winner, g.MatchDate 
                             FROM Games g
                             JOIN Players w ON g.WhitePlayerID = w.PlayerID
                             JOIN Players b ON g.BlackPlayerID = b.PlayerID
                             LEFT JOIN Players win ON g.WinnerID = win.PlayerID
                             WHERE w.FullName = @Name OR b.FullName = @Name
                             ORDER BY g.MatchDate DESC 
                             LIMIT @Limit OFFSET @Offset;";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Name", playerName);
                command.Parameters.AddWithValue("@Limit", pageSize);
                command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);

                using MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    MatchModel match = new MatchModel
                    {
                        GameID = reader.GetInt32("GameID"),

                        // Handle null database values safely
                        WhitePlayer = reader.IsDBNull(reader.GetOrdinal("WhitePlayer")) ? "" : reader.GetString("WhitePlayer"),
                        BlackPlayer = reader.IsDBNull(reader.GetOrdinal("BlackPlayer")) ? "" : reader.GetString("BlackPlayer"),

                        // If no winner, it is a draw
                        Winner = reader.IsDBNull(reader.GetOrdinal("Winner")) ? "Draw" : reader.GetString("Winner"),

                        MatchDate = reader.GetDateTime("MatchDate"),

                        // Moves are loaded in a separate query
                        Moves = new List<MoveModel>()
                    };

                    matches.Add(match);
                }
            }

            // IMPORTANT:
            // We load moves separately for each match (this can be slow for big data)
            foreach (MatchModel match in matches)
            {
                match.Moves = GetMovesByGameId(match.GameID, connection);
            }

            return matches;
        }

        private List<MoveModel> GetMovesByGameId(int gameId, MySqlConnection connection)
        {
            List<MoveModel> moves = new List<MoveModel>();

            // Get all moves for a specific game
            string query = @"SELECT MoveNumber, MoveText 
                             FROM Moves 
                             WHERE GameID = @GameID 
                             ORDER BY MoveNumber;";

            using MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@GameID", gameId);

            using MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                MoveModel move = new MoveModel
                {
                    // If null, default to 0
                    MoveNumber = reader.IsDBNull(reader.GetOrdinal("MoveNumber"))
                        ? 0
                        : reader.GetInt32("MoveNumber"),

                    // If null, use empty string
                    MoveText = reader.IsDBNull(reader.GetOrdinal("MoveText"))
                        ? ""
                        : reader.GetString("MoveText")
                };

                moves.Add(move);
            }

            return moves;
        }

        public void AddMatch(MatchModel model)
        {
            model.Moves ??= new List<MoveModel>();

            using MySqlConnection connection = new MySqlConnection(_connectionString);
            connection.Open();

            using MySqlTransaction transaction = connection.BeginTransaction();

            try
            {
                // Make sure both players exist before inserting game
                int whiteId = EnsurePlayerExists(connection, transaction, model.WhitePlayer);
                int blackId = EnsurePlayerExists(connection, transaction, model.BlackPlayer);

                // Winner can be null (draw case)
                int? winnerId = string.IsNullOrWhiteSpace(model.Winner)
                    ? null
                    : (int?)EnsurePlayerExists(connection, transaction, model.Winner);

                string query = @"INSERT INTO Games 
                                (WhitePlayerID, BlackPlayerID, WinnerID, MatchDate) 
                                VALUES (@W, @B, @Win, @Date); 
                                SELECT LAST_INSERT_ID();";

                using MySqlCommand cmd = new MySqlCommand(query, connection, transaction);
                cmd.Parameters.AddWithValue("@W", whiteId);
                cmd.Parameters.AddWithValue("@B", blackId);
                cmd.Parameters.AddWithValue("@Win", (object?)winnerId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Date", model.MatchDate);

                object result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                    throw new Exception("Failed to insert game.");

                int gameId = Convert.ToInt32(result);

                // Insert all moves of this match
                foreach (MoveModel move in model.Moves)
                {
                    if (move == null) continue;

                    string moveQuery = @"INSERT INTO Moves (GameID, MoveNumber, MoveText) 
                                         VALUES (@GID, @Num, @Text);";

                    using MySqlCommand moveCmd = new MySqlCommand(moveQuery, connection, transaction);
                    moveCmd.Parameters.AddWithValue("@GID", gameId);
                    moveCmd.Parameters.AddWithValue("@Num", move.MoveNumber);
                    moveCmd.Parameters.AddWithValue("@Text", move.MoveText ?? "");

                    moveCmd.ExecuteNonQuery();
                }

                // Save everything if no error
                transaction.Commit();
            }
            catch
            {
                // If something fails, undo everything
                transaction.Rollback();
                throw;
            }
        }

        private int EnsurePlayerExists(MySqlConnection conn, MySqlTransaction trans, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Player name cannot be empty.");

            // Check if player already exists
            string query = "SELECT PlayerID FROM Players WHERE FullName = @Name;";
            using MySqlCommand cmd = new MySqlCommand(query, conn, trans);
            cmd.Parameters.AddWithValue("@Name", name);

            object result = cmd.ExecuteScalar();

            if (result != null && result != DBNull.Value)
                return Convert.ToInt32(result);

            // If not found, create new player
            string insert = "INSERT INTO Players (FullName) VALUES (@Name); SELECT LAST_INSERT_ID();";
            using MySqlCommand insCmd = new MySqlCommand(insert, conn, trans);
            insCmd.Parameters.AddWithValue("@Name", name);

            object insertResult = insCmd.ExecuteScalar();

            if (insertResult == null || insertResult == DBNull.Value)
                throw new Exception("Failed to insert player.");

            return Convert.ToInt32(insertResult);
        }

        public void DeleteMatch(int gameId)
        {
            using MySqlConnection connection = new MySqlConnection(_connectionString);
            connection.Open();

            // Delete match by its ID
            using MySqlCommand command = new MySqlCommand("DELETE FROM Games WHERE GameID = @ID;", connection);
            command.Parameters.AddWithValue("@ID", gameId);

            command.ExecuteNonQuery();
        }

        public List<MatchModel> GetAllMatches()
        {
            // Just reuse pagination method to get everything
            return GetPagedMatches("", 1, int.MaxValue);
        }
    }
}