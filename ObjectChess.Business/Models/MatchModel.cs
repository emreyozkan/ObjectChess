namespace ObjectChess.Business.Models
{
    public class MatchModel
    {
        public int GameID { get; set; }
        public string WhitePlayer { get; set; } = string.Empty;
        public string BlackPlayer { get; set; } = string.Empty;
        public string Winner { get; set; } = string.Empty;
        public DateTime MatchDate { get; set; }
    }
}