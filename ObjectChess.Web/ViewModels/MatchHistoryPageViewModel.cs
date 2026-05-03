namespace ObjectChess.Web.ViewModels
{
    public class MatchHistoryPageViewModel
    {
        public List<MatchHistoryViewModel> Matches { get; set; } = new List<MatchHistoryViewModel>();
        public AddMatchViewModel NewMatch { get; set; } = new AddMatchViewModel();
    }
}