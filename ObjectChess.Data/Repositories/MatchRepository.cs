using System;
using System.Collections.Generic;
using System.Linq;
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

        public int GetTotalMatchCount()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM Games;";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Database error while counting matches.", ex);
            }
        }

        public List<MatchModel> GetPagedMatches(int page, int pageSize)
        {
            List<MatchModel> pagedMatches = new List<MatchModel>();
            int offset = (page - 1) * pageSize;

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    string matchQuery = @"
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
                        ORDER BY g.MatchDate DESC
                        LIMIT @Limit OFFSET @Offset;";

                    using (MySqlCommand command = new MySqlCommand(matchQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Limit", pageSize);
                        command.Parameters.AddWithValue("@Offset", offset);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                MatchModel match = new MatchModel
                                {
                                    GameID = Convert.ToInt32(reader["GameID"]),
                                    WhitePlayer = reader["WhitePlayer"]?.ToString() ?? "",
                                    BlackPlayer = reader["BlackPlayer"]?.ToString() ?? "",
                                    MatchDate = Convert.ToDateTime(reader["MatchDate"]),
                                    Winner = reader["Winner"] != DBNull.Value ? reader["Winner"]?.ToString() ?? "Draw" : "Draw",
                                    Moves = new List<MoveModel>()
                                };
                                pagedMatches.Add(match);
                            }
                        }
                    }

                    if (pagedMatches.Any())
                    {
                        List<int> gameIds = pagedMatches.Select(m => m.GameID).ToList();
                        string inClause = string.Join(",", gameIds);

                        string moveQuery = $"SELECT MoveID, GameID, MoveNumber, MoveText FROM Moves WHERE GameID IN ({inClause}) ORDER BY MoveNumber ASC;";

                        using (MySqlCommand moveCommand = new MySqlCommand(moveQuery, connection))
                        {
                            using (MySqlDataReader moveReader = moveCommand.ExecuteReader())
                            {
                                while (moveReader.Read())
                                {
                                    int gameId = Convert.ToInt32(moveReader["GameID"]);
                                    MatchModel? parentMatch = pagedMatches.FirstOrDefault(m => m.GameID == gameId);

                                    if (parentMatch != null)
                                    {
                                        parentMatch.Moves.Add(new MoveModel
                                        {
                                            MoveID = Convert.ToInt32(moveReader["MoveID"]),
                                            GameID = gameId,
                                            MoveNumber = Convert.ToInt32(moveReader["MoveNumber"]),
                                            MoveText = moveReader["MoveText"]?.ToString() ?? ""
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Database error while retrieving paged matches and moves.", ex);
            }

            return pagedMatches;
        }

        public List<MatchModel> GetAllMatches()
        {
            return GetPagedMatches(1, int.MaxValue); 
        }

        private int GetOrCreatePlayer(MySqlConnection connection, MySqlTransaction transaction, string username)
        {
            string checkQuery = "SELECT PlayerID FROM Players WHERE Username = @Username;";
            using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection, transaction))
            {
                checkCmd.Parameters.AddWithValue("@Username", username);
                object? result = checkCmd.ExecuteScalar();
                
                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
            }

            string insertQuery = "INSERT INTO Players (Username) VALUES (@Username); SELECT LAST_INSERT_ID();";
            using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection, transaction))
            {
                insertCmd.Parameters.AddWithValue("@Username", username);
                object? newId = insertCmd.ExecuteScalar();
                return newId != null ? Convert.ToInt32(newId) : 0;
            }
        }

        public void AddMatch(string whitePlayer, string blackPlayer, string winner, DateTime matchDate)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            int whiteId = GetOrCreatePlayer(connection, transaction, whitePlayer);
                            int blackId = GetOrCreatePlayer(connection, transaction, blackPlayer);
                            int? winnerId = !string.IsNullOrEmpty(winner) && winner != "Draw" ? GetOrCreatePlayer(connection, transaction, winner) : null;
                            
                            string query = "INSERT INTO Games (WhitePlayerID, BlackPlayerID, WinnerID, MatchDate) VALUES (@WhiteID, @BlackID, @WinnerID, @Date);";

                            using (MySqlCommand command = new MySqlCommand(query, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@WhiteID", whiteId);
                                command.Parameters.AddWithValue("@BlackID", blackId);
                                command.Parameters.AddWithValue("@WinnerID", (object?)winnerId ?? DBNull.Value);
                                command.Parameters.AddWithValue("@Date", matchDate);
                                command.ExecuteNonQuery();
                            }
                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Database error while adding a match.", ex);
            }
        }

        public void AddMatchWithMoves(MatchModel model)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            int whiteId = GetOrCreatePlayer(connection, transaction, model.WhitePlayer);
                            int blackId = GetOrCreatePlayer(connection, transaction, model.BlackPlayer);
                            int? winnerId = !string.IsNullOrEmpty(model.Winner) && model.Winner != "Draw" ? GetOrCreatePlayer(connection, transaction, model.Winner) : null;
                            
                            string matchQuery = "INSERT INTO Games (WhitePlayerID, BlackPlayerID, WinnerID, MatchDate) VALUES (@WhiteID, @BlackID, @WinnerID, @Date); SELECT LAST_INSERT_ID();";

                            int insertedGameId = 0;
                            using (MySqlCommand command = new MySqlCommand(matchQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@WhiteID", whiteId);
                                command.Parameters.AddWithValue("@BlackID", blackId);
                                command.Parameters.AddWithValue("@WinnerID", (object?)winnerId ?? DBNull.Value);
                                command.Parameters.AddWithValue("@Date", model.MatchDate);
                                insertedGameId = Convert.ToInt32(command.ExecuteScalar());
                            }

                            if (model.Moves != null && model.Moves.Count > 0)
                            {
                                string moveQuery = "INSERT INTO Moves (GameID, MoveNumber, MoveText) VALUES (@GameID, @MoveNumber, @MoveText);";
                                foreach (MoveModel move in model.Moves)
                                {
                                    using (MySqlCommand moveCommand = new MySqlCommand(moveQuery, connection, transaction))
                                    {
                                        moveCommand.Parameters.AddWithValue("@GameID", insertedGameId);
                                        moveCommand.Parameters.AddWithValue("@MoveNumber", move.MoveNumber);
                                        moveCommand.Parameters.AddWithValue("@MoveText", move.MoveText);
                                        moveCommand.ExecuteNonQuery();
                                    }
                                }
                            }
                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Database error while adding a match with moves.", ex);
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
            catch (Exception ex)
            {
                throw new Exception("Database error while deleting the match.", ex);
            }
        }
    }
}