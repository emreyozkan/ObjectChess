using ObjectChess.Business.Interfaces;
using ObjectChess.Business.Services;
using ObjectChess.Tests.Fakes;
using Xunit;

namespace ObjectChess.Tests
{
    public class MatchServiceTests
    {
        [Fact]
        public void GetTotalMatchCount_ShouldReturnCorrectCount()
        {
            IMatchRepository fakeRepo = new FakeMatchRepository(); 
            MatchService service = new MatchService(fakeRepo);

            int count = service.GetTotalMatchCount();

            Assert.Equal(2, count);
        }
    }
}