using caro_game.Core.Models;

namespace caro_game.Tests
{
    public class GameRulesTests
    {
        [Theory]
        [InlineData(4)]
        [InlineData(10)]
        [InlineData(20)]
        public void Constructor_AcceptsSizesFrom4To20(int boardSize)
        {
            var rules = new GameRules(boardSize);

            Assert.Equal(boardSize, rules.BoardSize);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(3)]
        [InlineData(21)]
        public void Constructor_RejectsSizesOutside4To20(int boardSize)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GameRules(boardSize));
        }

        [Theory]
        [InlineData(4, 4)]
        [InlineData(5, 5)]
        [InlineData(15, 5)]
        [InlineData(20, 5)]
        public void WinLength_IsFiveOrTheWholeLineOnSmallBoards(int boardSize, int expectedWinLength)
        {
            Assert.Equal(expectedWinLength, new GameRules(boardSize).WinLength);
            Assert.Equal(expectedWinLength, GameRules.GetWinLength(boardSize));
        }

        [Theory]
        [InlineData("4", 4)]
        [InlineData("15", 15)]
        [InlineData("20", 20)]
        [InlineData("  12  ", 12)]
        [InlineData("+8", 8)]
        public void ValidateBoardSize_AcceptsValidInput(string input, int expectedSize)
        {
            BoardSizeError error = GameRules.ValidateBoardSize(input, out int size);

            Assert.Equal(BoardSizeError.None, error);
            Assert.Equal(expectedSize, size);
        }

        [Theory]
        [InlineData(null, BoardSizeError.Empty)]
        [InlineData("", BoardSizeError.Empty)]
        [InlineData("   ", BoardSizeError.Empty)]
        [InlineData("abc", BoardSizeError.NotAWholeNumber)]
        [InlineData("1.5", BoardSizeError.NotAWholeNumber)]
        [InlineData("1e1", BoardSizeError.NotAWholeNumber)]
        [InlineData("-", BoardSizeError.NotAWholeNumber)]
        [InlineData("1 0", BoardSizeError.NotAWholeNumber)]
        [InlineData("3", BoardSizeError.OutOfRange)]
        [InlineData("0", BoardSizeError.OutOfRange)]
        [InlineData("-10", BoardSizeError.OutOfRange)]
        [InlineData("21", BoardSizeError.OutOfRange)]
        [InlineData("99999999999999999999", BoardSizeError.OutOfRange)]
        public void ValidateBoardSize_ReportsWhyInputIsInvalid(string? input, BoardSizeError expectedError)
        {
            BoardSizeError error = GameRules.ValidateBoardSize(input, out int size);

            Assert.Equal(expectedError, error);
            Assert.Equal(0, size);
        }
    }
}
