using System;
using System.IO;
using System.Globalization;

namespace RTEventTimer
{
    public class TimerConfig
    {
        // Timer settings
        public int CountdownMinutes { get; set; } = TimerDefaults.DefaultMinutes;
        public int CountdownSeconds { get; set; } = TimerDefaults.DefaultSeconds;
        public string ActiveText { get; set; } = TimerDefaults.ActiveTimerText;
        public string FinishedText { get; set; } = TimerDefaults.FinishedTimerText;
        
        // Display settings - Clock
        public double ClockX { get; set; } = 0;
        public double ClockY { get; set; } = 0;
        public double ClockFontSize { get; set; } = 120;
        
        // Display settings - Status Text
        public double TextX { get; set; } = 0;
        public double TextY { get; set; } = 0;
        public double TextFontSize { get; set; } = 36;
        
        private static readonly string ConfigFileName = "config.txt";
        
        private static string GetConfigPath()
        {
            // First, check if we're running from the development environment
            // by looking for the project file in parent directories
            var currentDir = AppDomain.CurrentDomain.BaseDirectory;
            var dir = new DirectoryInfo(currentDir);
            
            // Walk up the directory tree looking for the project file
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "RTEventTimer.csproj")))
                {
                    // We found the project root, use it for the config
                    return Path.Combine(dir.FullName, ConfigFileName);
                }
                dir = dir.Parent;
            }
            
            // If we didn't find the project file, we're probably running as a published executable
            // Use the executable's directory
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
        }
        
        public static TimerConfig Load()
        {
            var config = new TimerConfig();
            var configPath = GetConfigPath();
            
            if (!File.Exists(configPath))
                return config;
                
            try
            {
                var lines = File.ReadAllLines(configPath);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;
                        
                    var parts = line.Split('=', 2);
                    if (parts.Length != 2)
                        continue;
                        
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();
                    
                    switch (key.ToLower())
                    {
                        case "countdownminutes":
                            if (int.TryParse(value, out var minutes))
                                config.CountdownMinutes = minutes;
                            break;
                        case "countdownseconds":
                            if (int.TryParse(value, out var seconds))
                                config.CountdownSeconds = seconds;
                            break;
                        case "activetext":
                            config.ActiveText = value;
                            break;
                        case "finishedtext":
                            config.FinishedText = value;
                            break;
                        case "clockx":
                            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var clockX))
                                config.ClockX = clockX;
                            break;
                        case "clocky":
                            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var clockY))
                                config.ClockY = clockY;
                            break;
                        case "clockfontsize":
                            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var clockSize))
                                config.ClockFontSize = clockSize;
                            break;
                        case "textx":
                            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var textX))
                                config.TextX = textX;
                            break;
                        case "texty":
                            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var textY))
                                config.TextY = textY;
                            break;
                        case "textfontsize":
                            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var textSize))
                                config.TextFontSize = textSize;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading config: {ex.Message}");
            }
            
            return config;
        }
        
        public void Save()
        {
            try
            {
                var configPath = GetConfigPath();
                var lines = new[]
                {
                    "# RT Event Timer Configuration",
                    "# Timer Settings",
                    $"CountdownMinutes={CountdownMinutes}",
                    $"CountdownSeconds={CountdownSeconds}",
                    $"ActiveText={ActiveText}",
                    $"FinishedText={FinishedText}",
                    "",
                    "# Display Settings - Clock",
                    $"ClockX={ClockX.ToString(CultureInfo.InvariantCulture)}",
                    $"ClockY={ClockY.ToString(CultureInfo.InvariantCulture)}",
                    $"ClockFontSize={ClockFontSize.ToString(CultureInfo.InvariantCulture)}",
                    "",
                    "# Display Settings - Status Text",
                    $"TextX={TextX.ToString(CultureInfo.InvariantCulture)}",
                    $"TextY={TextY.ToString(CultureInfo.InvariantCulture)}",
                    $"TextFontSize={TextFontSize.ToString(CultureInfo.InvariantCulture)}"
                };
                
                File.WriteAllLines(configPath, lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving config: {ex.Message}");
            }
        }
    }
} 