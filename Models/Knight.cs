using System;

namespace ObjectChess.Models;

public class Knight : Piece
{
    public Knight(PieceColor color, int row, int column) : base(color, row, column)
    {
    }

    public override bool IsValidMove(int targetRow, int targetColumn, Piece[,] board)
    {
        int rowDiff = Math.Abs(targetRow - Row);
        int columnDiff = Math.Abs(targetColumn - Column);

        if ((rowDiff == 2 && columnDiff == 1) || (rowDiff == 1 && columnDiff == 2))
        {
            return true;
        }

        return false;
    }
}