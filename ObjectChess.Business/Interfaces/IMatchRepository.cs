using System;
using System.Collections.Generic;
using ObjectChess.Business.Models;

namespace ObjectChess.Business.Interfaces
{
    public interface IMatchRepository
    {
        List<MatchModel> GetAllMatches();
        int GetTotalMatchCount();
        List<MatchModel> GetPagedMatches(int page, int pageSize);
        void AddMatch(string whitePlayer, string blackPlayer, string winner, DateTime matchDate);
        void AddMatchWithMoves(MatchModel model);
        void DeleteMatch(int gameId);
    }
}