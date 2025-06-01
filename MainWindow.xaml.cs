using System;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Reflection;

namespace RTEventTimer
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private DateTime endTime;
        private TimeSpan remainingTime;
        private bool isRunning;
        private bool isPaused;
        private SoundPlayer soundPlayer;
        private SoundPlayer buttonSoundPlayer;
        private Storyboard flashStoryboard;
        private SettingsDialog? settingsDialog;
        
        // Settings - initialized from defaults
        private string activeText = TimerDefaults.ActiveTimerText;
        private string finishedText = TimerDefaults.FinishedTimerText;
        private int countdownMinutes = TimerDefaults.DefaultMinutes;
        private int countdownSeconds = TimerDefaults.DefaultSeconds;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimer();
            InitializeSoundPlayer();
            CreateFlashAnimation();
            LoadBackgroundImage();
            
            // Initialize display with default time
            UpdateTimerDisplay(new TimeSpan(0, countdownMinutes, countdownSeconds));
            
            // Don't override the initial XAML text unless timer is running
            // This allows custom initial messages in XAML
        }

        private void LoadBackgroundImage()
        {
            try
            {
                string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "background.png");
                
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
                string soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "timer_finished.wav");
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
                string buttonSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "button1.wav");
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
            int tenths = remaining.Milliseconds / 100;
            
            TimerDisplay.Text = $"{minutes:00}:{seconds:00}.{tenths}";
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
            remainingTime = new TimeSpan(0, countdownMinutes, countdownSeconds);
            UpdateTimerDisplay(remainingTime);
            StartTimer();
        }

        private void TimerFinished()
        {
            TimerDisplay.Text = "00:00.0";
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
            else if (TimerDisplay.Text == "00:00.0")
            {
                StatusText.Text = finishedText;
            }
            // Otherwise, leave the text as set in XAML or by user
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Allow window dragging
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

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
                IsTimerRunning = isRunning,
                IsTimerPaused = isPaused
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
        
        private void OnSettingsChanged(object? sender, SettingsChangedEventArgs e)
        {
            if (settingsDialog == null) return;
            
            // Apply settings
            countdownMinutes = settingsDialog.CountdownMinutes;
            countdownSeconds = settingsDialog.CountdownSeconds;
            activeText = settingsDialog.ActiveText;
            finishedText = settingsDialog.FinishedText;
            
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