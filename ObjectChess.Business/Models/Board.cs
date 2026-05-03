namespace ObjectChess.Business.Models;

public class Board 
{
    public Piece[,] Grid { get; private set; }

    public Board()
    {
        Grid = new Piece[8,8];
    }
}