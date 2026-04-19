using System;

namespace ObjectChess.Web.ViewModels
{
    public class AddMatchViewModel
    {
        public string WhitePlayer { get; set; } = string.Empty;
        public string BlackPlayer { get; set; } = string.Empty;
        public string Winner { get; set; } = string.Empty;
        public DateTime MatchDate { get; set; } = DateTime.Now;
    }
}