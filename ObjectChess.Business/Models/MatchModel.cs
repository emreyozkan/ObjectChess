namespace ObjectChess.Business.Models;

public class MatchModel
{
    public int GameId { get; set; }
    public int UserId { get; set; }
    public string WhitePlayer { get; set; } = string.Empty;
    public string BlackPlayer { get; set; } = string.Empty;
    public string? Winner { get; set; }
    public DateTime MatchDate { get; set; }
    public List<MoveModel> Moves { get; set; } = [];
}
