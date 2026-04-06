using System;
using ObjectChess.Helpers;

namespace ObjectChess.Models;

public class Queen : Piece
{
    public Queen(PieceColor color, int row, int column) : base(color, row, column)
    {
    }

    public override bool IsValidMove(int targetRow, int targetColumn, Piece[,] board)
    {
        bool isStraight = (targetRow == Row || targetColumn == Column);
        bool isDiagonal = Math.Abs(targetRow - Row) == Math.Abs(targetColumn - Column);

        if (!isStraight && !isDiagonal)
        {
            return false;
        }

        return PathValidator.IsPathClear(Row, Column, targetRow, targetColumn, board);
    }
}