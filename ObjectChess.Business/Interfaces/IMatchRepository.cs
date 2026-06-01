using System;
using System.Collections.Generic;
using ObjectChess.Business.Models;

namespace ObjectChess.Business.Interfaces
{
    public interface IMatchRepository
    {
        int GetTotalMatchCount(string playerName);
        List<MatchModel> GetPagedMatches(string playerName, int page, int pageSize);
        List<MatchModel> GetAllMatches();
        void AddMatch(MatchModel model);
        void DeleteMatch(int gameId);
    }
}