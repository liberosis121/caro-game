using System.Globalization;

namespace caro_game.Core.Models
{
    /// <summary>
    /// Rules of one game: board size n (4 ≤ n ≤ 20), how many stones in a row win,
    /// and the optional Vietnamese "blocked at both ends" rule.
    /// </summary>
    public sealed class GameRules
    {
        public const int MinBoardSize = 4;
        public const int MaxBoardSize = 20;
        public const int StandardWinLength = 5;

        public GameRules(int boardSize, bool blockBothEnds = false)
        {
            if (!IsValidBoardSize(boardSize))
            {
                throw new ArgumentOutOfRangeException(nameof(boardSize), boardSize,
                    $"Board size must be between {MinBoardSize} and {MaxBoardSize}.");
            }

            BoardSize = boardSize;
            WinLength = GetWinLength(boardSize);
            BlockBothEnds = blockBothEnds;
        }

        public int BoardSize { get; }

        /// <summary>
        /// Number of stones in a row (horizontal, vertical or diagonal) needed to win.
        /// </summary>
        public int WinLength { get; }

        /// <summary>
        /// When true, a winning row whose two ends are both blocked by the opponent does not count.
        /// </summary>
        public bool BlockBothEnds { get; }

        public static bool IsValidBoardSize(int boardSize)
        {
            return boardSize >= MinBoardSize && boardSize <= MaxBoardSize;
        }

        /// <summary>
        /// Classic caro needs 5 in a row. A 4 × 4 board cannot hold 5, so a full line of 4 wins there.
        /// </summary>
        public static int GetWinLength(int boardSize)
        {
            return Math.Min(StandardWinLength, boardSize);
        }

        /// <summary>
        /// Checks the text typed by the user and returns why it is invalid, or <see cref="BoardSizeError.None"/>.
        /// </summary>
        public static BoardSizeError ValidateBoardSize(string? text, out int boardSize)
        {
            boardSize = 0;
            string input = text?.Trim() ?? string.Empty;

            if (input.Length == 0)
            {
                return BoardSizeError.Empty;
            }

            if (!IsWholeNumber(input))
            {
                return BoardSizeError.NotAWholeNumber;
            }

            // A whole number that does not fit in int is simply too large.
            if (!int.TryParse(input, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int value)
                || !IsValidBoardSize(value))
            {
                return BoardSizeError.OutOfRange;
            }

            boardSize = value;
            return BoardSizeError.None;
        }

        private static bool IsWholeNumber(string input)
        {
            int start = input[0] is '+' or '-' ? 1 : 0;
            if (start == input.Length)
            {
                return false;
            }

            for (int i = start; i < input.Length; i++)
            {
                if (!char.IsAsciiDigit(input[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
