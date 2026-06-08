using System.Text.RegularExpressions;
using ObjectChess.Business.Interfaces;
using ObjectChess.Business.Models;

namespace ObjectChess.Business.Services;

public partial class MoveParser : IMoveParser
{
    // This regex describes what a valid chess move in standard notation looks like
    // It covers castling like O-O and a pawn move like e4 and a capture like exd5
    // It also covers a piece move like Nf3 and promotion =Q and check + and mate #
    [GeneratedRegex(@"^(O-O(-O)?|[a-h][1-8]|[a-h]x[a-h][1-8]|[KQRBN][a-h1-8x]?[a-h][1-8](=[QRBN])?[+#]?)$")]
    private static partial Regex MovePattern();

    public List<MoveModel> Parse(string rawMoves)
    {
        // This list will hold every move we manage to read
        List<MoveModel> moves = [];

        // If nothing was typed then just give back an empty list
        if (string.IsNullOrWhiteSpace(rawMoves))
        {
            return moves;
        }

        // Turn newlines into spaces first
        // Then cut the whole text into separate words and each word should be one move
        string[] tokens = rawMoves
            .Replace("\r\n", " ")
            .Replace("\n", " ")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Start counting from move number one
        int moveNumber = 1;
        int halfMoves = 0; // counts single moves and white plus black make one full move

        foreach (string token in tokens)
        {
            // Skip the move numbers people type like "1." or "1"
            // They are not actual moves
            if (token.EndsWith('.') || int.TryParse(token, out _))
            {
                continue;
            }

            // If a word does not look like a real chess move then stop
            // Tell the user which one is wrong
            if (!MovePattern().IsMatch(token))
            {
                throw new ArgumentException($"Invalid move: {token}");
            }

            // This word is a valid move so add it to the list
            moves.Add(new MoveModel
            {
                MoveNumber = moveNumber,
                MoveText = token
            });

            // Every 2 half moves (white then black) we go to the next move number
            if (++halfMoves % 2 == 0)
            {
                moveNumber++;
            }
        }

        return moves;
    }
}
