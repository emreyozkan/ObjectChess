using System.Collections.Generic;
using ObjectChess.Business.Models;

namespace ObjectChess.Business.Managers;

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

