using MySql.Data.MySqlClient;
using ObjectChess.Business.Interfaces;
using ObjectChess.Business.Models;

namespace ObjectChess.Data.Repositories;

public class MatchRepository(string connectionString) : IMatchRepository
{
    // Counts how many matches this user has in total
    public int GetTotalMatchCount(int userId)
    {
        using MySqlConnection connection = new(connectionString);
        connection.Open();

        using MySqlCommand command = new(
            "SELECT COUNT(*) FROM Games WHERE UserID = @UserId;", connection);
        command.Parameters.AddWithValue("@UserId", userId);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public List<MatchModel> GetPagedMatches(int userId, int page, int pageSize)
    {
        List<MatchModel> matches = [];

        using MySqlConnection connection = new(connectionString);
        connection.Open();

        using (MySqlCommand command = new(
            @"SELECT GameID, WhitePlayer, BlackPlayer, Winner, MatchDate
              FROM Games
              WHERE UserID = @UserId
              ORDER BY MatchDate DESC
              LIMIT @Limit OFFSET @Offset;", connection))
        {
            command.Parameters.AddWithValue("@UserId", userId);
            // LIMIT is how many rows per page
            // OFFSET is how many rows to skip to reach the right page
            command.Parameters.AddWithValue("@Limit", pageSize);
            command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);

            using MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                matches.Add(MapMatch(reader));
            }
        }

        // Grab all the moves for this page in one single query
        // Then hand each match its own moves
        // This means 2 queries total instead of 1 query per match (the N+1 problem)
        List<int> gameIds = matches.Select(m => m.GameId).ToList();
        Dictionary<int, List<MoveModel>> movesByGame = GetMovesForGames(gameIds, connection);

        foreach (MatchModel match in matches)
        {
            match.Moves = movesByGame.TryGetValue(match.GameId, out List<MoveModel>? moves)
                ? moves
                : [];
        }

        return matches;
    }

    public MatchModel? GetMatch(int gameId, int userId)
    {
        using MySqlConnection connection = new(connectionString);
        connection.Open();

        MatchModel? match = null;

        using (MySqlCommand command = new(
            @"SELECT GameID, WhitePlayer, BlackPlayer, Winner, MatchDate
              FROM Games
              WHERE GameID = @GameId AND UserID = @UserId;", connection))
        {
            // Checking UserID too means you can only open a match that belongs to you
            command.Parameters.AddWithValue("@GameId", gameId);
            command.Parameters.AddWithValue("@UserId", userId);

            using MySqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                match = MapMatch(reader);
            }
        }

        if (match is not null)
        {
            match.Moves = GetMoves(match.GameId, connection);
        }

        return match;
    }

    public void AddMatch(MatchModel match)
    {
        using MySqlConnection connection = new(connectionString);
        connection.Open();

        // A transaction means the game and its moves all save together
        // Either all of them save or none of them do so there is no half saved match
        using MySqlTransaction transaction = connection.BeginTransaction();
        try
        {
            using (MySqlCommand command = new(
                @"INSERT INTO Games (UserID, WhitePlayer, BlackPlayer, Winner, MatchDate)
                  VALUES (@UserId, @White, @Black, @Winner, @Date);
                  SELECT LAST_INSERT_ID();", connection, transaction))
            {
                command.Parameters.AddWithValue("@UserId", match.UserId);
                command.Parameters.AddWithValue("@White", match.WhitePlayer);
                command.Parameters.AddWithValue("@Black", match.BlackPlayer);
                // Winner can be empty for a draw so send a real db NULL in that case
                command.Parameters.AddWithValue("@Winner", (object?)match.Winner ?? DBNull.Value);
                command.Parameters.AddWithValue("@Date", match.MatchDate);

                // Grab the new GameID the database made so we can attach moves to it
                match.GameId = Convert.ToInt32(command.ExecuteScalar());
            }

            InsertMoves(match, connection, transaction);
            transaction.Commit();
        }
        catch
        {
            // Something went wrong so undo everything and leave no broken half saved match
            transaction.Rollback();
            throw;
        }
    }

    public void UpdateMatch(MatchModel match)
    {
        using MySqlConnection connection = new(connectionString);
        connection.Open();

        using MySqlTransaction transaction = connection.BeginTransaction();
        try
        {
            int affectedRows;
            using (MySqlCommand command = new(
                @"UPDATE Games
                  SET WhitePlayer = @White, BlackPlayer = @Black, Winner = @Winner, MatchDate = @Date
                  WHERE GameID = @GameId AND UserID = @UserId;", connection, transaction))
            {
                command.Parameters.AddWithValue("@White", match.WhitePlayer);
                command.Parameters.AddWithValue("@Black", match.BlackPlayer);
                command.Parameters.AddWithValue("@Winner", (object?)match.Winner ?? DBNull.Value);
                command.Parameters.AddWithValue("@Date", match.MatchDate);
                command.Parameters.AddWithValue("@GameId", match.GameId);
                command.Parameters.AddWithValue("@UserId", match.UserId);

                affectedRows = command.ExecuteNonQuery();
            }

            // If nothing got updated then the match does not exist or is not this user's
            // So stop here instead of continuing
            if (affectedRows == 0)
            {
                throw new InvalidOperationException("Match not found or not owned by the current user.");
            }

            // Easiest way to update the moves is to delete the old ones and add the new list
            using (MySqlCommand deleteMoves = new(
                "DELETE FROM Moves WHERE GameID = @GameId;", connection, transaction))
            {
                deleteMoves.Parameters.AddWithValue("@GameId", match.GameId);
                deleteMoves.ExecuteNonQuery();
            }

            InsertMoves(match, connection, transaction);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void DeleteMatch(int gameId, int userId)
    {
        using MySqlConnection connection = new(connectionString);
        connection.Open();

        // Again checking UserID so you can only delete your own match and not someone else's
        using MySqlCommand command = new(
            "DELETE FROM Games WHERE GameID = @GameId AND UserID = @UserId;", connection);
        command.Parameters.AddWithValue("@GameId", gameId);
        command.Parameters.AddWithValue("@UserId", userId);

        command.ExecuteNonQuery();
    }

    // Adds every move of this match into the Moves table
    private static void InsertMoves(MatchModel match, MySqlConnection connection, MySqlTransaction transaction)
    {
        foreach (MoveModel move in match.Moves)
        {
            using MySqlCommand command = new(
                "INSERT INTO Moves (GameID, MoveNumber, MoveText) VALUES (@GameId, @Number, @Text);",
                connection, transaction);
            command.Parameters.AddWithValue("@GameId", match.GameId);
            command.Parameters.AddWithValue("@Number", move.MoveNumber);
            command.Parameters.AddWithValue("@Text", move.MoveText);

            command.ExecuteNonQuery();
        }
    }

    // Turns one database row into a MatchModel object
    private static MatchModel MapMatch(MySqlDataReader reader)
    {
        return new MatchModel
        {
            GameId = reader.GetInt32("GameID"),
            WhitePlayer = reader.GetString("WhitePlayer"),
            BlackPlayer = reader.GetString("BlackPlayer"),
            // Winner is null in the db when the game was a draw so handle that instead of crashing
            Winner = reader.IsDBNull(reader.GetOrdinal("Winner")) ? null : reader.GetString("Winner"),
            MatchDate = reader.GetDateTime("MatchDate")
        };
    }

    private static Dictionary<int, List<MoveModel>> GetMovesForGames(List<int> gameIds, MySqlConnection connection)
    {
        Dictionary<int, List<MoveModel>> result = [];

        // No matches on this page means there is nothing to load so just stop here
        if (gameIds.Count == 0)
        {
            return result;
        }

        using MySqlCommand command = new() { Connection = connection };

        // Build one parameter per id like @id0 and @id1 instead of pasting the ids into the text
        // This keeps the IN list safe from SQL injection
        string[] parameterNames = new string[gameIds.Count];
        for (int i = 0; i < gameIds.Count; i++)
        {
            parameterNames[i] = $"@id{i}";
            command.Parameters.AddWithValue(parameterNames[i], gameIds[i]);
        }

        command.CommandText =
            $"SELECT GameID, MoveNumber, MoveText FROM Moves " +
            $"WHERE GameID IN ({string.Join(", ", parameterNames)}) " +
            $"ORDER BY GameID, MoveNumber, MoveID;";

        using MySqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            int gameId = reader.GetInt32("GameID");

            // Put each move into the bucket for its own game
            if (!result.TryGetValue(gameId, out List<MoveModel>? moves))
            {
                moves = [];
                result[gameId] = moves;
            }

            moves.Add(new MoveModel
            {
                MoveNumber = reader.GetInt32("MoveNumber"),
                MoveText = reader.GetString("MoveText")
            });
        }

        return result;
    }

    // Loads all the moves for one single game in order
    private static List<MoveModel> GetMoves(int gameId, MySqlConnection connection)
    {
        List<MoveModel> moves = [];

        using MySqlCommand command = new(
            "SELECT MoveNumber, MoveText FROM Moves WHERE GameID = @GameId ORDER BY MoveNumber, MoveID;",
            connection);
        command.Parameters.AddWithValue("@GameId", gameId);

        using MySqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            moves.Add(new MoveModel
            {
                MoveNumber = reader.GetInt32("MoveNumber"),
                MoveText = reader.GetString("MoveText")
            });
        }

        return moves;
    }
}
