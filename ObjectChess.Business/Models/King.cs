using System;

namespace ObjectChess.Business.Models;

public class King : Piece
{
    public King(PieceColor color, int row, int column) : base(color, row, column)
    {
    }

    public override bool IsValidMove(int targetRow, int targetColumn, Piece[,] board)
    {
        int rowDiff = Math.Abs(targetRow - Row);
        int columnDiff = Math.Abs(targetColumn - Column);

        if (rowDiff <= 1 && columnDiff <= 1)
        {
            return true;
        }

        return false;
    }
}