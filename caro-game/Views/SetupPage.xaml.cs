using caro_game.Core.Models;
using caro_game.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;

namespace caro_game.Views
{
    /// <summary>
    /// First screen: the players enter the board size n and options.
    /// The page is cached, so the values are still there after coming back from a game.
    /// </summary>
    public sealed partial class SetupPage : Page
    {
        public SetupPage()
        {
            ViewModel = App.Services.GetRequiredService<SetupViewModel>();
            InitializeComponent();
        }

        public SetupViewModel ViewModel { get; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.StartRequested += ViewModel_StartRequested;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ViewModel.StartRequested -= ViewModel_StartRequested;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Ready for typing: focus the board size box and select its text.
            BoardSizeTextBox.Focus(FocusState.Programmatic);
            BoardSizeTextBox.SelectAll();
        }

        private void ViewModel_StartRequested(object? sender, GameSettings settings)
        {
            // Pass the chosen settings to the game page (received in GamePage.OnNavigatedTo).
            Frame.Navigate(typeof(GamePage), settings,
                new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void InputTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && ViewModel.StartCommand.CanExecute(null))
            {
                e.Handled = true;
                ViewModel.StartCommand.Execute(null);
            }
        }
    }
}
