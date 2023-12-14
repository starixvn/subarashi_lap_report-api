using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Data;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Repositories
{
    public class WeeklyLiveMatchRepositoryTest
    {
        private readonly Mock<ICacheRefAdapter> _cacheRefAdapter;

        private readonly Mock<IObjectDb> _objectDb;

        private readonly Mock<IDataReader> _reader;

        private WeeklyLiveMatchRepository _repository;

        public WeeklyLiveMatchRepositoryTest()
        {
            DbSettingProviderManager
               .StartTestingSession()
               .Run();

            _reader = new Mock<IDataReader>();
            _objectDb = new Mock<IObjectDb>();

            _cacheRefAdapter = new Mock<ICacheRefAdapter>();
        }

        [Theory, InlineData("Week0", 100, 100), InlineData("", 100, 0),]
        public async Task GetDataList_WhenCall_ReturnCorrectResult(string sortColumn, int numbersOfMatches,
                                                                   int expectedNumberOfMatches)
        {
            // Arrange
            _reader.SetupSequence(x => x.Read()).Returns(true).Returns(false);
            _reader.Setup(x => x.FieldCount).Returns(3);
            _reader.Setup(x => x.GetInt32(It.IsAny<int>())).Returns(numbersOfMatches);

            _reader.Setup(x => x[It.Is<string>(x => x.Equals("sporttype"))]).Returns(1);

            SetupObjectDb();

            // Action
            var result = await _repository.GetDataList(
                new WeeklyLiveMatchMetaModel
                {
                    SortDirection = "asc",
                    SortColumn = sortColumn,
                    BaseCurrency = "USD"
                });

            // Assert
            Assert.Single(result.DataList);
            Assert.Equal(numbersOfMatches, result.DataList.ElementAt(0).NumbersOfMatches.ToArray()[0]);
            Assert.Equal(expectedNumberOfMatches, result.DataList.ElementAt(0).SortingWeekValue);
        }

        [Fact]
        public async Task GetDataList_SortWeek0_ReturnCorrectResult()
        {
            // Arrange
            const int NumberOfMatches1 = 1;
            const int NumberOfMatches2 = 2;
            _reader.SetupSequence(x => x.Read()).Returns(true).Returns(true).Returns(false);
            _reader.SetupSequence(x => x.GetInt32(It.IsAny<int>())).Returns(NumberOfMatches2).Returns(NumberOfMatches1);
            _reader.Setup(x => x.FieldCount).Returns(3);

            _reader.Setup(x => x[It.Is<string>(x => x.Equals("sporttype"))]).Returns(1);

            SetupObjectDb();
            // Action
            var result = await _repository.GetDataList(
                new WeeklyLiveMatchMetaModel
                {
                    SortDirection = "asc",
                    SortColumn = "Week0",
                    BaseCurrency = "USD"
                });

            // Assert
            Assert.Collection(result.DataList,
                _ => Assert.True(true),
                _ => Assert.True(true)
            );

            Assert.Equal(NumberOfMatches1, result.DataList.ElementAt(0).NumbersOfMatches.ToArray()[0]);
            Assert.Equal(NumberOfMatches1, result.DataList.ElementAt(0).SortingWeekValue);
        }

        [Theory, InlineData("Week1", "asc"), InlineData("Week0", ""), InlineData("Week0", null), InlineData("", "asc"),
         InlineData(null, "asc"),]
        public async Task GetDataList_InvalidSortIndex_ReturnCorrectResult(string sortColumn, string sortDirection)
        {
            // Arrange
            const int NumberOfMatches1 = 1;
            const int NumberOfMatches2 = 2;
            _reader.SetupSequence(x => x.Read()).Returns(true).Returns(true).Returns(false);
            _reader.SetupSequence(x => x.GetInt32(It.IsAny<int>())).Returns(NumberOfMatches2).Returns(NumberOfMatches1);
            _reader.Setup(x => x.FieldCount).Returns(3);

            _reader.Setup(x => x[It.Is<string>(x => x.Equals("sporttype"))]).Returns(1);

            SetupObjectDb();

            // Action
            var result = await _repository.GetDataList(
                new WeeklyLiveMatchMetaModel
                {
                    SortDirection = sortDirection,
                    SortColumn = sortColumn,
                    BaseCurrency = "USD"
                });

            // Assert
            Assert.Collection(result.DataList,
                _ => Assert.True(true),
                _ => Assert.True(true)
            );

            Assert.Equal(NumberOfMatches2, result.DataList.ElementAt(0).NumbersOfMatches.ToArray()[0]);
            Assert.Equal(1, result.DataList.ElementAt(0).Index);
        }

        [Theory, InlineData(0, 0), InlineData(1, 0), InlineData(2, 0), InlineData(3, 1), InlineData(4, 2),]
        public async Task GetDataList_WhenCall_ReturnCorrectNumberOfWeeks(int fieldCount, int expectedNumberOfWeeks)
        {
            // Arrange

            _reader.SetupSequence(x => x.Read()).Returns(true).Returns(false);
            _reader.Setup(x => x.FieldCount).Returns(fieldCount);

            SetupObjectDb();
            _repository = new WeeklyLiveMatchRepository(_cacheRefAdapter.Object);

            // Action
            var result = await _repository.GetDataList(
                new WeeklyLiveMatchMetaModel
                {
                    SortDirection = "asc",
                    BaseCurrency = "USD"
                });

            // Assert
            Assert.Equal(expectedNumberOfWeeks, result.NumberOfWeeks);
        }

        private void SetupObjectDb()
        {
            _objectDb.Setup(x => x.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(),
                          It.IsAny<CancellationToken>()))
                     .Returns(Task.FromResult(_reader.Object));

            ObjectDbFactory.SwitchToTestingMode();
            ObjectDbFactory.Mock((x, y) => _objectDb.Object);

            _repository = new WeeklyLiveMatchRepository(_cacheRefAdapter.Object);
        }
    }
}
