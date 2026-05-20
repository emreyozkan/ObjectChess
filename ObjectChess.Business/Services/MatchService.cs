using System;
using System.Collections.Generic;
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

        public int GetTotalMatchCount(string playerName)
        {
            return _matchRepository.GetTotalMatchCount(playerName);
        }

        public List<MatchModel> GetPagedMatches(string playerName, int page, int pageSize)
        {
            return _matchRepository.GetPagedMatches(playerName, page, pageSize);
        }

        public List<MatchModel> GetAllMatches()
        {
            return _matchRepository.GetAllMatches();
        }

        public void AddMatch(string whitePlayer, string blackPlayer, string winner, DateTime matchDate)
        {
            _matchRepository.AddMatch(whitePlayer, blackPlayer, winner, matchDate);
        }

        public void AddMatchWithMoves(MatchModel model, List<string> rawMoves)
        {
            int currentMoveNumber = 1;
            for (int i = 0; i < rawMoves.Count; i++)
            {
                MoveModel move = new MoveModel
                {
                    MoveNumber = currentMoveNumber,
                    MoveText = rawMoves[i]
                };
                model.Moves.Add(move);

                if ((i + 1) % 2 == 0)
                {
                    currentMoveNumber++;
                }
            }

            _matchRepository.AddMatchWithMoves(model);
        }

        public void DeleteMatch(int gameId)
        {
            _matchRepository.DeleteMatch(gameId);
        }
    }
}