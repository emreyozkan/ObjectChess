namespace ObjectChess.Business.Models;

public class MoveModel
{
    public int MoveId { get; set; }
    public int GameId { get; set; }
    public int MoveNumber { get; set; }
    public string MoveText { get; set; } = string.Empty;
}
