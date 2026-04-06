using System;
using ObjectChess.Models;

namespace ObjectChess.Helpers;

public static class PathValidator 
{
    public static bool IsPathClear(int startRow, int startColumn, int targetRow, int targetColumn, Piece[,] board)
    {
        int rowDirection = Math.Sign(targetRow - startRow);
        int columnDirection = Math.Sign(targetColumn - startColumn);
        
        int currentRow = startRow + rowDirection;
        int currentColumn = startColumn + columnDirection;

        while (currentRow != targetRow || currentColumn != targetColumn)
        {
            if (board[currentRow, currentColumn] != null)
            {
                return false;
            }

            currentRow += rowDirection;
            currentColumn += columnDirection;
        }

        return true;
    }
}