namespace ObjectChess.Models;

public class Move
{
    public Piece MovedPiece { get; private set; }

    public int FromRow { get; private set; }
    public int FromColumn { get; private set; }

    public int ToRow { get; private set; }
    public int ToColumn { get; private set; }

    public Piece CapturedPiece { get; private set; }

    public Move(Piece movedPiece, int fromRow, int fromColumn, int toRow, int toColumn, Piece capturedPiece)
    {
        MovedPiece = movedPiece;
        FromRow = fromRow;
        FromColumn = fromColumn;
        ToRow = toRow;
        ToColumn = toColumn;
        CapturedPiece = capturedPiece;
    }
}

