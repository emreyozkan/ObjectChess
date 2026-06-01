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
    // Only logged-in users can access this controller
    [Authorize]
    public class MatchController : Controller
    {
        private readonly IMatchService _matchService;

        // Constructor: service is injected (Dependency Injection)
        public MatchController(IMatchService matchService)
        {
            _matchService = matchService;
        }

        // GET: Show match history page with pagination
        [HttpGet]
        public IActionResult MatchHistory(int page = 1)
        {
            // Get current logged-in user name from identity system
            string currentFullName = User.Identity?.Name ?? "";

            // Build full view model for the page
            MatchHistoryPageViewModel model = GetMatchHistoryViewModel(currentFullName, page);

            return View(model);
        }

        // POST: Add new match from form
        [HttpPost]
        public IActionResult AddMatch(MatchHistoryPageViewModel pageModel)
        {
            string currentFullName = User.Identity?.Name ?? "";

            // Get new match data from form
            AddMatchViewModel newMatch = pageModel.NewMatch;

            string whiteValue = newMatch.WhitePlayer ?? "";
            string blackValue = newMatch.BlackPlayer ?? "";
            string winnerValue = newMatch.Winner ?? "";
            string rawMoves = newMatch.RawMovesText ?? "";

            // Replace "Me" keyword with current user name
            if (whiteValue.Equals("Me", StringComparison.OrdinalIgnoreCase))
                whiteValue = currentFullName;

            if (blackValue.Equals("Me", StringComparison.OrdinalIgnoreCase))
                blackValue = currentFullName;

            if (winnerValue.Equals("Me", StringComparison.OrdinalIgnoreCase))
                winnerValue = currentFullName;

            // Create domain model object
            MatchModel matchModel = new MatchModel
            {
                WhitePlayer = whiteValue,
                BlackPlayer = blackValue,
                Winner = winnerValue,
                MatchDate = newMatch.MatchDate,

                // Moves will be filled in service layer
                Moves = new List<MoveModel>()
            };

            try
            {
                // Send data to service layer (business logic happens there)
                _matchService.AddMatch(matchModel, rawMoves, currentFullName);
            }
            catch (ArgumentException ex)
            {
                // If validation fails, show error on UI
                ModelState.AddModelError("NewMatch.RawMovesText", ex.Message);

                // Reload page with error message
                MatchHistoryPageViewModel errorModel = GetMatchHistoryViewModel(currentFullName, 1);
                errorModel.NewMatch = pageModel.NewMatch;

                return View("MatchHistory", errorModel);
            }

            // If success, refresh page
            return RedirectToAction("MatchHistory");
        }

        // POST: Delete a match
        [HttpPost]
        public IActionResult DeleteMatch(int gameId)
        {
            _matchService.DeleteMatch(gameId);
            return RedirectToAction("MatchHistory");
        }

        // Build full page view model (history + pagination + form)
        private MatchHistoryPageViewModel GetMatchHistoryViewModel(string playerName, int page)
        {
            int pageSize = 10;

            // Get total number of matches for pagination
            int totalMatches = _matchService.GetTotalMatchCount(playerName);

            // Get only current page matches
            List<MatchModel> pagedRawMatches = _matchService.GetPagedMatches(playerName, page, pageSize);

            // Convert domain models to view models (UI layer separation)
            List<MatchHistoryViewModel> historyList = pagedRawMatches
                .Select(item => new MatchHistoryViewModel
                {
                    GameID = item.GameID,
                    WhitePlayer = item.WhitePlayer,
                    BlackPlayer = item.BlackPlayer,
                    Winner = item.Winner,
                    MatchDate = item.MatchDate,

                    // Safety check in case moves is null
                    Moves = item.Moves ?? new List<MoveModel>()
                })
                .ToList();

            // Final page model sent to View
            return new MatchHistoryPageViewModel
            {
                Matches = historyList,
                CurrentPage = page,

                // Calculate total pages for pagination
                TotalPages = (int)Math.Ceiling(totalMatches / (double)pageSize),

                // Empty form model for new match input
                NewMatch = new AddMatchViewModel()
            };
        }
    }
}