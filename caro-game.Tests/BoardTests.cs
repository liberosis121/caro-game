using caro_game.Core.Models;

namespace caro_game.Tests
{
    public class BoardTests
    {
        [Fact]
        public void NewBoard_IsEmpty()
        {
            var board = new Board(5);

            Assert.Equal(5, board.Size);
            Assert.Equal(0, board.OccupiedCount);
            Assert.False(board.IsFull);
            Assert.True(board.IsEmpty(new Position(2, 3)));
        }

        [Fact]
        public void Place_StoresStoneAndRejectsOccupiedCell()
        {
            var board = new Board(4);
            var position = new Position(1, 2);

            board.Place(position, Player.O);

            Assert.Equal(Player.O, board[position]);
            Assert.Equal(1, board.OccupiedCount);
            Assert.Throws<InvalidOperationException>(() => board.Place(position, Player.X));
        }

        [Theory]
        [InlineData(-1, 0)]
        [InlineData(0, -1)]
        [InlineData(4, 0)]
        [InlineData(0, 4)]
        public void PositionsOutsideTheBoard_AreRejected(int row, int column)
        {
            var board = new Board(4);
            var position = new Position(row, column);

            Assert.False(board.IsInside(position));
            Assert.Throws<ArgumentOutOfRangeException>(() => board[position]);
            Assert.Throws<ArgumentOutOfRangeException>(() => board.Place(position, Player.X));
        }

        [Fact]
        public void IsFull_WhenEveryCellIsOccupied()
        {
            Board board = TestBoards.FromRows(
                "XOXO",
                "XOXO",
                "OXOX",
                "OXOX");

            Assert.True(board.IsFull);
            Assert.Equal(16, board.OccupiedCount);
        }

        [Fact]
        public void RemoveAndClear_EmptyCells()
        {
            Board board = TestBoards.FromRows(
                "XO..",
                "....",
                "....",
                "....");

            board.Remove(new Position(0, 0));

            Assert.True(board.IsEmpty(new Position(0, 0)));
            Assert.Equal(1, board.OccupiedCount);
            Assert.Throws<InvalidOperationException>(() => board.Remove(new Position(0, 0)));

            board.Clear();

            Assert.Equal(0, board.OccupiedCount);
            Assert.True(board.IsEmpty(new Position(0, 1)));
        }
    }
}
