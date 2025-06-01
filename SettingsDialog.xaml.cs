using System;
using System.Windows;

namespace RTEventTimer
{
    public partial class SettingsDialog : Window
    {
        public int CountdownMinutes { get; set; }
        public int CountdownSeconds { get; set; }
        public string ActiveText { get; set; }
        public string FinishedText { get; set; }
        public bool IsTimerRunning { get; set; }
        public bool IsTimerPaused { get; set; }
        
        // Display settings
        public double ClockX { get; set; }
        public double ClockY { get; set; }
        public double ClockFontSize { get; set; }
        public double TextX { get; set; }
        public double TextY { get; set; }
        public double TextFontSize { get; set; }
        
        // Action to play button sound
        public Action? PlayButtonSound { get; set; }
        
        // Event to notify main window of changes
        public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

        public SettingsDialog()
        {
            InitializeComponent();
            DataContext = this;
            
            // Set default values if properties haven't been set
            if (string.IsNullOrEmpty(ActiveText))
                ActiveText = TimerDefaults.ActiveTimerText;
            if (string.IsNullOrEmpty(FinishedText))
                FinishedText = TimerDefaults.FinishedTimerText;
                
            // Handle window closing to save settings
            this.Closing += Window_Closing;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // Initialize controls with current values
            MinutesTextBox.Text = CountdownMinutes.ToString();
            SecondsTextBox.Text = CountdownSeconds.ToString();
            ActiveTextBox.Text = ActiveText;
            FinishedTextBox.Text = FinishedText;
            
            // Initialize display settings
            ClockXTextBox.Text = ClockX.ToString();
            ClockYTextBox.Text = ClockY.ToString();
            ClockFontSizeTextBox.Text = ClockFontSize.ToString();
            TextXTextBox.Text = TextX.ToString();
            TextYTextBox.Text = TextY.ToString();
            TextFontSizeTextBox.Text = TextFontSize.ToString();
            
            // Update button states based on timer state
            UpdateButtonStates();
        }

        public void UpdateTimerState(bool isRunning, bool isPaused)
        {
            IsTimerRunning = isRunning;
            IsTimerPaused = isPaused;
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            if (PauseButton != null)
            {
                // Update Pause/Continue button text and state
                if (IsTimerPaused)
                {
                    PauseButton.Content = "Continue";
                    PauseButton.IsEnabled = true;
                }
                else if (IsTimerRunning)
                {
                    PauseButton.Content = "Pause";
                    PauseButton.IsEnabled = true;
                }
                else
                {
                    PauseButton.Content = "Pause";
                    PauseButton.IsEnabled = false;
                }
                
                StopButton.IsEnabled = IsTimerRunning || IsTimerPaused;
                RestartButton.IsEnabled = true;
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            PlayButtonSound?.Invoke();
            
            if (IsTimerPaused)
            {
                // Resume timer
                if (ValidateAndUpdateSettings())
                {
                    SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(TimerAction.Start));
                    IsTimerRunning = true;
                    IsTimerPaused = false;
                    UpdateButtonStates();
                }
            }
            else if (IsTimerRunning)
            {
                // Pause timer
                SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(TimerAction.Pause));
                IsTimerPaused = true;
                IsTimerRunning = false;
                UpdateButtonStates();
            }
            else
            {
                // Start timer from stopped state
                if (ValidateAndUpdateSettings())
                {
                    SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(TimerAction.Start));
                    IsTimerRunning = true;
                    IsTimerPaused = false;
                    UpdateButtonStates();
                }
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            PlayButtonSound?.Invoke();
            SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(TimerAction.Stop));
            IsTimerRunning = false;
            IsTimerPaused = false;
            UpdateButtonStates();
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            PlayButtonSound?.Invoke();
            if (ValidateAndUpdateSettings())
            {
                SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(TimerAction.Restart));
                IsTimerRunning = true;
                IsTimerPaused = false;
                UpdateButtonStates();
            }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            PlayButtonSound?.Invoke();
            if (ValidateAndUpdateSettings())
            {
                SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(TimerAction.None));
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            PlayButtonSound?.Invoke();
            
            // Validate and save settings before closing
            if (ValidateAndUpdateSettings())
            {
                SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(TimerAction.None));
            }
            
            Close();
        }

        private bool ValidateAndUpdateSettings()
        {
            // Validate and parse input
            if (!int.TryParse(MinutesTextBox.Text, out int minutes) || minutes < 0)
            {
                MessageBox.Show("Please enter a valid number of minutes (0 or greater).", 
                    "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(SecondsTextBox.Text, out int seconds) || seconds < 0 || seconds > 59)
            {
                MessageBox.Show("Please enter a valid number of seconds (0-59).", 
                    "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (minutes == 0 && seconds == 0)
            {
                MessageBox.Show("Timer duration must be greater than 0.", 
                    "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            // Validate display settings
            if (!double.TryParse(ClockXTextBox.Text, out double clockX))
            {
                MessageBox.Show("Please enter a valid number for Clock X position.", 
                    "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            if (!double.TryParse(ClockYTextBox.Text, out double clockY))
            {
                MessageBox.Show("Please enter a valid number for Clock Y position.", 
                    "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            if (!double.TryParse(ClockFontSizeTextBox.Text, out double clockFontSize) || clockFontSize <= 0)
            {
                MessageBox.Show("Please enter a valid font size for Clock (greater than 0).", 
                    "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            if (!double.TryParse(TextXTextBox.Text, out double textX))
            {
                MessageBox.Show("Please enter a valid number for Text X position.", 
                    "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            if (!double.TryParse(TextYTextBox.Text, out double textY))
            {
                MessageBox.Show("Please enter a valid number for Text Y position.", 
                    "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            if (!double.TryParse(TextFontSizeTextBox.Text, out double textFontSize) || textFontSize <= 0)
            {
                MessageBox.Show("Please enter a valid font size for Text (greater than 0).", 
                    "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Update properties
            CountdownMinutes = minutes;
            CountdownSeconds = seconds;
            ActiveText = string.IsNullOrWhiteSpace(ActiveTextBox.Text) ? TimerDefaults.ActiveTimerText : ActiveTextBox.Text;
            FinishedText = string.IsNullOrWhiteSpace(FinishedTextBox.Text) ? TimerDefaults.FinishedTimerText : FinishedTextBox.Text;
            
            // Update display properties
            ClockX = clockX;
            ClockY = clockY;
            ClockFontSize = clockFontSize;
            TextX = textX;
            TextY = textY;
            TextFontSize = textFontSize;

            return true;
        }

        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Only validate and save if not already handled by Close button
            // Check if the settings have changed by comparing with the textbox values
            bool hasChanges = false;
            
            if (ActiveTextBox != null && ActiveTextBox.Text != ActiveText)
                hasChanges = true;
            if (FinishedTextBox != null && FinishedTextBox.Text != FinishedText)
                hasChanges = true;
            
            if (hasChanges && ValidateAndUpdateSettings())
            {
                SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(TimerAction.None));
            }
        }
    }

    public class SettingsChangedEventArgs : EventArgs
    {
        public TimerAction Action { get; }
        
        public SettingsChangedEventArgs(TimerAction action)
        {
            Action = action;
        }
    }
} 