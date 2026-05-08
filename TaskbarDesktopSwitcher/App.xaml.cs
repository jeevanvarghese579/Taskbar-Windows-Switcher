using System.Windows;
using System.Windows.Threading;

namespace TaskbarDesktopSwitcher
{
    public partial class App : System.Windows.Application
    {
        private MainWindow? _mainWindow;
        private NotifyIconManager? _notifyIconManager;
        private MouseHook? _mouseHook;
        private StartupManager? _startupManager;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Initialize startup manager
            _startupManager = new StartupManager();

            // Initialize and start mouse hook
            _mouseHook = new MouseHook();
            _mouseHook.WheelScrolled += OnWheelScrolled;
            _mouseHook.Start();

            // Create main window (but don't show it initially)
            _mainWindow = new MainWindow();
            _mainWindow.StartupManager = _startupManager;
            _mainWindow.MouseHook = _mouseHook;

            // Initialize notify icon manager
            _notifyIconManager = new NotifyIconManager(_mainWindow, _startupManager);
            
            // Pass notify icon manager reference to main window
            _mainWindow.NotifyIconManager = _notifyIconManager;

            // Show the main window only if "Start Minimized" is not enabled
            if (!_startupManager.IsStartMinimizedEnabled())
            {
                _mainWindow.Show();
            }
        }

        private void OnWheelScrolled(object? sender, WheelEventArgs e)
        {
            // Check if mouse is over taskbar
            if (TaskbarHelper.IsMouseOverTaskbar())
            {
                // Suppress the original wheel event over taskbar
                e.Handled = true;

                // Trigger virtual desktop switch based on wheel direction
                if (e.Delta > 0)
                {
                    // Wheel up - Win + Ctrl + Left Arrow (previous desktop)
                    VirtualDesktopSwitcher.SwitchToPreviousDesktop();
                }
                else if (e.Delta < 0)
                {
                    // Wheel down - Win + Ctrl + Right Arrow (next desktop)
                    VirtualDesktopSwitcher.SwitchToNextDesktop();
                }
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // Clean up resources
            _mouseHook?.Stop();
            _mouseHook?.Dispose();
            _notifyIconManager?.Dispose();
        }

        public void ExitApplication()
        {
            if (_mainWindow != null)
            {
                _mainWindow.IsExiting = true;
            }
            System.Windows.Application.Current.Shutdown();
        }
    }
}
