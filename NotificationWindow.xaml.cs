using System;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Eye202020;

/// <summary>
/// Notification window that appears every 20 minutes, stays visible for 20 seconds, then fades out.
/// Uses P/Invoke to allow input passthrough for gaming compatibility.
/// </summary>
public partial class NotificationWindow : Window
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
    private DispatcherTimer? _countdownTimer;
    private int _remainingSeconds = 20;
    private bool _showInstruction;

    /// <summary>
    /// Initialize notification window with optional instruction image display
    /// </summary>
    /// <param name="showInstruction">Whether to show the instruction image</param>
    public NotificationWindow(bool showInstruction = true)
    {
        _showInstruction = showInstruction;
        InitializeComponent();
        SetupInstructionImage();
        PositionWindow();
        ApplyInputPassthroughStyles();
        PlayNotificationSound();
        StartCountdownTimer();
        StartVisibilityTimer();
    }

    /// <summary>
    /// Setup instruction image visibility based on user preference
    /// </summary>
    private void SetupInstructionImage()
    {
        if (InstructionImage != null)
        {
            InstructionImage.Visibility = _showInstruction ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Apply window styles for input passthrough when window handle is created
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
                
                // Add WS_EX_TRANSPARENT (mouse clicks pass through) and WS_EX_NOACTIVATE (never steals focus)
                extendedStyle |= WS_EX_TRANSPARENT | WS_EX_NOACTIVATE;
                
                // Apply the new style
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle);
            }
        };
    }

    /// <summary>
    /// Position window at top center of screen
    /// </summary>
    private void PositionWindow()
    {
        // Use ContentRendered to ensure window is fully measured and rendered
        ContentRendered += (s, e) =>
        {
            // Get work area (screen minus taskbar)
            var workArea = SystemParameters.WorkArea;
            
            // Calculate position for top center with 20px margin from top
            Left = (workArea.Width - ActualWidth) / 2 + workArea.Left;
            Top = workArea.Top + 20;
        };
    }

    /// <summary>
    /// Play system beep sound when notification appears
    /// </summary>
    private void PlayNotificationSound()
    {
        SystemSounds.Beep.Play();
    }

    /// <summary>
    /// Start countdown timer that updates every second
    /// </summary>
    private void StartCountdownTimer()
    {
        _countdownTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _countdownTimer.Tick += (s, e) =>
        {
            _remainingSeconds--;
            if (_remainingSeconds > 0)
            {
                CountdownText.Text = $"Look away for {_remainingSeconds} seconds";
            }
            else
            {
                _countdownTimer?.Stop();
                CountdownText.Text = "Look away for 0 seconds";
            }
        };
        _countdownTimer.Start();
    }

    /// <summary>
    /// Start timer for 20-second visibility, then fade out
    /// </summary>
    private void StartVisibilityTimer()
    {
        _visibilityTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(20)
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
    /// Cleanup timers on window close
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        _visibilityTimer?.Stop();
        _countdownTimer?.Stop();
        base.OnClosed(e);
    }
}

