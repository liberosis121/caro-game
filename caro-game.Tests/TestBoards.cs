using caro_game.Core.Models;

namespace caro_game.Tests
{
    internal static class TestBoards
    {
        /// <summary>
        /// Builds a board from text rows, e.g. "XX.O": 'X' and 'O' are stones, any other character is empty.
        /// </summary>
        public static Board FromRows(params string[] rows)
        {
            var board = new Board(rows.Length);
            for (int row = 0; row < rows.Length; row++)
            {
                Assert.Equal(rows.Length, rows[row].Length);
                for (int column = 0; column < rows[row].Length; column++)
                {
                    char symbol = rows[row][column];
                    if (symbol == 'X')
                    {
                        board.Place(new Position(row, column), Player.X);
                    }
                    else if (symbol == 'O')
                    {
                        board.Place(new Position(row, column), Player.O);
                    }
                }
            }

            return board;
        }

        /// <summary>
        /// Plays the moves in order (players alternate automatically) and returns the last result.
        /// </summary>
        public static MoveResult? PlayAll(this caro_game.Core.Services.GameEngine engine, params (int Row, int Column)[] moves)
        {
            MoveResult? result = null;
            foreach ((int row, int column) in moves)
            {
                result = engine.Play(new Position(row, column));
            }

            return result;
        }
    }
}
