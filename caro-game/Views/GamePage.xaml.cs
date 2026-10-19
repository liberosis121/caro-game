using System;
using caro_game.Core.Models;
using caro_game.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace caro_game.Views
{
    /// <summary>
    /// Board screen. Receives the GameSettings from SetupPage through Frame navigation.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        public GamePage()
        {
            InitializeComponent();
        }

        public GameViewModel ViewModel { get; private set; } = null!;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is not GameSettings settings)
            {
                throw new ArgumentException("GamePage needs GameSettings as navigation parameter.", nameof(e));
            }

            // settings comes from the navigation; IWinChecker and IDialogService come from the DI container.
            ViewModel = ActivatorUtilities.CreateInstance<GameViewModel>(App.Services, settings);
            ViewModel.BackToSetupRequested += ViewModel_BackToSetupRequested;
            Bindings.Update();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ViewModel.BackToSetupRequested -= ViewModel_BackToSetupRequested;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Put keyboard focus on the centre cell so the arrow keys work right away.
            int centre = ViewModel.BoardSize / 2;
            int index = (centre * ViewModel.BoardSize) + centre;
            if (BoardRepeater.TryGetElement(index) is Control cell)
            {
                cell.Focus(FocusState.Programmatic);
            }
        }

        private void ViewModel_BackToSetupRequested(object? sender, EventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
    }
}
