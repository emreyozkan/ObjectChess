using System.ComponentModel.DataAnnotations;

namespace ObjectChess.Web.ViewModels;

public class EditMatchViewModel
{
    public int GameId { get; set; }

    [Required]
    public string? WhitePlayer { get; set; }

    [Required]
    public string? BlackPlayer { get; set; }

    [Required]
    public string? Winner { get; set; }

    [Required]
    public DateTime MatchDate { get; set; }

    public string? RawMovesText { get; set; }
}
