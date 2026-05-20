using System;
using System.ComponentModel.DataAnnotations;

namespace ObjectChess.Web.ViewModels
{
    public class AddMatchViewModel
    {
        public string? WhitePlayer { get; set; }
        
        public string? BlackPlayer { get; set; }
        
        public string? Winner { get; set; }

        [Required]
        public DateTime MatchDate { get; set; } = DateTime.Now;

        public string? RawMovesText { get; set; }
    }
}