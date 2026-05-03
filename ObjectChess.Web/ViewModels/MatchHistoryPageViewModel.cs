using System.Collections.Generic;
using System;

namespace ObjectChess.Web.ViewModels
{
    public class MatchHistoryPageViewModel
    {
        public List<MatchHistoryViewModel> Matches { get; set; } = new List<MatchHistoryViewModel>();
        public AddMatchViewModel NewMatch { get; set; } = new AddMatchViewModel { MatchDate = DateTime.Now };
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;
    }
}