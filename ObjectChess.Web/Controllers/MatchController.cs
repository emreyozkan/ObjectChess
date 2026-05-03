using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ObjectChess.Web.ViewModels;
using ObjectChess.Business.Models;
using ObjectChess.Business.Interfaces;
using System;

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
            var newMatch = pageModel.NewMatch;

            _matchService.AddMatch(
                newMatch.WhitePlayer ?? "", 
                newMatch.BlackPlayer ?? "", 
                newMatch.Winner ?? "", 
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