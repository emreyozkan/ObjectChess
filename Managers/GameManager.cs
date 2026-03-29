using System.Collections.Generic;
using ObjectChess.Models;

namespace ObjectChess.Managers;

public class GameManager
{
    public List<Move> MatchHistory { get; private set; } = new List<Move>();

    public PieceColor CurrentTurn { get; private set; }

    public GameManager()
    {
        CurrentTurn = PieceColor.White;
    }

    public void SwitchTurn()
    {
        if (CurrentTurn == PieceColor.White)
        {
            CurrentTurn = PieceColor.Black;
        }
        else 
        {
            CurrentTurn = PieceColor.White;
        }
    }

    public void RecordMove(Move playedMove)
    {
        MatchHistory.Add(playedMove);
        SwitchTurn();
    }
}    

