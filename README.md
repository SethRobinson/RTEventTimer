# RTEventTimer - Real-Time Event Timer for Windows

A modern, always-on-top countdown timer application for Windows with transparent background support, designed for use in mahjong parlors or any environment requiring a highly visible countdown display.

## Features

- **Large Digital Clock**: Displays countdown accurate to tenths of a second
- **Always On Top**: Timer window stays above all other windows
- **Transparent Background**: Uses PNG with transparency for seamless overlay
- **Customizable Messages**: Set different text for active and finished states
- **Timer Controls**: Start, Stop, Pause, and Restart functionality
- **Audio Alert**: Plays sound when timer reaches zero
- **Visual Alert**: Flashing animation when timer completes
- **Single File Deployment**: All resources embedded in one executable


## Quick Start (For End Users)

1. Download `RTEventTimer_v1.0.0.zip`
2. Extract the ZIP file
3. Double-click `RTEventTimer.exe` to run
4. No installation required!

## Usage

1. **Moving the Window**: Click and drag anywhere on the timer display to reposition
2. **Opening Settings**: Click the gear icon in the top-right corner
3. **Timer Controls**:
   - **Start**: Begin or resume the countdown
   - **Pause**: Temporarily stop the countdown (can resume)
   - **Stop**: Stop and reset the timer
   - **Restart**: Stop and immediately restart with the set duration
4. **Customization**:
   - Set countdown duration in minutes and seconds
   - Customize the "active" message shown during countdown
   - Customize the "finished" message shown when timer reaches zero

## Building from Source

### Prerequisites

- .NET 6.0 SDK or later
- Windows 10/11
- 7-Zip (for packaging, path set in build script)

### Required Assets

Before building, add these files to the `Assets` folder:

1. **background.png**: A transparent PNG image (1920x270 pixels recommended)
   - The transparent areas will allow desktop content to show through
   - Semi-transparent areas will create an overlay effect
   
2. **timer_finished.wav**: Sound file that plays when timer reaches zero
   - Any WAV format audio file will work

3. **icon.ico**: Application icon (multi-resolution)
   - Should contain 16x16, 32x32, 48x48, and 256x256 sizes

### Build Instructions

#### Automated Build (Recommended)

Run the build script to create a signed, single-file executable:

```batch
build_and_package.bat
```

This will:
- Build a self-contained single executable
- Sign the executable (requires signing environment)
- Package everything into a ZIP file
- Output: `RTEventTimer_v1.0.0.zip`

#### Manual Build

For development or testing:

```bash
# Debug build
dotnet build

# Run directly
dotnet run

# Create release build
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Design Specifications

- **Display Size**: 270 pixels height (25% of 1080p screen height)
- **Font**: Large, bold Consolas font for maximum readability
- **Text Effects**: Drop shadow for visibility against varied backgrounds
- **Color Scheme**: White text with black shadow for contrast

## Technical Details

- **Framework**: .NET 6.0 with WPF
- **Language**: C#
- **Window Style**: Borderless with transparency support
- **Update Rate**: 10 Hz (100ms intervals) for smooth tenth-second display
- **Deployment**: Single-file executable with embedded resources

## Creating Custom Assets

### Background Image
For best results, create a PNG with:
- Dimensions: 1920x270 pixels
- Format: 32-bit PNG with alpha channel
- Design tips:
  - Use semi-transparent dark gradients for readability
  - Add subtle borders or effects around the edges
  - Keep the center area relatively clear for timer display

### Sample Background Creation (Photoshop/GIMP)
1. Create new image: 1920x270 pixels
2. Fill with transparent background
3. Add new layer with black-to-transparent gradient
4. Set layer opacity to 70-80%
5. Add subtle rounded corners or border effects
6. Export as PNG with transparency

## Troubleshooting

- **Timer not starting**: Ensure .NET 6.0 Desktop Runtime is installed
- **No sound on completion**: Check Windows sound settings and volume
- **Window not staying on top**: Check if another "always on top" application is running
- **Build errors**: Ensure all required assets are in the Assets folder before building

## License

This application is provided as-is for use in mahjong parlors and similar environments. 