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
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // Initialize controls with current values
            MinutesTextBox.Text = CountdownMinutes.ToString();
            SecondsTextBox.Text = CountdownSeconds.ToString();
            ActiveTextBox.Text = ActiveText;
            FinishedTextBox.Text = FinishedText;
            
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
            SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(TimerAction.Stop));
            IsTimerRunning = false;
            IsTimerPaused = false;
            UpdateButtonStates();
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
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
            if (ValidateAndUpdateSettings())
            {
                SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(TimerAction.None));
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
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

            // Update properties
            CountdownMinutes = minutes;
            CountdownSeconds = seconds;
            ActiveText = string.IsNullOrWhiteSpace(ActiveTextBox.Text) ? TimerDefaults.ActiveTimerText : ActiveTextBox.Text;
            FinishedText = string.IsNullOrWhiteSpace(FinishedTextBox.Text) ? TimerDefaults.FinishedTimerText : FinishedTextBox.Text;

            return true;
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