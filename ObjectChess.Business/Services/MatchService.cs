using ObjectChess.Business.Interfaces;
using ObjectChess.Business.Models;

namespace ObjectChess.Business.Services;

public class MatchService(IMatchRepository matchRepository, IMoveParser moveParser) : IMatchService
{
    private const string MeKeyword = "Me";
    private const string DrawKeyword = "Draw";

    public int GetTotalMatchCount(int userId)
        => matchRepository.GetTotalMatchCount(userId);

    public List<MatchModel> GetPagedMatches(int userId, int page, int pageSize)
    {
        // Never let the page number go below one
        if (page < 1)
        {
            page = 1;
        }

        // Fall back to a default size if a bad page size comes in
        if (pageSize < 1)
        {
            pageSize = 10;
        }

        return matchRepository.GetPagedMatches(userId, page, pageSize);
    }

    public MatchModel? GetMatch(int gameId, int userId)
        => matchRepository.GetMatch(gameId, userId);

    public void AddMatch(MatchModel match, string rawMoves, string currentUserFullName)
    {
        PrepareMatch(match, rawMoves, currentUserFullName);
        matchRepository.AddMatch(match);
    }

    public void UpdateMatch(MatchModel match, string rawMoves, string currentUserFullName)
    {
        PrepareMatch(match, rawMoves, currentUserFullName);
        matchRepository.UpdateMatch(match);
    }

    public void DeleteMatch(int gameId, int userId)
        => matchRepository.DeleteMatch(gameId, userId);

    // Shared steps used by both adding and editing a match
    // This way the rules stay the same in both places
    private void PrepareMatch(MatchModel match, string rawMoves, string currentUserFullName)
    {
        NormalizePlayerNames(match, currentUserFullName);
        ValidateMatchRules(match);
        match.Moves = moveParser.Parse(rawMoves ?? string.Empty);
    }

    private static void NormalizePlayerNames(MatchModel match, string currentUserFullName)
    {
        match.WhitePlayer = ResolveName(match.WhitePlayer, currentUserFullName);
        match.BlackPlayer = ResolveName(match.BlackPlayer, currentUserFullName);

        if (match.Winner is not null)
        {
            match.Winner = ResolveName(match.Winner, currentUserFullName);
        }
    }

    private static string ResolveName(string value, string currentUserFullName)
    {
        // Trim off any extra spaces around the name
        string trimmed = (value ?? string.Empty).Trim();
        // Little shortcut where if someone types "Me" we swap it for their own account name
        return trimmed.Equals(MeKeyword, StringComparison.OrdinalIgnoreCase)
            ? currentUserFullName
            : trimmed;
    }

    private static void ValidateMatchRules(MatchModel match)
    {
        // Both players must actually have a name
        if (string.IsNullOrWhiteSpace(match.WhitePlayer) ||
            string.IsNullOrWhiteSpace(match.BlackPlayer))
        {
            throw new ArgumentException("Both players are required.");
        }

        // Can not have the same person playing both sides
        if (match.WhitePlayer.Equals(match.BlackPlayer, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Players cannot be the same.");
        }

        // A game is a draw if no winner was given or they picked "Draw"
        bool isDraw = string.IsNullOrWhiteSpace(match.Winner) ||
                      match.Winner.Equals(DrawKeyword, StringComparison.OrdinalIgnoreCase);

        if (isDraw)
        {
            // Store a draw as null in the db instead of the word "Draw"
            match.Winner = null;
            return;
        }

        // If it is not a draw then the winner has to be one of the two players
        if (!match.Winner!.Equals(match.WhitePlayer, StringComparison.OrdinalIgnoreCase) &&
            !match.Winner.Equals(match.BlackPlayer, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Winner must be one of the two players or 'Draw'.");
        }
    }
}
