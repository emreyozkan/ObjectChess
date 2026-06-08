using ObjectChess.Business.Models;

namespace ObjectChess.Business.Interfaces;

public interface IMatchRepository
{
    int GetTotalMatchCount(int userId);
    List<MatchModel> GetPagedMatches(int userId, int page, int pageSize);
    MatchModel? GetMatch(int gameId, int userId);
    void AddMatch(MatchModel match);
    void UpdateMatch(MatchModel match);
    void DeleteMatch(int gameId, int userId);
}
