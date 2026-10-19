using caro_game.Core.Models;
using caro_game.Core.Services;

namespace caro_game.Tests
{
    public class WinCheckerTests
    {
        private readonly WinChecker _checker = new();

        [Fact]
        public void FiveInARow_Horizontally_Wins()
        {
            Board board = TestBoards.FromRows(
                "........",
                ".XXXXX..",
                "........",
                "........",
                "........",
                "........",
                "........",
                "........");

            IReadOnlyList<Position> line = _checker.FindWinningLine(board, new Position(1, 3), new GameRules(8));

            Assert.Equal(
                new[] { new Position(1, 1), new Position(1, 2), new Position(1, 3), new Position(1, 4), new Position(1, 5) },
                line);
        }

        [Fact]
        public void FiveInARow_Vertically_Wins()
        {
            Board board = TestBoards.FromRows(
                "......",
                "....O.",
                "....O.",
                "....O.",
                "....O.",
                "....O.");

            IReadOnlyList<Position> line = _checker.FindWinningLine(board, new Position(5, 4), new GameRules(6));

            Assert.Equal(5, line.Count);
            Assert.All(line, p => Assert.Equal(4, p.Column));
        }

        [Fact]
        public void FiveInARow_OnMainDiagonal_Wins()
        {
            Board board = TestBoards.FromRows(
                "X.....",
                ".X....",
                "..X...",
                "...X..",
                "....X.",
                "......");

            IReadOnlyList<Position> line = _checker.FindWinningLine(board, new Position(0, 0), new GameRules(6));

            Assert.Equal(5, line.Count);
            Assert.All(line, p => Assert.Equal(p.Row, p.Column));
        }

        [Fact]
        public void FiveInARow_OnAntiDiagonal_Wins()
        {
            Board board = TestBoards.FromRows(
                "......",
                ".....O",
                "....O.",
                "...O..",
                "..O...",
                ".O....");

            IReadOnlyList<Position> line = _checker.FindWinningLine(board, new Position(3, 3), new GameRules(6));

            Assert.Equal(5, line.Count);
            Assert.All(line, p => Assert.Equal(6, p.Row + p.Column));
        }

        [Fact]
        public void FourInARow_OnBigBoard_DoesNotWin()
        {
            Board board = TestBoards.FromRows(
                "......",
                "XXXX..",
                "......",
                "......",
                "......",
                "......");

            Assert.Empty(_checker.FindWinningLine(board, new Position(1, 3), new GameRules(6)));
        }

        [Fact]
        public void LongerThanFive_StillWins_AndHighlightsTheWholeRow()
        {
            Board board = TestBoards.FromRows(
                "XXXXXX.",
                ".......",
                ".......",
                ".......",
                ".......",
                ".......",
                ".......");

            Assert.Equal(6, _checker.FindWinningLine(board, new Position(0, 2), new GameRules(7)).Count);
        }

        [Fact]
        public void OnFourByFourBoard_AFullLineOfFourWins()
        {
            Board board = TestBoards.FromRows(
                "OOO.",
                "XXXX",
                "....",
                "....");

            Assert.Equal(4, _checker.FindWinningLine(board, new Position(1, 0), new GameRules(4)).Count);
            Assert.Empty(_checker.FindWinningLine(board, new Position(0, 0), new GameRules(4)));
        }

        [Fact]
        public void BlockedAtBothEnds_DoesNotWin_WhenRuleIsOn()
        {
            Board board = TestBoards.FromRows(
                "OXXXXXO.",
                "........",
                "........",
                "........",
                "........",
                "........",
                "........",
                "........");
            var lastMove = new Position(0, 3);

            Assert.Empty(_checker.FindWinningLine(board, lastMove, new GameRules(8, blockBothEnds: true)));
            Assert.Equal(5, _checker.FindWinningLine(board, lastMove, new GameRules(8, blockBothEnds: false)).Count);
        }

        [Fact]
        public void BlockedAtOneEnd_StillWins_WhenRuleIsOn()
        {
            Board board = TestBoards.FromRows(
                "OXXXXX..",
                "........",
                "........",
                "........",
                "........",
                "........",
                "........",
                "........");

            Assert.Equal(5, _checker.FindWinningLine(board, new Position(0, 5), new GameRules(8, blockBothEnds: true)).Count);
        }

        [Fact]
        public void BoardEdge_IsNotABlock()
        {
            Board board = TestBoards.FromRows(
                "XXXXXO",
                "......",
                "......",
                "......",
                "......",
                "......");

            Assert.Equal(5, _checker.FindWinningLine(board, new Position(0, 0), new GameRules(6, blockBothEnds: true)).Count);
        }

        [Fact]
        public void EmptyCell_IsNeverAWin()
        {
            var board = new Board(5);

            Assert.Empty(_checker.FindWinningLine(board, new Position(2, 2), new GameRules(5)));
        }
    }
}
