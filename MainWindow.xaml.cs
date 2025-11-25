using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace Eye202020;

/// <summary>
/// Main window that runs in the background with system tray icon and timer for eye protection notifications
/// </summary>
public partial class MainWindow : Window
{
    private NotifyIcon? _notifyIcon;
    private DispatcherTimer? _timer;
    private DateTime _timerStartTime;
    private ToolStripMenuItem? _countdownMenuItem;
    private DispatcherTimer? _countdownDisplayTimer;
    private Icon? _customIcon;
    private bool _showInstruction = true; // Default to showing instruction
    private ToolStripMenuItem? _showInstructionMenuItem;
    private TimeSpan _notificationInterval = TimeSpan.FromMinutes(20);

    public MainWindow()
    {
        InitializeComponent();
        InitializeTrayIcon();
        InitializeTimer();
        HideWindow();
        ShowStartupNotification();
    }

    /// <summary>
    /// Initialize system tray icon with context menu
    /// </summary>
    private void InitializeTrayIcon()
    {
        // Try to load custom icon, fallback to system icon if not found
        try
        {
            var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
            if (File.Exists(iconPath))
            {
                _customIcon = new Icon(iconPath);
            }
        }
        catch
        {
            // If loading fails, use system icon
        }

        _notifyIcon = new NotifyIcon
        {
            Icon = _customIcon ?? System.Drawing.SystemIcons.Application,
            Text = "20-20-20 Eye Protection",
            Visible = true
        };

        // Create context menu with countdown, toggle, and quit option
        var contextMenu = new ContextMenuStrip();
        
        _countdownMenuItem = new ToolStripMenuItem("Next in: --:--");
        _countdownMenuItem.Enabled = false; // Make it non-clickable, just for display
        contextMenu.Items.Add(_countdownMenuItem);
        
        contextMenu.Items.Add(new ToolStripSeparator());
        
        _showInstructionMenuItem = new ToolStripMenuItem("Show instruction");
        _showInstructionMenuItem.Checked = _showInstruction;
        _showInstructionMenuItem.CheckOnClick = true;
        _showInstructionMenuItem.Click += (s, e) =>
        {
            _showInstruction = _showInstructionMenuItem.Checked;
        };
        contextMenu.Items.Add(_showInstructionMenuItem);
        
        contextMenu.Items.Add(new ToolStripSeparator());
        
        var quitMenuItem = new ToolStripMenuItem("Quit");
        quitMenuItem.Click += (s, e) => System.Windows.Application.Current.Shutdown();
        contextMenu.Items.Add(quitMenuItem);

        _notifyIcon.ContextMenuStrip = contextMenu;
        
        // Start timer to update countdown display
        _countdownDisplayTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _countdownDisplayTimer.Tick += UpdateCountdownDisplay;
        _countdownDisplayTimer.Start();
    }

    /// <summary>
    /// Initialize 20-minute timer to trigger notifications
    /// </summary>
    private void InitializeTimer()
    {
        _timerStartTime = DateTime.Now;
        _timer = new DispatcherTimer
        {
            Interval = _notificationInterval
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    /// <summary>
    /// Timer tick event handler - creates and shows notification window
    /// </summary>
    private void Timer_Tick(object? sender, EventArgs e)
    {
        ShowNotification();
        // Restart timer for next 20-minute interval
        _timerStartTime = DateTime.Now;
    }

    /// <summary>
    /// Show notification window
    /// </summary>
    private void ShowNotification()
    {
        var notificationWindow = new NotificationWindow(_showInstruction);
        notificationWindow.Show();
    }

    /// <summary>
    /// Update countdown display in menu item text
    /// </summary>
    private void UpdateCountdownDisplay(object? sender, EventArgs e)
    {
        if (_countdownMenuItem == null)
            return;
        
        var elapsed = DateTime.Now - _timerStartTime;
        var remaining = _notificationInterval - elapsed;
        
        if (remaining.TotalSeconds <= 0)
        {
            _countdownMenuItem.Text = "Next notification due!";
        }
        else
        {
            var minutes = (int)remaining.TotalMinutes;
            var seconds = remaining.Seconds;
            _countdownMenuItem.Text = $"Next in: {minutes:D2}:{seconds:D2}";
        }
    }

    /// <summary>
    /// Hide window on startup - runs in background
    /// </summary>
    private void HideWindow()
    {
        WindowState = WindowState.Minimized;
        ShowInTaskbar = false;
        Hide();
    }

    /// <summary>
    /// Show startup notification bubble at bottom-right
    /// </summary>
    private void ShowStartupNotification()
    {
        // Delay slightly to ensure window is hidden first
        var startupTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        startupTimer.Tick += (s, e) =>
        {
            startupTimer.Stop();
            var startupNotification = new StartupNotificationWindow();
            startupNotification.Show();
        };
        startupTimer.Start();
    }

    /// <summary>
    /// Cleanup resources on window close
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        _timer?.Stop();
        _countdownDisplayTimer?.Stop();
        _notifyIcon?.Dispose();
        _customIcon?.Dispose();
        base.OnClosed(e);
    }
}