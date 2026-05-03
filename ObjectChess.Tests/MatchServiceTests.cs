using Moq;
using ObjectChess.Business.Interfaces;
using ObjectChess.Business.Models;
using ObjectChess.Business.Services;
using Xunit;
using System;
using System.Collections.Generic;

namespace ObjectChess.Tests
{
    public class MatchServiceTests
    {
        [Fact]
        public void GetTotalMatchCount_ShouldReturnCorrectCount()
        {
            Mock<IMatchRepository> mockRepo = new Mock<IMatchRepository>();
            
            List<MatchModel> fakeMatches = new List<MatchModel>
            {
                new MatchModel { GameID = 1, WhitePlayer = "Ali", BlackPlayer = "Veli" },
                new MatchModel { GameID = 2, WhitePlayer = "Can", BlackPlayer = "Cem" }
            };
            
            mockRepo.Setup(repo => repo.GetAllMatches()).Returns(fakeMatches);

            MatchService service = new MatchService(mockRepo.Object);

            int count = service.GetTotalMatchCount();

            Assert.Equal(2, count);
        }

        [Fact]
        public void AddMatch_ShouldCallRepositoryOnce()
        {
            Mock<IMatchRepository> mockRepo = new Mock<IMatchRepository>();
            MatchService service = new MatchService(mockRepo.Object);
            DateTime matchDate = DateTime.Now;

            service.AddMatch("Magnus Carlsen", "Hikaru Nakamura", "Fabiano Caruana", matchDate);

            mockRepo.Verify(repo => repo.AddMatch("Magnus Carlsen", "Hikaru Nakamura", "Fabiano Caruana", matchDate), Times.Once());
        }

        [Fact]
        public void GetPagedMatches_ShouldReturnCorrectSubset()
        {
            Mock<IMatchRepository> mockRepo = new Mock<IMatchRepository>();
            
            List<MatchModel> fakeMatches = new List<MatchModel>();
            for (int i = 1; i <= 15; i++)
            {
                fakeMatches.Add(new MatchModel { GameID = i, MatchDate = DateTime.Now.AddDays(-i) });
            }

            mockRepo.Setup(repo => repo.GetAllMatches()).Returns(fakeMatches);
            MatchService service = new MatchService(mockRepo.Object);

            List<MatchModel> result = service.GetPagedMatches(1, 10);

            Assert.Equal(10, result.Count);
        }

        [Fact]
        public void GetTotalMatchCount_WhenEmpty_ShouldReturnZero()
        {
            Mock<IMatchRepository> mockRepo = new Mock<IMatchRepository>();
            mockRepo.Setup(repo => repo.GetAllMatches()).Returns(new List<MatchModel>());
            MatchService service = new MatchService(mockRepo.Object);

            int count = service.GetTotalMatchCount();

            Assert.Equal(0, count);
        }
    }
}