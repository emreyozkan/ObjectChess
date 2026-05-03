using System;

namespace ObjectChess.Web.ViewModels
{
    public class AddMatchViewModel
    {
        public string? WhitePlayer { get; set; }
        public string? BlackPlayer { get; set; }
        public string? Winner { get; set; }
        public DateTime MatchDate { get; set; } = DateTime.Now;
    }
}