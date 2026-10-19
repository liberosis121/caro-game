using System;
using System.Runtime.InteropServices;
using caro_game.Views;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace caro_game
{
    public sealed partial class MainWindow : Window
    {
        // Sizes in effective pixels (epx), converted to physical pixels with the monitor scale.
        private const int PreferredWidth = 1280;
        private const int PreferredHeight = 860;
        private const int MinimumWidth = 720;
        private const int MinimumHeight = 640;

        public MainWindow()
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;

            ConfigureSize();
            RootFrame.Navigate(typeof(SetupPage));
        }

        [DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(IntPtr hwnd);

        /// <summary>
        /// Opens the window centred on the screen, as large as preferred but never bigger than the work area,
        /// and sets a minimum size so the board and side panel always fit.
        /// </summary>
        private void ConfigureSize()
        {
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            double scale = GetDpiForWindow(hwnd) / 96.0;

            if (AppWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.PreferredMinimumWidth = (int)(MinimumWidth * scale);
                presenter.PreferredMinimumHeight = (int)(MinimumHeight * scale);
            }

            RectInt32 workArea = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Primary).WorkArea;
            int width = Math.Min((int)(PreferredWidth * scale), workArea.Width);
            int height = Math.Min((int)(PreferredHeight * scale), workArea.Height);
            int x = workArea.X + ((workArea.Width - width) / 2);
            int y = workArea.Y + ((workArea.Height - height) / 2);

            AppWindow.MoveAndResize(new RectInt32(x, y, width, height));
        }
    }
}
