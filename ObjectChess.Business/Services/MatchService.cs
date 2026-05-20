using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

        private bool IsValidChessMove(string moveText)
        {
            if (string.IsNullOrEmpty(moveText)) return true;

            string pattern = @"^(?:[KQRBN]?[a-h]?[1-8]?x?[a-h][1-8](?:=[QRBN])?[+#]?|O-O(?:-O)?[+#]?)$";
            return Regex.IsMatch(moveText, pattern);
        }

        public List<MatchModel> GetAllMatches()
        {
            return _matchRepository.GetAllMatches();
        }

        public int GetTotalMatchCount()
        {
            return _matchRepository.GetTotalMatchCount();
        }

        public List<MatchModel> GetPagedMatches(int page, int pageSize)
        {
            return _matchRepository.GetPagedMatches(page, pageSize);
        }

        public void AddMatch(string whitePlayer, string blackPlayer, string winner, DateTime matchDate)
        {
            _matchRepository.AddMatch(whitePlayer, blackPlayer, winner, matchDate);
        }

        public void AddMatchWithMoves(MatchModel model, List<string> rawMoves)
        {
            if (rawMoves != null && rawMoves.Count > 0)
            {
                int counter = 1;
                foreach (string cleanMove in rawMoves)
                {
                    if (!IsValidChessMove(cleanMove))
                    {
                        throw new ArgumentException($"Invalid chess move detected: {cleanMove}");
                    }

                    model.Moves.Add(new MoveModel
                    {
                        MoveNumber = counter,
                        MoveText = cleanMove
                    });
                    counter++;
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