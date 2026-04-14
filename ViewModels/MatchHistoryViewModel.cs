namespace ObjectChess.Web.ViewModels
{
    public class MatchHistoryViewModel
    {
        public string WhitePlayer { get; set; } = string.Empty;
        public string BlackPlayer { get; set; } = string.Empty;
        public string Winner { get; set; } = string.Empty;
        public DateTime MatchDate { get; set; }
    }
}