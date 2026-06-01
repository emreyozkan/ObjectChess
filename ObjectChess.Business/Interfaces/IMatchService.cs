using System;
using System.Collections.Generic;
using ObjectChess.Business.Models;

namespace ObjectChess.Business.Interfaces
{
    public interface IMatchService
    {
        int GetTotalMatchCount(string playerName);
        List<MatchModel> GetPagedMatches(string playerName, int page, int pageSize);
        List<MatchModel> GetAllMatches();
        void AddMatch(MatchModel model, string rawMovesText, string currentUserName);
        void DeleteMatch(int gameId);
    }
}