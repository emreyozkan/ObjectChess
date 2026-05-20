using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; 
using ObjectChess.Web.ViewModels;
using ObjectChess.Business.Models;
using ObjectChess.Business.Interfaces;

namespace ObjectChess.Web.Controllers
{
    [Authorize] 
    public class MatchController : Controller
    {
        private readonly IMatchService _matchService;

        public MatchController(IMatchService matchService)
        {
            _matchService = matchService;
        }

        [HttpGet]
        public IActionResult MatchHistory(int page = 1)
        {
            string currentFullName = User.Identity?.Name ?? "";
            MatchHistoryPageViewModel model = GetMatchHistoryViewModel(currentFullName, page);
            return View(model);
        }

        [HttpPost]
        public IActionResult AddMatch(MatchHistoryPageViewModel pageModel)
        {
            string currentFullName = User.Identity?.Name ?? "";
            AddMatchViewModel newMatch = pageModel.NewMatch;

            string whiteValue = newMatch.WhitePlayer ?? "";
            string blackValue = newMatch.BlackPlayer ?? "";
            string winnerValue = newMatch.Winner ?? "";

            if (whiteValue.Equals("Me", StringComparison.OrdinalIgnoreCase))
            {
                whiteValue = currentFullName;
            }
            if (blackValue.Equals("Me", StringComparison.OrdinalIgnoreCase))
            {
                blackValue = currentFullName;
            }
            if (winnerValue.Equals("Me", StringComparison.OrdinalIgnoreCase))
            {
                winnerValue = currentFullName;
            }

            MatchModel matchModel = new MatchModel
            {
                WhitePlayer = whiteValue,
                BlackPlayer = blackValue,
                Winner = winnerValue,
                MatchDate = newMatch.MatchDate,
                Moves = new List<MoveModel>()
            };

            List<string> parsedMoves = new List<string>();

            if (!string.IsNullOrEmpty(newMatch.RawMovesText))
            {
                string normalizedText = newMatch.RawMovesText.Replace("\r\n", " ").Replace("\n", " ");
                string[] splitMoves = normalizedText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string moveStr in splitMoves)
                {
                    string cleanMove = moveStr.Trim();

                    if (cleanMove.EndsWith(".") || int.TryParse(cleanMove, out _))
                    {
                        continue;
                    }
                    parsedMoves.Add(cleanMove);
                }
            }

            try
            {
                _matchService.AddMatchWithMoves(matchModel, parsedMoves);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("NewMatch.RawMovesText", ex.Message);
                
                MatchHistoryPageViewModel errorModel = GetMatchHistoryViewModel(currentFullName, 1);
                errorModel.NewMatch = pageModel.NewMatch; 
                
                return View("MatchHistory", errorModel);
            }

            return RedirectToAction("MatchHistory");
        }

        [HttpPost]
        public IActionResult DeleteMatch(int gameId)
        {
            _matchService.DeleteMatch(gameId);
            return RedirectToAction("MatchHistory");
        }

        private MatchHistoryPageViewModel GetMatchHistoryViewModel(string playerName, int page)
        {
            int pageSize = 10;
            int totalMatches = _matchService.GetTotalMatchCount(playerName);
            List<MatchModel> pagedRawMatches = _matchService.GetPagedMatches(playerName, page, pageSize);

            List<MatchHistoryViewModel> historyList = pagedRawMatches
                .Select(item => new MatchHistoryViewModel 
                {
                    GameID = item.GameID,
                    WhitePlayer = item.WhitePlayer,
                    BlackPlayer = item.BlackPlayer,
                    Winner = item.Winner,
                    MatchDate = item.MatchDate,
                    Moves = item.Moves ?? new List<MoveModel>()
                }).ToList();

            return new MatchHistoryPageViewModel
            {
                Matches = historyList,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalMatches / (double)pageSize),
                NewMatch = new AddMatchViewModel() 
            };
        }
    }
}