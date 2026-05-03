namespace ObjectChess.Business.Models;
public enum PieceColor
{
    White,
    Black
}

public abstract class Piece
{
    public PieceColor Color { get; private set; }

    public int Row { get; protected set; }
    public int Column { get; protected set; }

    public bool HasMoved { get; protected set; }

    public Piece(PieceColor color, int row, int column)
    {
        Color = color;
        Row = row;
        Column = column;
        HasMoved = false;
    }

    public abstract bool IsValidMove(int targetRow, int targetColumn, Piece[,] board);
}

