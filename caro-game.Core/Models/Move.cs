namespace caro_game.Core.Models
{
    /// <summary>
    /// A stone placed by a player at a position.
    /// </summary>
    public sealed record Move(Position Position, Player Player);
}
