using System;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;

namespace TaskbarDesktopSwitcher
{
    /// <summary>
    /// Manages the system tray icon and its context menu.
    /// Provides quick access to app functions from the tray.
    /// Uses Windows Forms NotifyIcon for system tray functionality.
    /// </summary>
    public class NotifyIconManager : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly MainWindow _mainWindow;
        private readonly StartupManager _startupManager;
        private ToolStripMenuItem? _startWithWindowsMenuItem;
        private ToolStripMenuItem? _enableSwitcherMenuItem;

        public NotifyIconManager(MainWindow mainWindow, StartupManager startupManager)
        {
            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
            _startupManager = startupManager ?? throw new ArgumentNullException(nameof(startupManager));

            // Initialize the notify icon with custom icon
            var iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
            _notifyIcon = new NotifyIcon
            {
                Icon = System.IO.File.Exists(iconPath) ? new Icon(iconPath) : SystemIcons.Application,
                Text = "Taskbar Desktop Switcher",
                Visible = true
            };

            // Create context menu
            _notifyIcon.ContextMenuStrip = CreateContextMenu();

            // Handle left click to open window
            _notifyIcon.MouseClick += (s, e) => 
            {
                if (e.Button == MouseButtons.Left)
                {
                    ShowMainWindow();
                }
            };
            // Also handle double-click for convenience
            _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();
        }

        /// <summary>
        /// Creates the context menu for the tray icon.
        /// </summary>
        private ContextMenuStrip CreateContextMenu()
        {
            var menu = new ContextMenuStrip();

            // Open menu item
            var openItem = new ToolStripMenuItem("Open");
            openItem.Click += (s, e) => ShowMainWindow();
            menu.Items.Add(openItem);

            // Enable/Disable Desktop Switcher menu item
            _enableSwitcherMenuItem = new ToolStripMenuItem("Enable Desktop Switcher");
            _enableSwitcherMenuItem.Checked = _mainWindow.IsSwitcherEnabled;
            _enableSwitcherMenuItem.Click += (s, e) =>
            {
                if (_enableSwitcherMenuItem != null && _mainWindow != null)
                {
                    // When Click fires, Checked has already been toggled by ToolStripMenuItem
                    bool isEnabled = _enableSwitcherMenuItem.Checked;
                    
                    // Update main window toggle
                    _mainWindow.EnableSwitcherToggle.IsChecked = isEnabled;
                    
                    // Directly control the MouseHook
                    if (isEnabled)
                    {
                        _mainWindow.MouseHook?.Start();
                        _mainWindow.UpdateStatus(true);
                    }
                    else
                    {
                        _mainWindow.MouseHook?.Stop();
                        _mainWindow.UpdateStatus(false);
                    }
                    
                    _mainWindow.IsSwitcherEnabled = isEnabled;
                }
            };
            menu.Items.Add(_enableSwitcherMenuItem);

            // Start with Windows toggle menu item
            _startWithWindowsMenuItem = new ToolStripMenuItem("Start with Windows");
            _startWithWindowsMenuItem.Checked = _startupManager.IsStartWithWindowsEnabled();
            _startWithWindowsMenuItem.Click += (s, e) =>
            {
                if (_startWithWindowsMenuItem != null)
                {
                    // When Click fires, Checked has already been toggled by ToolStripMenuItem
                    // If now checked = user just checked it -> enable startup
                    // If now unchecked = user just unchecked it -> disable startup
                    if (_startWithWindowsMenuItem.Checked)
                    {
                        // User just checked it - enable startup
                        _startupManager.EnableStartWithWindows();
                    }
                    else
                    {
                        // User just unchecked it - disable startup
                        _startupManager.DisableStartWithWindows();
                    }

                    // Sync the toggle in main window
                    if (_mainWindow.StartupManager != null)
                    {
                        _mainWindow.StartWithWindowsToggle.IsChecked = _startWithWindowsMenuItem.Checked;
                    }
                }
            };
            menu.Items.Add(_startWithWindowsMenuItem);

            // Separator
            menu.Items.Add(new ToolStripSeparator());

            // About menu item
            var aboutItem = new ToolStripMenuItem("About");
            aboutItem.Click += (s, e) =>
            {
                var aboutWindow = new AboutWindow();
                aboutWindow.Owner = _mainWindow;
                aboutWindow.ShowDialog();
            };
            menu.Items.Add(aboutItem);

            // Separator
            menu.Items.Add(new ToolStripSeparator());

            // Exit menu item
            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) =>
            {
                var app = System.Windows.Application.Current as App;
                app?.ExitApplication();
            };
            menu.Items.Add(exitItem);

            return menu;
        }

        /// <summary>
        /// Shows the main window and brings it to front.
        /// </summary>
        private void ShowMainWindow()
        {
            if (_mainWindow != null)
            {
                _mainWindow.Show();
                _mainWindow.WindowState = WindowState.Normal;
                _mainWindow.Activate();
                _mainWindow.Focus();
            }
        }

        /// <summary>
        /// Updates the Start with Windows check state in the context menu.
        /// </summary>
        public void UpdateStartWithWindowsState()
        {
            if (_startWithWindowsMenuItem != null)
            {
                _startWithWindowsMenuItem.Checked = _startupManager.IsStartWithWindowsEnabled();
            }
        }

        /// <summary>
        /// Updates the Enable Desktop Switcher check state in the context menu.
        /// </summary>
        public void UpdateEnableSwitcherState(bool isEnabled)
        {
            if (_enableSwitcherMenuItem != null)
            {
                _enableSwitcherMenuItem.Checked = isEnabled;
            }
        }

        public void Dispose()
        {
            _notifyIcon?.Dispose();
        }
    }
}