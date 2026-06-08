namespace ObjectChess.Web.ViewModels;

public class MatchHistoryPageViewModel
{
    public List<MatchHistoryViewModel> Matches { get; set; } = [];
    public AddMatchViewModel NewMatch { get; set; } = new() { MatchDate = DateTime.Now };
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; } = 10;
}
