using System;
using System.Collections.Generic;
using System.Linq;
using ObjectChess.Business.Interfaces;
using ObjectChess.Business.Models;

namespace ObjectChess.Tests.Fakes
{
    public class FakeMatchRepository : IMatchRepository
    {
        private readonly List<MatchModel> _matches = new List<MatchModel>();

        public int GetTotalMatchCount(string currentUserEmail)
        {
            return _matches.Count(m => m.WhitePlayer == currentUserEmail || m.BlackPlayer == currentUserEmail);
        }

        public List<MatchModel> GetPagedMatches(string currentUserEmail, int offset, int pageSize)
        {
            return _matches
                .Where(m => m.WhitePlayer == currentUserEmail || m.BlackPlayer == currentUserEmail)
                .Skip(offset)
                .Take(pageSize)
                .ToList();
        }

        public void AddMatch(MatchModel match)
        {
            _matches.Add(match);
        }

        public void DeleteMatch(int gameId)
        {
            MatchModel? match = _matches.FirstOrDefault(m => m.GameID == gameId);
            if (match != null)
            {
                _matches.Remove(match);
            }
        }

        public List<MatchModel> GetAllMatches()
        {
            return _matches;
        }

        public void AddMatch(string whitePlayer, string blackPlayer, string winner, DateTime matchDate)
        {
            MatchModel match = new MatchModel
            {
                WhitePlayer = whitePlayer,
                BlackPlayer = blackPlayer,
                Winner = winner,
                MatchDate = matchDate,
                Moves = new List<MoveModel>()
            };
            _matches.Add(match);
        }
    }
}