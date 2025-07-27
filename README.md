# AutoClicker Pro

A feature-rich autoclicker application built with C# and Windows Forms.

## Features

- **Adjustable Click Frequency**: Set click rate from 1 to 1000 clicks per second
- **Customizable Hotkey**: Choose from F1-F12 keys to toggle clicking (default: F9)
- **Position Recording**: Record specific screen coordinates for targeted clicking
- **Random Delay Variation**: Add randomness to avoid detection patterns
- **Click Counter**: Track total number of clicks performed
- **System Tray Support**: Minimize to system tray for background operation
- **Settings Persistence**: Automatically saves and restores your preferences
- **Professional GUI**: Clean, intuitive interface with grouped controls

## How to Build

1. Ensure you have .NET 9.0 or later installed
2. Open a terminal in the project directory
3. Run the following command:
   ```
   dotnet build --configuration Release
   ```
4. The executable will be created in `bin/Release/net9.0-windows/`

## How to Create a Standalone Executable

To create a single-file executable that doesn't require .NET runtime:

```bash
dotnet publish --configuration Release --runtime win-x64 --self-contained true --output ./publish-net9
```

## Usage

1. **Set Click Frequency**: Use the slider or numeric input to set clicks per second (1-1000)
2. **Choose Hotkey**: Select your preferred toggle key from the dropdown (F1-F12)
3. **Record Position** (Optional): Click "Record Current Position" to set a specific click location
4. **Enable Random Delay** (Optional): Check the box and set variation in milliseconds
5. **Start Clicking**: Click "Start Clicking" button or press your chosen hotkey
6. **Stop Clicking**: Click "Stop Clicking" button or press the hotkey again

## Controls

- **Start/Stop Button**: Manually toggle the autoclicker
- **Hotkey Toggle**: Press your selected F-key to start/stop clicking
- **Reset Counter**: Clear the click count
- **Reset Position**: Return to "click anywhere" mode
- **Minimize to Tray**: Hide the application to system tray

## Settings

All settings are automatically saved to `settings.json` in the application directory and restored when the program is restarted.

## System Requirements

- Windows 10 or later
- .NET 9.0 Runtime (if not using self-contained build)

## Safety Features

- Random delay variation to avoid detection
- Easy stop mechanism (hotkey or button)
- Visual status indicators
- Frequency limits to prevent system overload