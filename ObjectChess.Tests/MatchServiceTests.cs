using Xunit;
using ObjectChess.Business.Models;
using ObjectChess.Business.Services;
using ObjectChess.Tests.Fakes;

namespace ObjectChess.Tests;

public class MatchServiceTests
{
    private const int UserId = 1;
    private const int OtherUserId = 2;

    private static MatchService CreateService(FakeMatchRepository repository)
    {
        return new MatchService(repository, new MoveParser());
    }

    private static MatchModel BuildMatch(int userId, string white = "Alice", string black = "Bob")
    {
        return new MatchModel
        {
            UserId = userId,
            WhitePlayer = white,
            BlackPlayer = black,
            Winner = white,
            MatchDate = DateTime.Now,
            Moves = []
        };
    }

    [Fact]
    public void GetTotalMatchCount_ShouldReturnCountForUser()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);
        repository.AddMatch(BuildMatch(UserId));

        Assert.Equal(1, service.GetTotalMatchCount(UserId));
    }

    [Fact]
    public void GetTotalMatchCount_ShouldReturnZero_WhenUserHasNoMatches()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);

        Assert.Equal(0, service.GetTotalMatchCount(UserId));
    }

    [Fact]
    public void GetPagedMatches_ShouldReturnRequestedPageSize()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);
        for (int i = 0; i < 5; i++)
        {
            repository.AddMatch(BuildMatch(UserId));
        }

        List<MatchModel> matches = service.GetPagedMatches(UserId, 1, 2);

        Assert.Equal(2, matches.Count);
    }

    [Fact]
    public void GetPagedMatches_ShouldReturnEmpty_WhenPageBeyondData()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);
        repository.AddMatch(BuildMatch(UserId));

        List<MatchModel> matches = service.GetPagedMatches(UserId, 3, 5);

        Assert.Empty(matches);
    }

    [Fact]
    public void GetPagedMatches_ShouldNotReturnOtherUsersMatches()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);
        repository.AddMatch(BuildMatch(OtherUserId));

        List<MatchModel> matches = service.GetPagedMatches(UserId, 1, 10);

        Assert.Empty(matches);
    }

    [Fact]
    public void GetPagedMatches_ShouldGiveEachMatchItsOwnMoves()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);
        service.AddMatch(BuildMatch(UserId, white: "A1", black: "B1"), "e4", "A1");
        service.AddMatch(BuildMatch(UserId, white: "A2", black: "B2"), "d4 d5", "A2");

        List<MatchModel> matches = service.GetPagedMatches(UserId, 1, 10);

        Assert.Single(matches.Single(m => m.WhitePlayer == "A1").Moves);
        Assert.Equal(2, matches.Single(m => m.WhitePlayer == "A2").Moves.Count);
    }

    [Fact]
    public void GetPagedMatches_ShouldTreatPageBelowOneAsFirstPage()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);
        repository.AddMatch(BuildMatch(UserId));
        repository.AddMatch(BuildMatch(UserId));

        List<MatchModel> matches = service.GetPagedMatches(UserId, 0, 10);

        Assert.Equal(2, matches.Count);
    }

    [Fact]
    public void GetPagedMatches_ShouldFallBackToDefaultSize_WhenPageSizeBelowOne()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);
        for (int i = 0; i < 3; i++)
        {
            repository.AddMatch(BuildMatch(UserId));
        }

        List<MatchModel> matches = service.GetPagedMatches(UserId, 1, 0);

        Assert.Equal(3, matches.Count);
    }

    [Fact]
    public void DeleteMatch_ShouldRemoveMatch()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);
        MatchModel match = BuildMatch(UserId);
        match.GameId = 1;
        repository.AddMatch(match);

        service.DeleteMatch(1, UserId);

        Assert.Equal(0, service.GetTotalMatchCount(UserId));
    }

    [Fact]
    public void DeleteMatch_ShouldNotRemoveOtherUsersMatch()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);
        MatchModel match = BuildMatch(OtherUserId);
        match.GameId = 1;
        repository.AddMatch(match);

        service.DeleteMatch(1, UserId);

        Assert.Equal(1, service.GetTotalMatchCount(OtherUserId));
    }

    [Fact]
    public void DeleteMatch_ShouldNotThrow_WhenIdIsInvalid()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);

        Exception? exception = Record.Exception(() => service.DeleteMatch(999, UserId));

        Assert.Null(exception);
    }

    [Fact]
    public void AddMatch_ShouldPersistMatchForUser()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);
        MatchModel match = BuildMatch(UserId);

        service.AddMatch(match, "e4 e5 Nf3 Nc6", "Alice");

        Assert.Equal(1, service.GetTotalMatchCount(UserId));
    }

    [Fact]
    public void AddMatch_ShouldResolveMeToCurrentUserName()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);
        MatchModel match = new()
        {
            UserId = UserId,
            WhitePlayer = "Me",
            BlackPlayer = "Bob",
            Winner = "Me",
            MatchDate = DateTime.Now
        };

        service.AddMatch(match, string.Empty, "Alice");

        MatchModel stored = service.GetPagedMatches(UserId, 1, 10).Single();
        Assert.Equal("Alice", stored.WhitePlayer);
        Assert.Equal("Alice", stored.Winner);
    }

    [Fact]
    public void AddMatch_ShouldStoreNullWinner_OnDraw()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);
        MatchModel match = new()
        {
            UserId = UserId,
            WhitePlayer = "Alice",
            BlackPlayer = "Bob",
            Winner = "Draw",
            MatchDate = DateTime.Now
        };

        service.AddMatch(match, string.Empty, "Alice");

        Assert.Null(service.GetPagedMatches(UserId, 1, 10).Single().Winner);
    }

    [Fact]
    public void AddMatch_ShouldThrow_WhenPlayersAreSame()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);
        MatchModel match = BuildMatch(UserId, white: "Bob", black: "Bob");

        Assert.Throws<ArgumentException>(() => service.AddMatch(match, string.Empty, "Alice"));
    }

    [Fact]
    public void AddMatch_ShouldThrow_WhenWinnerIsNotAPlayer()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);
        MatchModel match = new()
        {
            UserId = UserId,
            WhitePlayer = "Alice",
            BlackPlayer = "Bob",
            Winner = "Carl",
            MatchDate = DateTime.Now
        };

        Assert.Throws<ArgumentException>(() => service.AddMatch(match, string.Empty, "Alice"));
    }

    [Fact]
    public void AddMatch_ShouldThrow_OnInvalidMove()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);
        MatchModel match = BuildMatch(UserId);

        Assert.Throws<ArgumentException>(() => service.AddMatch(match, "e4 zzzz", "Alice"));
    }

    [Fact]
    public void AddMatch_ShouldTrimWhitespaceAroundNames()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);
        MatchModel match = new()
        {
            UserId = UserId,
            WhitePlayer = "  Alice  ",
            BlackPlayer = "  Bob  ",
            Winner = "  Alice  ",
            MatchDate = DateTime.Now
        };

        service.AddMatch(match, string.Empty, "Alice");

        MatchModel stored = service.GetPagedMatches(UserId, 1, 10).Single();
        Assert.Equal("Alice", stored.WhitePlayer);
        Assert.Equal("Bob", stored.BlackPlayer);
    }

    [Fact]
    public void AddMatch_ShouldThrow_WhenPlayerIsEmpty()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);
        MatchModel match = new()
        {
            UserId = UserId,
            WhitePlayer = "",
            BlackPlayer = "Bob",
            Winner = "Bob",
            MatchDate = DateTime.Now
        };

        Assert.Throws<ArgumentException>(() => service.AddMatch(match, string.Empty, "Alice"));
    }

    [Fact]
    public void AddMatch_ShouldThrow_WhenPlayerIsWhitespace()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);
        MatchModel match = new()
        {
            UserId = UserId,
            WhitePlayer = "   ",
            BlackPlayer = "Bob",
            Winner = "Bob",
            MatchDate = DateTime.Now
        };

        Assert.Throws<ArgumentException>(() => service.AddMatch(match, string.Empty, "Alice"));
    }

    [Fact]
    public void AddMatch_ShouldStoreNullWinner_WhenWinnerIsEmpty()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);
        MatchModel match = new()
        {
            UserId = UserId,
            WhitePlayer = "Alice",
            BlackPlayer = "Bob",
            Winner = "",
            MatchDate = DateTime.Now
        };

        service.AddMatch(match, string.Empty, "Alice");

        Assert.Null(service.GetPagedMatches(UserId, 1, 10).Single().Winner);
    }

    [Fact]
    public void AddMatch_ShouldAcceptWinner_RegardlessOfCase()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);
        MatchModel match = new()
        {
            UserId = UserId,
            WhitePlayer = "Alice",
            BlackPlayer = "Bob",
            Winner = "alice",
            MatchDate = DateTime.Now
        };

        Exception? exception = Record.Exception(() => service.AddMatch(match, string.Empty, "Alice"));

        Assert.Null(exception);
        Assert.Equal(1, service.GetTotalMatchCount(UserId));
    }

    [Fact]
    public void AddMatch_ShouldResolveMeForBlackPlayer_CaseInsensitive()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);
        MatchModel match = new()
        {
            UserId = UserId,
            WhitePlayer = "Bob",
            BlackPlayer = "me",
            Winner = "Bob",
            MatchDate = DateTime.Now
        };

        service.AddMatch(match, string.Empty, "Alice");

        Assert.Equal("Alice", service.GetPagedMatches(UserId, 1, 10).Single().BlackPlayer);
    }

    [Fact]
    public void UpdateMatch_ShouldReplaceMoves()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);
        MatchModel match = BuildMatch(UserId);
        service.AddMatch(match, "e4 e5", "Alice");
        int gameId = service.GetPagedMatches(UserId, 1, 10).Single().GameId;

        MatchModel edit = new()
        {
            GameId = gameId,
            UserId = UserId,
            WhitePlayer = "Alice",
            BlackPlayer = "Bob",
            Winner = "Alice",
            MatchDate = DateTime.Now
        };
        service.UpdateMatch(edit, "d4 d5", "Alice");

        MatchModel stored = service.GetMatch(gameId, UserId)!;
        Assert.Equal(2, stored.Moves.Count);
        Assert.Equal("d4", stored.Moves[0].MoveText);
    }

    [Fact]
    public void UpdateMatch_ShouldThrow_WhenMatchNotOwnedByUser()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);
        MatchModel match = BuildMatch(OtherUserId);
        match.GameId = 1;
        repository.AddMatch(match);

        MatchModel edit = new()
        {
            GameId = 1,
            UserId = UserId,
            WhitePlayer = "Alice",
            BlackPlayer = "Bob",
            Winner = "Alice",
            MatchDate = DateTime.Now
        };

        Assert.Throws<InvalidOperationException>(() => service.UpdateMatch(edit, string.Empty, "Alice"));
    }

    [Fact]
    public void GetMatch_ShouldReturnNull_ForOtherUsersMatch()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);
        MatchModel match = BuildMatch(UserId);
        match.GameId = 1;
        repository.AddMatch(match);

        Assert.Null(service.GetMatch(1, OtherUserId));
    }

    [Fact]
    public void GetMatch_ShouldReturnNull_WhenMatchDoesNotExist()
    {
        FakeMatchRepository repository = new();
        MatchService service = CreateService(repository);

        Assert.Null(service.GetMatch(999, UserId));
    }
}
