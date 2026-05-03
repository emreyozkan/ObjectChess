using System;
using ObjectChess.Business.Helpers;

namespace ObjectChess.Business.Models;

public class Bishop : Piece
{
    public Bishop(PieceColor color, int row, int column) : base(color, row, column)
    {
    }

    public override bool IsValidMove(int targetRow, int targetColumn, Piece[,] board)
    {
        if (Math.Abs(targetRow - Row) != Math.Abs(targetColumn - Column))
        {
            return false; 
        }

        return PathValidator.IsPathClear(Row, Column, targetRow, targetColumn, board);
    }
}