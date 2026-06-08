using ObjectChess.Business.Models;

namespace ObjectChess.Web.ViewModels;

public class MatchHistoryViewModel
{
    public int GameId { get; set; }
    public string? WhitePlayer { get; set; }
    public string? BlackPlayer { get; set; }
    public string? Winner { get; set; }
    public DateTime MatchDate { get; set; }
    public List<MoveModel> Moves { get; set; } = [];
}
