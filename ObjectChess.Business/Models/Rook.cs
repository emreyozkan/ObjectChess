using ObjectChess.Business.Helpers;

namespace ObjectChess.Business.Models;

public class Rook : Piece 
{
    public Rook(PieceColor color, int row, int column) : base(color, row, column)
    {
    }

    public override bool IsValidMove(int targetRow, int targetColumn, Piece[,] board)
    {
        if (targetRow != Row && targetColumn != Column)
        {
            return false;
        }

        return PathValidator.IsPathClear(Row, Column, targetRow, targetColumn, board);
    }
}