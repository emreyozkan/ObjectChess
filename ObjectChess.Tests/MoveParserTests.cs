using Xunit;
using ObjectChess.Business.Models;
using ObjectChess.Business.Services;

namespace ObjectChess.Tests;

public class MoveParserTests
{
    private static MoveParser CreateParser() => new();

    [Fact]
    public void Parse_ShouldReturnEmpty_WhenInputIsEmpty()
    {
        Assert.Empty(CreateParser().Parse(string.Empty));
    }

    [Fact]
    public void Parse_ShouldReturnEmpty_WhenInputIsWhitespace()
    {
        Assert.Empty(CreateParser().Parse("   "));
    }

    [Fact]
    public void Parse_ShouldReadEachMove()
    {
        List<MoveModel> moves = CreateParser().Parse("e4 e5 Nf3 Nc6");

        Assert.Equal(4, moves.Count);
        Assert.Equal("e4", moves[0].MoveText);
    }

    [Fact]
    public void Parse_ShouldSkipMoveNumbers()
    {
        List<MoveModel> moves = CreateParser().Parse("1. e4 e5 2. Nf3 Nc6");

        Assert.Equal(4, moves.Count);
    }

    [Fact]
    public void Parse_ShouldNumberMovesByFullMove()
    {
        List<MoveModel> moves = CreateParser().Parse("e4 e5 Nf3");

        Assert.Equal(1, moves[0].MoveNumber);
        Assert.Equal(1, moves[1].MoveNumber);
        Assert.Equal(2, moves[2].MoveNumber);
    }

    [Fact]
    public void Parse_ShouldHandleNewlines()
    {
        List<MoveModel> moves = CreateParser().Parse("e4\ne5");

        Assert.Equal(2, moves.Count);
    }

    [Theory]
    [InlineData("O-O")]
    [InlineData("O-O-O")]
    [InlineData("Nf3")]
    [InlineData("Nxe5")]
    [InlineData("exd5")]
    [InlineData("Qh5+")]
    [InlineData("Qh7#")]
    public void Parse_ShouldAcceptValidMove(string move)
    {
        Assert.Single(CreateParser().Parse(move));
    }

    [Theory]
    [InlineData("zzzz")]
    [InlineData("e9")]
    [InlineData("Xf3")]
    [InlineData("e4!")]
    public void Parse_ShouldThrow_OnInvalidMove(string move)
    {
        Assert.Throws<ArgumentException>(() => CreateParser().Parse(move));
    }
}
