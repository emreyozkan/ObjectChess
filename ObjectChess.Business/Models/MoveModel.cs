using System;

namespace ObjectChess.Business.Models
{
    public class MoveModel
    {
        public int MoveID { get; set; }
        public int GameID { get; set; }
        public int MoveNumber { get; set; } 
        public string MoveText { get; set; } = ""; 
    }
}