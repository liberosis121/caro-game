using System;
using caro_game.Core.Services;
using caro_game.Services;
using caro_game.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace caro_game
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Services = ConfigureServices();
        }

        /// <summary>
        /// Dependency injection container: pages resolve their view models (and the view models
        /// their services) from here instead of creating dependencies with "new".
        /// </summary>
        public static IServiceProvider Services { get; private set; } = null!;

        /// <summary>
        /// The main window, used by services that need its XamlRoot (e.g. ContentDialog).
        /// </summary>
        public static Window? MainWindow { get; private set; }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            MainWindow = _window;
            _window.Activate();
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IWinChecker, WinChecker>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddTransient<SetupViewModel>();

            // GameViewModel also needs the GameSettings chosen at runtime,
            // so GamePage creates it with ActivatorUtilities (settings + services from this container).
            return services.BuildServiceProvider();
        }
    }
}
