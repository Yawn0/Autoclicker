using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace AutoClicker.Services
{
    public class ClickEngine
    {
        // Win32 API imports for mouse clicking and cursor control
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern bool ClipCursor(ref Rectangle lpRect);

        [DllImport("user32.dll")]
        public static extern bool ClipCursor(IntPtr lpRect);

        // Mouse event constants
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        private System.Threading.Timer? _clickTimer;
        private bool _isClicking = false;
        private int _clickCount = 0;
        private Random _random = new Random();
        private int _frequency = 10;
        private bool _randomDelayEnabled = false;
        private int _randomDelayValue = 50;
        private Point _lockedPosition;

        public event EventHandler<ClickEventArgs>? ClickPerformed;
        public event EventHandler<StatusChangedEventArgs>? StatusChanged;

        public bool IsClicking => _isClicking;
        public int ClickCount => _clickCount;

        public void SetFrequency(int frequency)
        {
            _frequency = frequency;
            if (_isClicking)
            {
                // Restart timer with new frequency
                StopClicking();
                StartClicking();
            }
        }

        public void SetRandomDelay(bool enabled, int value)
        {
            _randomDelayEnabled = enabled;
            _randomDelayValue = value;
        }

        public void StartClicking()
        {
            if (_isClicking) return;

            // Capture current cursor position and lock it
            GetCursorPos(out _lockedPosition);
            
            _isClicking = true;
            StatusChanged?.Invoke(this, new StatusChangedEventArgs("Status: Clicking Active", true));

            int interval = 1000 / _frequency;
            _clickTimer = new System.Threading.Timer(PerformClick, null, 0, interval);
        }

        public void StopClicking()
        {
            if (!_isClicking) return;

            _isClicking = false;
            _clickTimer?.Dispose();
            _clickTimer = null;
            
            // Release cursor lock
            ClipCursor(IntPtr.Zero);
            
            StatusChanged?.Invoke(this, new StatusChangedEventArgs("Status: Ready", false));
        }

        public void ToggleClicking()
        {
            if (_isClicking)
            {
                StopClicking();
            }
            else
            {
                StartClicking();
            }
        }

        public void ResetCounter()
        {
            _clickCount = 0;
            ClickPerformed?.Invoke(this, new ClickEventArgs(_clickCount));
        }

        private void PerformClick(object? state)
        {
            if (!_isClicking) return;

            // Ensure cursor stays at locked position
            SetCursorPos(_lockedPosition.X, _lockedPosition.Y);

            // Apply random delay if enabled
            if (_randomDelayEnabled)
            {
                int delay = _random.Next(-_randomDelayValue, _randomDelayValue + 1);
                if (delay > 0)
                {
                    Thread.Sleep(delay);
                }
            }

            // Perform the click at locked position
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)_lockedPosition.X, (uint)_lockedPosition.Y, 0, 0);
            
            _clickCount++;
            ClickPerformed?.Invoke(this, new ClickEventArgs(_clickCount));
        }

        public void Dispose()
        {
            StopClicking();
            // Ensure cursor lock is released
            ClipCursor(IntPtr.Zero);
        }
    }

    public class ClickEventArgs : EventArgs
    {
        public int ClickCount { get; }

        public ClickEventArgs(int clickCount)
        {
            ClickCount = clickCount;
        }
    }

    public class StatusChangedEventArgs : EventArgs
    {
        public string Status { get; }
        public bool IsActive { get; }

        public StatusChangedEventArgs(string status, bool isActive)
        {
            Status = status;
            IsActive = isActive;
        }
    }
}