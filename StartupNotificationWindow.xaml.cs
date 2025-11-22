using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Eye202020;

/// <summary>
/// Startup notification window that appears briefly when the app starts
/// </summary>
public partial class StartupNotificationWindow : Window
{
    // P/Invoke declarations for user32.dll
    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    // Window style constants
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_NOACTIVATE = 0x08000000;

    private DispatcherTimer? _visibilityTimer;

    public StartupNotificationWindow()
    {
        InitializeComponent();
        PositionWindow();
        ApplyInputPassthroughStyles();
        StartVisibilityTimer();
    }

    /// <summary>
    /// Apply window styles for input passthrough when window handle is created
    /// Note: For standard notifications, we only use WS_EX_NOACTIVATE (don't steal focus)
    /// but allow mouse interaction (no WS_EX_TRANSPARENT)
    /// </summary>
    private void ApplyInputPassthroughStyles()
    {
        SourceInitialized += (s, e) =>
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd != IntPtr.Zero)
            {
                // Get current extended window style
                var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                
                // Only add WS_EX_NOACTIVATE (never steals focus) for standard notification
                // Don't use WS_EX_TRANSPARENT so it behaves like a normal notification
                extendedStyle |= WS_EX_NOACTIVATE;
                
                // Apply the new style
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle);
            }
        };
    }

    /// <summary>
    /// Position window at bottom-right corner of screen
    /// </summary>
    private void PositionWindow()
    {
        // Use ContentRendered to ensure window is fully measured and rendered
        ContentRendered += (s, e) =>
        {
            // Get work area (screen minus taskbar)
            var workArea = SystemParameters.WorkArea;
            
            // Calculate position for bottom-right corner with 20px margin
            Left = workArea.Right - ActualWidth - 20;
            Top = workArea.Bottom - ActualHeight - 20;
        };
    }

    /// <summary>
    /// Start timer for 3-second visibility, then fade out
    /// </summary>
    private void StartVisibilityTimer()
    {
        _visibilityTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3)
        };
        _visibilityTimer.Tick += (s, e) =>
        {
            _visibilityTimer.Stop();
            FadeOutAndClose();
        };
        _visibilityTimer.Start();
    }

    /// <summary>
    /// Fade out animation and close window
    /// </summary>
    private void FadeOutAndClose()
    {
        var fadeOut = new DoubleAnimation
        {
            From = 1.0,
            To = 0.0,
            Duration = new Duration(TimeSpan.FromSeconds(0.5))
        };

        fadeOut.Completed += (s, e) => Close();

        BeginAnimation(OpacityProperty, fadeOut);
    }

    /// <summary>
    /// Cleanup timer on window close
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        _visibilityTimer?.Stop();
        base.OnClosed(e);
    }
}

