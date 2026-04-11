using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ObjectChess.Models;

namespace ObjectChess.Controllers
{
    public class MatchController : Controller
    {
        private readonly IConfiguration _configuration;

        public MatchController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult MatchHistory()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            
            List<MatchHistoryViewModel> matchHistoryList = new List<MatchHistoryViewModel>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT 
                        White.Username AS WhitePlayer, 
                        Black.Username AS BlackPlayer, 
                        Winner.Username AS Winner, 
                        g.MatchDate 
                    FROM Games g
                    JOIN Players White ON g.WhitePlayerID = White.PlayerID
                    JOIN Players Black ON g.BlackPlayerID = Black.PlayerID
                    LEFT JOIN Players Winner ON g.WinnerID = Winner.PlayerID
                    ORDER BY g.MatchDate DESC;";
                
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            MatchHistoryViewModel match = new MatchHistoryViewModel();

                            match.WhitePlayer = reader["WhitePlayer"].ToString();
                            match.BlackPlayer = reader["BlackPlayer"].ToString();
                            match.MatchDate = Convert.ToDateTime(reader["MatchDate"]);

                            if (reader["Winner"] != DBNull.Value)
                            {
                                match.Winner = reader["Winner"].ToString();
                            }
                            else
                            {
                                match.Winner = "Draw";
                            }

                            matchHistoryList.Add(match);
                        }
                    }
                }
            }

            return View(matchHistoryList);
        }
    }
}