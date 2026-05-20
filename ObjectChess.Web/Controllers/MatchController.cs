using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ObjectChess.Web.ViewModels;
using ObjectChess.Business.Models;
using ObjectChess.Business.Interfaces;

namespace ObjectChess.Web.Controllers
{
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
            MatchHistoryPageViewModel model = GetMatchHistoryViewModel(page);
            return View(model);
        }

        [HttpPost]
        public IActionResult AddMatch(MatchHistoryPageViewModel pageModel)
        {
            AddMatchViewModel newMatch = pageModel.NewMatch;

            MatchModel matchModel = new MatchModel
            {
                WhitePlayer = newMatch.WhitePlayer ?? "",
                BlackPlayer = newMatch.BlackPlayer ?? "",
                Winner = newMatch.Winner ?? "",
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
                
                MatchHistoryPageViewModel errorModel = GetMatchHistoryViewModel(1);
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

        private MatchHistoryPageViewModel GetMatchHistoryViewModel(int page)
        {
            int pageSize = 10;
            int totalMatches = _matchService.GetTotalMatchCount();
            List<MatchModel> pagedRawMatches = _matchService.GetPagedMatches(page, pageSize);

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