using System;
using System.Collections.Generic;
using ObjectChess.Business.Models;
using ObjectChess.Data.Entities;
using ObjectChess.Data.Repositories;

namespace ObjectChess.Business.Services
{
    public class MatchService
    {
        private readonly MatchRepository _matchRepository;

        public MatchService(string connectionString)
        {
            _matchRepository = new MatchRepository(connectionString);
        }

        public List<MatchModel> GetAllMatches()
        {
            List<MatchEntity> entities = _matchRepository.GetAllMatches();
            List<MatchModel> models = new List<MatchModel>();

            foreach (var entity in entities)
            {
                models.Add(new MatchModel
                {
                    GameID = entity.GameID,
                    WhitePlayer = entity.WhitePlayer ?? "", 
                    BlackPlayer = entity.BlackPlayer ?? "",
                    Winner = entity.Winner ?? "Draw",
                    MatchDate = entity.MatchDate
                });
            }

            return models;
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