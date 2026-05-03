using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ObjectChess.Web.ViewModels;
using ObjectChess.Business.Services;
using ObjectChess.Business.Models;
using System;

namespace ObjectChess.Web.Controllers
{
    public class MatchController : Controller
    {
        private readonly MatchService _matchService;

        public MatchController(MatchService matchService)
        {
            _matchService = matchService;
        }

        [HttpGet]
        public IActionResult MatchHistory(int page = 1)
        {
            int pageSize = 10;
            List<MatchModel> rawMatches = _matchService.GetAllMatches();
            int totalMatches = rawMatches.Count;

            List<MatchHistoryViewModel> historyList = rawMatches
                .OrderByDescending(m => m.MatchDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(item => new MatchHistoryViewModel 
                {
                    GameID = item.GameID,
                    WhitePlayer = item.WhitePlayer,
                    BlackPlayer = item.BlackPlayer,
                    Winner = item.Winner,
                    MatchDate = item.MatchDate
                }).ToList();

            MatchHistoryPageViewModel pageModel = new MatchHistoryPageViewModel
            {
                Matches = historyList,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalMatches / (double)pageSize)
            };

            return View(pageModel);
        }

        [HttpPost]
        public IActionResult AddMatch(MatchHistoryPageViewModel pageModel)
        {
            AddMatchViewModel newMatch = pageModel.NewMatch;

            _matchService.AddMatch(
                newMatch.WhitePlayer, 
                newMatch.BlackPlayer, 
                newMatch.Winner, 
                newMatch.MatchDate
            );

            return RedirectToAction("MatchHistory");
        }

        [HttpPost]
        public IActionResult DeleteMatch(int gameId)
        {
            _matchService.DeleteMatch(gameId);

            return RedirectToAction("MatchHistory");
        }
    }
}