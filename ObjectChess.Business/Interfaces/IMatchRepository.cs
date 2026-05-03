using System;
using System.Collections.Generic;
using ObjectChess.Business.Models; 

namespace ObjectChess.Business.Interfaces
{
    public interface IMatchRepository
    {
        List<MatchModel> GetAllMatches();
        void AddMatch(string whitePlayer, string blackPlayer, string winner, DateTime matchDate);
        void DeleteMatch(int gameId);
    }
}