using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using ObjectChess.Business.Models; 
using ObjectChess.Business.Interfaces;

namespace ObjectChess.Data.Repositories
{
    public class MatchRepository : IMatchRepository
    {
        private readonly string _connectionString;

        public MatchRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<MatchModel> GetAllMatches()
        {
            List<MatchModel> matchHistoryList = new List<MatchModel>();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT 
                            g.GameID,
                            White.Username AS WhitePlayer, 
                            Black.Username AS BlackPlayer, 
                            Winner.Username AS Winner, 
                            g.MatchDate 
                        FROM Games g
                        JOIN Players White ON g.WhitePlayerID = White.PlayerID
                        JOIN Players Black ON g.BlackPlayerID = Black.PlayerID
                        LEFT JOIN Players Winner ON g.WinnerID = Winner.PlayerID
                        ORDER BY g.MatchDate DESC;";
                    
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                MatchModel match = new MatchModel();

                                match.GameID = Convert.ToInt32(reader["GameID"]);
                                match.WhitePlayer = reader["WhitePlayer"]?.ToString() ?? "";
                                match.BlackPlayer = reader["BlackPlayer"]?.ToString() ?? "";
                                match.MatchDate = Convert.ToDateTime(reader["MatchDate"]);

                                if (reader["Winner"] != DBNull.Value)
                                {
                                    match.Winner = reader["Winner"]?.ToString() ?? "Draw";
                                }
                                else
                                {
                                    match.Winner = "Draw";
                                }

                                matchHistoryList.Add(match);
                            }
                        }
                    }
                }
            }
            catch (MySqlException dbError)
            {
                throw new Exception("Database error while retrieving matches: " + dbError.Message, dbError);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while retrieving matches.", ex);
            }

            return matchHistoryList;
        }

        private int GetOrCreatePlayer(MySqlConnection connection, string username)
        {
            try
            {
                string checkQuery = "SELECT PlayerID FROM Players WHERE Username = @Username;";
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@Username", username);
                    object? result = checkCmd.ExecuteScalar();
                    
                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                }

                string insertQuery = "INSERT INTO Players (Username) VALUES (@Username); SELECT LAST_INSERT_ID();";
                using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection))
                {
                    insertCmd.Parameters.AddWithValue("@Username", username);
                    object? newId = insertCmd.ExecuteScalar();
                    return newId != null ? Convert.ToInt32(newId) : 0;
                }
            }
            catch (MySqlException dbError)
            {
                throw new Exception("Database error while verifying player: " + dbError.Message, dbError);
            }
        }

        public void AddMatch(string whitePlayer, string blackPlayer, string winner, DateTime matchDate)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    
                    int whiteId = GetOrCreatePlayer(connection, whitePlayer);
                    int blackId = GetOrCreatePlayer(connection, blackPlayer);
                    
                    int? winnerId = null;
                    if (!string.IsNullOrEmpty(winner) && winner != "Draw")
                    {
                        winnerId = GetOrCreatePlayer(connection, winner);
                    }
                    
                    string query = @"
                        INSERT INTO Games (WhitePlayerID, BlackPlayerID, WinnerID, MatchDate) 
                        VALUES (@WhiteID, @BlackID, @WinnerID, @Date);";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@WhiteID", whiteId);
                        command.Parameters.AddWithValue("@BlackID", blackId);
                        command.Parameters.AddWithValue("@WinnerID", (object?)winnerId ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Date", matchDate);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException dbError)
            {
                throw new Exception("Database error while adding a match: " + dbError.Message, dbError);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while adding a match.", ex);
            }
        }

        public void DeleteMatch(int gameId)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM Games WHERE GameID = @GameID;";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@GameID", gameId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException dbError)
            {
                throw new Exception("Database error while deleting the match: " + dbError.Message, dbError);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while deleting the match.", ex);
            }
        }
    }
}