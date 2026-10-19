namespace caro_game.Core.Models
{
    /// <summary>
    /// Everything chosen on the setup screen. Passed to the game page when navigating.
    /// </summary>
    public sealed class GameSettings
    {
        public GameSettings(GameRules rules, string playerXName, string playerOName, Player firstPlayer)
        {
            ArgumentNullException.ThrowIfNull(rules);
            ArgumentException.ThrowIfNullOrWhiteSpace(playerXName);
            ArgumentException.ThrowIfNullOrWhiteSpace(playerOName);
            if (firstPlayer == Player.None)
            {
                throw new ArgumentException("The first player must be X or O.", nameof(firstPlayer));
            }

            Rules = rules;
            PlayerXName = playerXName.Trim();
            PlayerOName = playerOName.Trim();
            FirstPlayer = firstPlayer;
        }

        public GameRules Rules { get; }

        public string PlayerXName { get; }

        public string PlayerOName { get; }

        public Player FirstPlayer { get; }

        public string GetPlayerName(Player player)
        {
            return player switch
            {
                Player.X => PlayerXName,
                Player.O => PlayerOName,
                _ => throw new ArgumentOutOfRangeException(nameof(player), player, "Only X or O has a name.")
            };
        }
    }
}
