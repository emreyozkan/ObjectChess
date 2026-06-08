using ObjectChess.Business.Interfaces;
using ObjectChess.Business.Models;

namespace ObjectChess.Tests.Fakes;

public class FakeMatchRepository : IMatchRepository
{
    private readonly List<MatchModel> _matches = [];
    private int _nextId = 1;

    public int GetTotalMatchCount(int userId)
    {
        return _matches.Count(match => match.UserId == userId);
    }

    public List<MatchModel> GetPagedMatches(int userId, int page, int pageSize)
    {
        return _matches
            .Where(match => match.UserId == userId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public MatchModel? GetMatch(int gameId, int userId)
    {
        return _matches.FirstOrDefault(match => match.GameId == gameId && match.UserId == userId);
    }

    public void AddMatch(MatchModel match)
    {
        if (match.GameId == 0)
        {
            match.GameId = _nextId++;
        }

        _matches.Add(match);
    }

    public void UpdateMatch(MatchModel match)
    {
        MatchModel? existing = _matches
            .FirstOrDefault(m => m.GameId == match.GameId && m.UserId == match.UserId);

        if (existing is null)
        {
            throw new InvalidOperationException("Match not found or not owned by the current user.");
        }

        existing.WhitePlayer = match.WhitePlayer;
        existing.BlackPlayer = match.BlackPlayer;
        existing.Winner = match.Winner;
        existing.MatchDate = match.MatchDate;
        existing.Moves = match.Moves;
    }

    public void DeleteMatch(int gameId, int userId)
    {
        MatchModel? match = _matches
            .FirstOrDefault(m => m.GameId == gameId && m.UserId == userId);

        if (match is not null)
        {
            _matches.Remove(match);
        }
    }
}
