using Microsoft.AspNetCore.Mvc;
using ObjectChess.Business.Services;
using ObjectChess.Web.ViewModels;
using ObjectChess.Data.Models;

namespace ObjectChess.Web.Controllers
{
    public class MatchController : Controller
    {
        private readonly IConfiguration _configuration;

        public MatchController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult MatchHistory()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            
            MatchService matchService = new MatchService(connectionString);
            var rawMatches = matchService.GetAllMatches();

            var viewModels = new List<MatchHistoryViewModel>();

            foreach (var item in rawMatches)
            {
                viewModels.Add(new MatchHistoryViewModel
                {
                    WhitePlayer = item.WhitePlayer,
                    BlackPlayer = item.BlackPlayer,
                    Winner = item.Winner,
                    MatchDate = item.MatchDate
                });
            }

            return View(viewModels);
        }
    }
}