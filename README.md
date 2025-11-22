# 20-20-20 Eye Protection

A Windows desktop application that helps protect your eyes by reminding you to take breaks every 20 minutes, following the 20-20-20 rule (every 20 minutes, look at something 20 feet away for 20 seconds).

## Features

- **Background Operation**: Runs silently in the system tray
- **20-Minute Timer**: Automatically displays a notification every 20 minutes
- **20-Second Countdown**: Shows a countdown timer during the break reminder
- **Gaming-Friendly**: Notifications use input passthrough, so they won't interfere with fullscreen games
- **Non-Intrusive**: Notifications appear at the top center and fade out automatically
- **Live Countdown**: System tray menu shows the countdown until the next notification
- **Startup Notification**: Displays a brief notification when the app starts

## Requirements

- Windows 10/11
- .NET 8.0 Runtime (for framework-dependent builds) OR no installation needed (for self-contained builds)

## Building

### Framework-Dependent Build
```bash
dotnet build -c Release
```

### Self-Contained Single-File Build (Recommended)
Creates a single executable that works on any Windows machine without .NET installation:
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The output will be in: `bin/Release/net8.0-windows/win-x64/publish/Eye202020.exe`

## Usage

1. Run `Eye202020.exe`
2. The app will start in the background and show a brief startup notification
3. Right-click the system tray icon to:
   - View the countdown until the next notification
   - Quit the application
4. Every 20 minutes, a notification will appear at the top center of your screen
5. The notification shows a 20-second countdown and automatically fades away

## Customization

### Changing the App Icon
1. Create or obtain an `.ico` file
2. Name it `icon.ico` and place it in the project root
3. Rebuild the application

The icon will be used for both the application icon and the system tray icon.

## Technical Details

- Built with WPF (Windows Presentation Foundation)
- Uses P/Invoke for input passthrough (WS_EX_TRANSPARENT and WS_EX_NOACTIVATE)
- System tray integration using Windows Forms NotifyIcon
- Self-contained builds exclude language resources for smaller size

## License

[Add your license here]


