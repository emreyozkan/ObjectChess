using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ObjectChess.Business.Interfaces;
using ObjectChess.Business.Models;
using ObjectChess.Web.ViewModels;

namespace ObjectChess.Web.Controllers;

[Authorize]
public class MatchController(IMatchService matchService) : Controller
{
    private const int PageSize = 10;

    // Who is logged in right now
    // We read it from the login cookie claims instead of trusting the form
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private string CurrentUserFullName => User.Identity?.Name ?? string.Empty;

    [HttpGet]
    public IActionResult MatchHistory(int page = 1)
    {
        return View(BuildPageViewModel(page));
    }

    [HttpPost]
    public IActionResult AddMatch(MatchHistoryPageViewModel pageModel)
    {
        AddMatchViewModel input = pageModel.NewMatch;

        // Build a match object from the form data
        MatchModel match = new()
        {
            // Stamp the match with the logged in user so it belongs to them
            UserId = CurrentUserId,
            WhitePlayer = input.WhitePlayer ?? string.Empty,
            BlackPlayer = input.BlackPlayer ?? string.Empty,
            Winner = input.Winner,
            MatchDate = input.MatchDate
        };

        try
        {
            matchService.AddMatch(match, input.RawMovesText ?? string.Empty, CurrentUserFullName);
        }
        catch (ArgumentException ex)
        {
            // A bad move or bad input throws so we show that message on the form instead of crashing
            ModelState.AddModelError("NewMatch.RawMovesText", ex.Message);

            MatchHistoryPageViewModel errorModel = BuildPageViewModel(1);
            errorModel.NewMatch = input;
            return View("MatchHistory", errorModel);
        }

        return RedirectToAction("MatchHistory");
    }

    [HttpGet]
    public IActionResult EditMatch(int id)
    {
        // Pass CurrentUserId so people can not edit a match that is not theirs
        // Even if they type a random id into the url
        MatchModel? match = matchService.GetMatch(id, CurrentUserId);

        // Either the match is missing or it is not theirs so say not found
        if (match is null)
        {
            return NotFound();
        }

        EditMatchViewModel model = new()
        {
            GameId = match.GameId,
            WhitePlayer = match.WhitePlayer,
            BlackPlayer = match.BlackPlayer,
            Winner = match.Winner ?? "Draw",
            MatchDate = match.MatchDate,
            RawMovesText = string.Join(" ", match.Moves.Select(move => move.MoveText))
        };

        return View(model);
    }

    [HttpPost]
    public IActionResult EditMatch(EditMatchViewModel model)
    {
        // If the form broke a rule then show it again with the errors
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        MatchModel match = new()
        {
            GameId = model.GameId,
            UserId = CurrentUserId,
            WhitePlayer = model.WhitePlayer ?? string.Empty,
            BlackPlayer = model.BlackPlayer ?? string.Empty,
            Winner = model.Winner,
            MatchDate = model.MatchDate
        };

        try
        {
            matchService.UpdateMatch(match, model.RawMovesText ?? string.Empty, CurrentUserFullName);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("RawMovesText", ex.Message);
            return View(model);
        }

        return RedirectToAction("MatchHistory");
    }

    [HttpPost]
    public IActionResult DeleteMatch(int gameId)
    {
        // Pass CurrentUserId so you can only delete your own match
        matchService.DeleteMatch(gameId, CurrentUserId);
        return RedirectToAction("MatchHistory");
    }

    private MatchHistoryPageViewModel BuildPageViewModel(int page)
    {
        int totalMatches = matchService.GetTotalMatchCount(CurrentUserId);
        // Work out how many pages we need and round up so leftover matches get their own page
        int totalPages = Math.Max(1, (int)Math.Ceiling(totalMatches / (double)PageSize));

        // Keep the page number inside the valid range so nobody can ask for page -5 or page 9999
        if (page < 1)
        {
            page = 1;
        }

        if (page > totalPages)
        {
            page = totalPages;
        }

        List<MatchModel> pagedMatches = matchService.GetPagedMatches(CurrentUserId, page, PageSize);

        // Turn each match into a smaller view model that the page can show
        List<MatchHistoryViewModel> history = pagedMatches
            .Select(match => new MatchHistoryViewModel
            {
                GameId = match.GameId,
                WhitePlayer = match.WhitePlayer,
                BlackPlayer = match.BlackPlayer,
                Winner = match.Winner ?? "Draw",
                MatchDate = match.MatchDate,
                Moves = match.Moves
            })
            .ToList();

        return new MatchHistoryPageViewModel
        {
            Matches = history,
            CurrentPage = page,
            TotalPages = totalPages,
            NewMatch = new AddMatchViewModel()
        };
    }
}
