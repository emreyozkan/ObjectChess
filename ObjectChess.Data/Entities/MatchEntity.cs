using System;

namespace ObjectChess.Data.Entities
{
    public class MatchEntity
    {
        public int GameID { get; set; }
        
        public string? WhitePlayer { get; set; } 
        public string? BlackPlayer { get; set; }
        public string? Winner { get; set; }
        
        public DateTime MatchDate { get; set; }
    }
}