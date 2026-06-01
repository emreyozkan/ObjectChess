using System;
using System.Collections.Generic;
using Xunit;
using ObjectChess.Business.Services;
using ObjectChess.Business.Models;
using ObjectChess.Tests.Fakes;

namespace ObjectChess.Tests
{
    public class MatchServiceTests
    {
        [Fact]
        public void GetTotalMatchCount_ShouldReturnCorrectNumber()
        {
            FakeMatchRepository fakeRepository = new FakeMatchRepository();
            MatchService matchService = new MatchService(fakeRepository);
            string playerEmail = "emre@example.com";

            MatchModel match = new MatchModel
            {
                WhitePlayer = playerEmail,
                BlackPlayer = "opponent@example.com",
                Winner = playerEmail,
                MatchDate = DateTime.Now,
                Moves = new List<MoveModel>()
            };

            fakeRepository.AddMatch(match);

            int count = matchService.GetTotalMatchCount(playerEmail);

            Assert.Equal(1, count);
        }

        [Fact]
        public void GetPagedMatches_ShouldReturnOnlyRequestedAmount()
        {
            FakeMatchRepository fakeRepository = new FakeMatchRepository();
            MatchService matchService = new MatchService(fakeRepository);
            string playerEmail = "emre@example.com";

            for (int i = 0; i < 5; i++)
            {
                MatchModel match = new MatchModel
                {
                    WhitePlayer = playerEmail,
                    BlackPlayer = "opponent@example.com",
                    Winner = playerEmail,
                    MatchDate = DateTime.Now,
                    Moves = new List<MoveModel>()
                };

                fakeRepository.AddMatch(match);
            }

            int offset = 0;
            int pageSize = 2;

            List<MatchModel> matches = matchService.GetPagedMatches(playerEmail, offset, pageSize);

            Assert.Equal(2, matches.Count);
        }

        [Fact]
        public void DeleteMatch_ShouldReduceTotalMatchCount()
        {
            FakeMatchRepository fakeRepository = new FakeMatchRepository();
            MatchService matchService = new MatchService(fakeRepository);
            string playerEmail = "emre@example.com";

            MatchModel match = new MatchModel
            {
                GameID = 1,
                WhitePlayer = playerEmail,
                BlackPlayer = "opponent@example.com",
                Winner = playerEmail,
                MatchDate = DateTime.Now,
                Moves = new List<MoveModel>()
            };

            fakeRepository.AddMatch(match);

            matchService.DeleteMatch(1);

            int count = matchService.GetTotalMatchCount(playerEmail);

            Assert.Equal(0, count);
        }

        [Fact]
        public void GetTotalMatchCount_ShouldReturnZero_WhenPlayerIsNew()
        {
            FakeMatchRepository fakeRepository = new FakeMatchRepository();
            MatchService matchService = new MatchService(fakeRepository);

            int count = matchService.GetTotalMatchCount("new_player@example.com");

            Assert.Equal(0, count);
        }

        [Fact]
        public void AddMatch_ShouldIncreaseTotalCount()
        {
            FakeMatchRepository fakeRepository = new FakeMatchRepository();
            MatchService matchService = new MatchService(fakeRepository);
            string playerEmail = "emre@example.com";

            fakeRepository.AddMatch(playerEmail, "opponent@example.com", playerEmail, DateTime.Now);

            int count = matchService.GetTotalMatchCount(playerEmail);

            Assert.Equal(1, count);
        }

        [Fact]
        public void GetPagedMatches_ShouldReturnEmptyList_WhenOffsetIsTooLarge()
        {
            FakeMatchRepository fakeRepository = new FakeMatchRepository();
            MatchService matchService = new MatchService(fakeRepository);
            string playerEmail = "emre@example.com";

            fakeRepository.AddMatch(playerEmail, "opponent@example.com", playerEmail, DateTime.Now);

            int offset = 10;
            int pageSize = 5;

            List<MatchModel> matches = matchService.GetPagedMatches(playerEmail, offset, pageSize);

            Assert.Empty(matches);
        }

        [Fact]
        public void DeleteMatch_ShouldNotCrash_WhenIdIsInvalid()
        {
            FakeMatchRepository fakeRepository = new FakeMatchRepository();
            MatchService matchService = new MatchService(fakeRepository);

            Exception exception = Record.Exception(() => matchService.DeleteMatch(999));

            Assert.Null(exception);
        }

        [Fact]
        public void GetPagedMatches_ShouldNotReturnOtherPlayersMatches()
        {
            FakeMatchRepository fakeRepository = new FakeMatchRepository();
            MatchService matchService = new MatchService(fakeRepository);

            string myEmail = "emre@example.com";
            string strangerEmail = "stranger@example.com";

            fakeRepository.AddMatch(strangerEmail, "other_opponent@example.com", strangerEmail, DateTime.Now);

            List<MatchModel> myMatches = matchService.GetPagedMatches(myEmail, 0, 10);

            Assert.Empty(myMatches);
        }

        [Fact]
        public void GetAllMatches_ShouldReturnEverything()
        {
            FakeMatchRepository fakeRepository = new FakeMatchRepository();

            fakeRepository.AddMatch("player1@example.com", "player2@example.com", "player1@example.com", DateTime.Now);
            fakeRepository.AddMatch("player3@example.com", "player4@example.com", "player4@example.com", DateTime.Now);

            List<MatchModel> allMatches = fakeRepository.GetAllMatches();

            Assert.Equal(2, allMatches.Count);
        }
    }
}