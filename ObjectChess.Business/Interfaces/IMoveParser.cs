using ObjectChess.Business.Models;

namespace ObjectChess.Business.Interfaces;

public interface IMoveParser
{
    List<MoveModel> Parse(string rawMoves);
}
