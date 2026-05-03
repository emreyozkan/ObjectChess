using System;
using System.Collections.Generic;
using System.Linq;
using ObjectChess.Business.Models;
using ObjectChess.Business.Interfaces;

namespace ObjectChess.Business.Services
{
    public class MatchService : IMatchService
    {
        private readonly IMatchRepository _matchRepository;

        public MatchService(IMatchRepository matchRepository)
        {
            _matchRepository = matchRepository;
        }

        public List<MatchModel> GetAllMatches()
        {
            return _matchRepository.GetAllMatches();
        }

        public int GetTotalMatchCount()
        {
            return _matchRepository.GetAllMatches().Count;
        }

        public List<MatchModel> GetPagedMatches(int page, int pageSize)
        {
            return _matchRepository.GetAllMatches()
                .OrderByDescending(m => m.MatchDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public void AddMatch(string whitePlayer, string blackPlayer, string winner, DateTime matchDate)
        {
            _matchRepository.AddMatch(whitePlayer, blackPlayer, winner, matchDate);
        }

        public void DeleteMatch(int gameId)
        {
            _matchRepository.DeleteMatch(gameId);
        }
    }
}