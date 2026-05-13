using ObjectChess.Business.Interfaces;
using ObjectChess.Business.Models;
using System;
using System.Collections.Generic;

namespace ObjectChess.Tests.Fakes 
{
    public class FakeMatchRepository : IMatchRepository
    {
        public List<MatchModel> GetAllMatches()
        {
            return new List<MatchModel>
            {
                new MatchModel { GameID = 1, WhitePlayer = "Magnus", BlackPlayer = "Hikaru" },
                new MatchModel { GameID = 2, WhitePlayer = "Anish", BlackPlayer = "Jorden" }
            };
        }

        public void AddMatch(string whitePlayer, string blackPlayer, string winner, DateTime matchDate)
        {
        }

        public void DeleteMatch(int gameId)
        {
        }
    }
}