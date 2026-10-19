using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace caro_game.Helpers
{
    /// <summary>
    /// Command for actions that await something (e.g. a confirmation dialog).
    /// It disables itself while running so a double click cannot start it twice.
    /// </summary>
    public sealed class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;
        private bool _isRunning;

        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return !_isRunning && (_canExecute?.Invoke() ?? true);
        }

        // ICommand.Execute has to return void. Exceptions are not swallowed:
        // they reach the dispatcher like any other UI event handler error.
        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }

            _isRunning = true;
            RaiseCanExecuteChanged();
            try
            {
                await _execute();
            }
            finally
            {
                _isRunning = false;
                RaiseCanExecuteChanged();
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
