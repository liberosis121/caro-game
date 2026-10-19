using System.Threading.Tasks;

namespace caro_game.Services
{
    /// <summary>
    /// Lets view models ask the user for confirmation without knowing about ContentDialog or XamlRoot.
    /// </summary>
    public interface IDialogService
    {
        /// <returns>True when the user chose <paramref name="confirmText"/>.</returns>
        Task<bool> ConfirmAsync(string title, string message, string confirmText, string cancelText);
    }
}
