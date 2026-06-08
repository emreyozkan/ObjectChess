using ObjectChess.Business.Models;

namespace ObjectChess.Business.Interfaces;

public interface IMatchService
{
    int GetTotalMatchCount(int userId);
    List<MatchModel> GetPagedMatches(int userId, int page, int pageSize);
    MatchModel? GetMatch(int gameId, int userId);
    void AddMatch(MatchModel match, string rawMoves, string currentUserFullName);
    void UpdateMatch(MatchModel match, string rawMoves, string currentUserFullName);
    void DeleteMatch(int gameId, int userId);
}
