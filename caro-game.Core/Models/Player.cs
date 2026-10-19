namespace caro_game.Core.Models
{
    /// <summary>
    /// Owner of a cell: nobody, player X or player O.
    /// </summary>
    public enum Player
    {
        None = 0,
        X = 1,
        O = 2
    }

    public static class PlayerExtensions
    {
        /// <summary>
        /// Returns the other player (X ↔ O).
        /// </summary>
        public static Player Opponent(this Player player)
        {
            return player switch
            {
                Player.X => Player.O,
                Player.O => Player.X,
                _ => throw new ArgumentOutOfRangeException(nameof(player), player, "Only X or O has an opponent.")
            };
        }

        /// <summary>
        /// Text drawn on the board for the player ("X", "O" or empty).
        /// </summary>
        public static string ToSymbol(this Player player)
        {
            return player switch
            {
                Player.X => "X",
                Player.O => "O",
                _ => string.Empty
            };
        }
    }
}
