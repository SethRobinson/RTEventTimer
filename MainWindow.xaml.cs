using System;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RTEventTimer
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private DateTime endTime;
        private TimeSpan remainingTime;
        private bool isRunning;
        private bool isPaused;
        private bool hasFinished;  // Track if timer has completed
        private SoundPlayer soundPlayer;
        private SoundPlayer buttonSoundPlayer;
        private Storyboard flashStoryboard;
        private SettingsDialog? settingsDialog;
        private TimerConfig config;
        
        // Settings - initialized from config file
        private string activeText;
        private string finishedText;
        private int countdownMinutes;
        private int countdownSeconds;

        private Point _startPoint;
        private bool _isDragging;

        public MainWindow()
        {
            InitializeComponent();
            
            // Load configuration
            config = TimerConfig.Load();
            activeText = config.ActiveText;
            finishedText = config.FinishedText;
            countdownMinutes = config.CountdownMinutes;
            countdownSeconds = config.CountdownSeconds;
            
            // Apply display settings
            ApplyDisplaySettings();
            
            InitializeTimer();
            InitializeSoundPlayer();
            CreateFlashAnimation();
            LoadBackgroundImage();
            
            // Initialize display with default time
            UpdateTimerDisplay(new TimeSpan(0, countdownMinutes, countdownSeconds));
            
            // Don't override the initial XAML text unless timer is running
            // This allows custom initial messages in XAML
            
            // Set initial status text
            UpdateStatusText();
        }
        
        private void ApplyDisplaySettings()
        {
            // Apply font sizes
            TimerDisplay.FontSize = config.ClockFontSize;
            StatusText.FontSize = config.TextFontSize;
            
            // Apply positions using transforms
            TimerDisplay.RenderTransform = new TranslateTransform(config.ClockX, config.ClockY);
            StatusText.RenderTransform = new TranslateTransform(config.TextX, config.TextY);
        }

        private static string GetProjectRootOrAppDirectory()
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
                    // We found the project root
                    return dir.FullName;
                }
                dir = dir.Parent;
            }
            
            // If we didn't find the project file, we're probably running as a published executable
            // Use the executable's directory
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        private void LoadBackgroundImage()
        {
            try
            {
                string imagePath = Path.Combine(GetProjectRootOrAppDirectory(), "Assets", "background.png");
                
                if (File.Exists(imagePath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    
                    BackgroundImage.Source = bitmap;
                }
                else
                {
                    // Log for debugging
                    Console.WriteLine($"Background image not found at: {imagePath}");
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't crash - the fallback background will show
                Console.WriteLine($"Error loading background image: {ex.Message}");
            }
        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100); // Update every 0.1 seconds
            timer.Tick += Timer_Tick;
        }

        private void InitializeSoundPlayer()
        {
            try
            {
                // Load timer finished sound
                string soundPath = Path.Combine(GetProjectRootOrAppDirectory(), "Assets", "timer_finished.wav");
                if (File.Exists(soundPath))
                {
                    soundPlayer = new SoundPlayer(soundPath);
                    soundPlayer.Load();
                }
                else
                {
                    Console.WriteLine($"Timer finished sound not found: {soundPath}");
                }
                
                // Load button click sound
                string buttonSoundPath = Path.Combine(GetProjectRootOrAppDirectory(), "Assets", "button1.wav");
                if (File.Exists(buttonSoundPath))
                {
                    buttonSoundPlayer = new SoundPlayer(buttonSoundPath);
                    buttonSoundPlayer.Load();
                }
                else
                {
                    Console.WriteLine($"Button sound not found: {buttonSoundPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading sound files: {ex.Message}");
            }
        }

        private void PlayButtonSound()
        {
            try
            {
                buttonSoundPlayer?.Play();
            }
            catch { }
        }

        private void CreateFlashAnimation()
        {
            flashStoryboard = new Storyboard();
            var animation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.3,
                Duration = TimeSpan.FromMilliseconds(500),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTarget(animation, TimerDisplay);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));
            flashStoryboard.Children.Add(animation);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var remaining = endTime - DateTime.Now;
            
            if (remaining.TotalSeconds <= 0)
            {
                StopTimer();
                TimerFinished();
                return;
            }

            UpdateTimerDisplay(remaining);
        }

        private void UpdateTimerDisplay(TimeSpan remaining)
        {
            if (remaining.TotalSeconds < 0)
                remaining = TimeSpan.Zero;
                
            int minutes = (int)remaining.TotalMinutes;
            int seconds = remaining.Seconds;
            
            TimerDisplay.Text = $"{minutes:00}:{seconds:00}";
        }

        private void StartTimer()
        {
            flashStoryboard.Stop();
            TimerDisplay.Opacity = 1;
            
            if (!isRunning || isPaused)
            {
                if (!isPaused)
                {
                    // Starting fresh
                    remainingTime = new TimeSpan(0, countdownMinutes, countdownSeconds);
                    hasFinished = false;  // Reset finished flag
                }
                
                // Set end time based on remaining time
                endTime = DateTime.Now.Add(remainingTime);
                
                isRunning = true;
                isPaused = false;
                timer.Start();
                UpdateStatusText();
                
                // Update settings dialog if open
                settingsDialog?.UpdateTimerState(isRunning, isPaused);
            }
        }

        private void StopTimer()
        {
            isRunning = false;
            isPaused = false;
            timer.Stop();
            flashStoryboard.Stop();
            TimerDisplay.Opacity = 1;
            
            // Update settings dialog if open
            settingsDialog?.UpdateTimerState(isRunning, isPaused);
        }

        private void PauseTimer()
        {
            if (isRunning && !isPaused)
            {
                // Save remaining time before pausing
                remainingTime = endTime - DateTime.Now;
                if (remainingTime.TotalSeconds < 0)
                    remainingTime = TimeSpan.Zero;
                    
                isPaused = true;
                isRunning = false;
                timer.Stop();
                
                // Update settings dialog if open
                settingsDialog?.UpdateTimerState(isRunning, isPaused);
            }
        }

        private void RestartTimer()
        {
            StopTimer();
            hasFinished = false;  // Reset finished flag
            remainingTime = new TimeSpan(0, countdownMinutes, countdownSeconds);
            UpdateTimerDisplay(remainingTime);
            StartTimer();
        }

        private void TimerFinished()
        {
            TimerDisplay.Text = "00:00";
            hasFinished = true;  // Mark timer as finished
            StatusText.Text = finishedText;
            
            // Play sound
            try
            {
                soundPlayer?.Play();
            }
            catch { }
            
            // Start flashing
            flashStoryboard.Begin();
        }

        private void UpdateStatusText()
        {
            // Only update if we have a specific state to show
            if (isRunning || isPaused)
            {
                StatusText.Text = activeText;
            }
            else if (hasFinished)
            {
                // Only show finished text if timer has actually completed
                StatusText.Text = finishedText;
            }
            else
            {
                // Timer hasn't started or finished - show no text
                StatusText.Text = "";
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Custom drag implementation to avoid Windows snap behavior
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _startPoint = e.GetPosition(null);
                _isDragging = true;
                this.CaptureMouse();
                
                // Hook mouse events for custom dragging
                this.MouseMove += Window_MouseMove;
                this.MouseLeftButtonUp += Window_MouseLeftButtonUp;
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(null);
                
                // Calculate the offset
                double offsetX = currentPoint.X - _startPoint.X;
                double offsetY = currentPoint.Y - _startPoint.Y;
                
                // Directly set window position without using DragMove
                this.Left += offsetX;
                this.Top += offsetY;
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                this.ReleaseMouseCapture();
                
                // Unhook events
                this.MouseMove -= Window_MouseMove;
                this.MouseLeftButtonUp -= Window_MouseLeftButtonUp;
            }
        }

        private void ResizeGrip_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Allow window resizing from the custom grip
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Use Windows API to resize from bottom-right corner
                try
                {
                    System.Windows.Interop.HwndSource hwndSource = PresentationSource.FromVisual((Visual)sender) as System.Windows.Interop.HwndSource;
                    if (hwndSource != null)
                    {
                        // Send resize message to Windows
                        SendMessage(hwndSource.Handle, 0x112, (IntPtr)0xF008, IntPtr.Zero);
                    }
                }
                catch { }
            }
        }

        // Windows API for resize functionality
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            PlayButtonSound();
            
            // If settings dialog is already open, just bring it to front
            if (settingsDialog != null && settingsDialog.IsLoaded)
            {
                settingsDialog.Activate();
                return;
            }
            
            // Create and show non-modal settings dialog
            settingsDialog = new SettingsDialog
            {
                Owner = this,
                CountdownMinutes = countdownMinutes,
                CountdownSeconds = countdownSeconds,
                ActiveText = activeText,
                FinishedText = finishedText,
                ClockX = config.ClockX,
                ClockY = config.ClockY,
                ClockFontSize = config.ClockFontSize,
                TextX = config.TextX,
                TextY = config.TextY,
                TextFontSize = config.TextFontSize,
                IsTimerRunning = isRunning,
                IsTimerPaused = isPaused,
                PlayButtonSound = PlayButtonSound
            };
            
            // Subscribe to settings changed event
            settingsDialog.SettingsChanged += OnSettingsChanged;
            settingsDialog.Closed += (s, args) => 
            {
                if (settingsDialog != null)
                {
                    settingsDialog.SettingsChanged -= OnSettingsChanged;
                    settingsDialog = null;
                }
            };
            
            // Show as non-modal window
            settingsDialog.Show();
        }
        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            PlayButtonSound();
            
            // Close any open settings dialog
            settingsDialog?.Close();
            
            // Close the main application
            Application.Current.Shutdown();
        }
        
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            PlayButtonSound();
            
            // Minimize the window to taskbar
            WindowState = WindowState.Minimized;
        }
        
        private void OnSettingsChanged(object? sender, SettingsChangedEventArgs e)
        {
            if (settingsDialog == null) return;
            
            // Apply settings
            countdownMinutes = settingsDialog.CountdownMinutes;
            countdownSeconds = settingsDialog.CountdownSeconds;
            activeText = settingsDialog.ActiveText;
            finishedText = settingsDialog.FinishedText;
            
            // Update config with new values
            config.CountdownMinutes = countdownMinutes;
            config.CountdownSeconds = countdownSeconds;
            config.ActiveText = activeText;
            config.FinishedText = finishedText;
            config.ClockX = settingsDialog.ClockX;
            config.ClockY = settingsDialog.ClockY;
            config.ClockFontSize = settingsDialog.ClockFontSize;
            config.TextX = settingsDialog.TextX;
            config.TextY = settingsDialog.TextY;
            config.TextFontSize = settingsDialog.TextFontSize;
            
            // Apply display settings immediately
            ApplyDisplaySettings();
            
            // Save configuration to file
            config.Save();
            
            // Update display text immediately if not running
            if (!isRunning)
            {
                UpdateStatusText();
            }

            // Handle timer control actions
            switch (e.Action)
            {
                case TimerAction.Start:
                    StartTimer();
                    break;
                case TimerAction.Stop:
                    StopTimer();
                    UpdateTimerDisplay(new TimeSpan(0, countdownMinutes, countdownSeconds));
                    UpdateStatusText();
                    break;
                case TimerAction.Pause:
                    PauseTimer();
                    break;
                case TimerAction.Restart:
                    RestartTimer();
                    break;
                case TimerAction.None:
                    // Just settings update, no timer action
                    if (!isRunning && !isPaused)
                    {
                        UpdateTimerDisplay(new TimeSpan(0, countdownMinutes, countdownSeconds));
                    }
                    break;
            }
        }
    }

    public enum TimerAction
    {
        None,
        Start,
        Stop,
        Pause,
        Restart
    }
} 