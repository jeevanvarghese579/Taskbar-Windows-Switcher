using System.Windows;
using System.Windows.Media;

namespace TaskbarDesktopSwitcher
{
    public partial class MainWindow : Window
    {
        public StartupManager? StartupManager { get; set; }
        public bool IsExiting { get; set; } = false;

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainWindow_Closing;
            LoadIcon();
        }

        private void LoadIcon()
        {
            try
            {
                var iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
                if (System.IO.File.Exists(iconPath))
                {
                    var icon = new System.Windows.Media.Imaging.BitmapImage(new Uri(iconPath));
                    this.Icon = icon;
                }
            }
            catch
            {
                // Silently handle icon loading errors
            }
        }

        #nullable disable
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsExiting)
            {
                // Allow exit
                return;
            }
            // Minimize to tray instead of closing
            e.Cancel = true;
            this.Hide();
        }
        #nullable restore

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize the toggle based on current startup state
            if (StartupManager != null)
            {
                StartWithWindowsToggle.IsChecked = StartupManager.IsStartWithWindowsEnabled();
            }
        }

        private void StartWithWindowsToggle_Checked(object sender, RoutedEventArgs e)
        {
            StartupManager?.EnableStartWithWindows();
        }

        private void StartWithWindowsToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            StartupManager?.DisableStartWithWindows();
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }

        public void UpdateStatus(bool isRunning)
        {
            if (isRunning)
            {
                StatusIndicator.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80)); // Green
                StatusText.Text = "Running";
            }
            else
            {
                StatusIndicator.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(244, 67, 54)); // Red
                StatusText.Text = "Stopped";
            }
        }
    }
}