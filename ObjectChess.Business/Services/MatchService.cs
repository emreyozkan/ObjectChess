using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ObjectChess.Business.Models;
using ObjectChess.Business.Interfaces;

namespace ObjectChess.Business.Services
{
    public class MatchService : IMatchService
    {
        private readonly IMatchRepository _repo;

        // Constructor: repository is injected here (Dependency Injection)
        public MatchService(IMatchRepository repo) => _repo = repo;

        // Get total matches of a player (by name, not ID yet)
        public int GetTotalMatchCount(string playerName) 
            => _repo.GetTotalMatchCount(playerName);

        // Get matches with pagination (pN = page number, p = page, s = size)
        public List<MatchModel> GetPagedMatches(string pN, int p, int s) 
            => _repo.GetPagedMatches(pN, p, s);

        // Get all matches (not recommended for big data)
        public List<MatchModel> GetAllMatches() 
            => _repo.GetAllMatches();

        // Delete a match by game ID
        public void DeleteMatch(int gameId) 
            => _repo.DeleteMatch(gameId);

        // Add a new match with validation + move parsing
        public void AddMatch(MatchModel model, string rawMoves, string currentName)
        {
            rawMoves = rawMoves ?? ""; // avoid null crash

            // Convert "Me" keyword into actual player name
            NormalizePlayerNames(model, currentName);

            // Check if match data is logically valid
            ValidateMatchRules(model);

            // Convert raw move text into structured MoveModel list
            ProcessMoves(model, rawMoves);

            // Save final match into database
            _repo.AddMatch(model);
        }

        // Replace "Me" with actual current player name
        private void NormalizePlayerNames(MatchModel m, string currentName)
        {
            if (m.WhitePlayer.Equals("Me", StringComparison.OrdinalIgnoreCase))
                m.WhitePlayer = currentName;

            if (m.BlackPlayer.Equals("Me", StringComparison.OrdinalIgnoreCase))
                m.BlackPlayer = currentName;

            if (m.Winner != null && m.Winner.Equals("Me", StringComparison.OrdinalIgnoreCase))
                m.Winner = currentName;
        }

        // Validate match rules (basic game logic checks)
        private void ValidateMatchRules(MatchModel m)
        {
            // same player cannot play against himself
            if (m.WhitePlayer.Equals(m.BlackPlayer, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Players cannot be the same.");

            // check if game is draw
            bool isDraw = string.IsNullOrEmpty(m.Winner) 
                || m.Winner.Equals("Draw", StringComparison.OrdinalIgnoreCase);

            // winner must be one of white or black player, cannot be a third one
            if (!isDraw &&
                !m.Winner.Equals(m.WhitePlayer, StringComparison.OrdinalIgnoreCase) &&
                !m.Winner.Equals(m.BlackPlayer, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Invalid winner.");

            // if draw, store winner as null in DB
            if (isDraw)
                m.Winner = null;
        }

        // Convert raw chess moves string into structured list
        private void ProcessMoves(MatchModel m, string raw)
        {
            m.Moves = new List<MoveModel>();

            // if no moves, just return empty list
            if (string.IsNullOrWhiteSpace(raw))
                return;

            // clean line breaks and split by spaces
            string[] tokens = raw
                .Replace("\r\n", " ")
                .Replace("\n", " ")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            int moveNum = 1;   // full move number (1,2,3...)
            int halfMoves = 0; // counts white + black moves

            // basic chess move validation regex
            string pattern = @"^(O-O(-O)?|[a-h][1-8]|[a-h]x[a-h][1-8]|[KQRBN][a-h1-8x]?[a-h][1-8](=[QRBN])?[+#]?)$";

            foreach (var token in tokens)
            {
                // skip move numbers like "1." or "2."
                if (token.EndsWith(".") || int.TryParse(token, out _))
                    continue;

                // validate move format
                if (!Regex.IsMatch(token, pattern))
                    throw new ArgumentException($"Invalid move: {token}");

                // add move to list
                m.Moves.Add(new MoveModel
                {
                    MoveNumber = moveNum,
                    MoveText = token
                });

                // after black move, increase full move number
                if (++halfMoves % 2 == 0)
                    moveNum++;
            }
        }
    }
}