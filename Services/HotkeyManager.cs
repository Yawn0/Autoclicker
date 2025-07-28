using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AutoClicker.Services
{
    public class HotkeyManager
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 9000;

        private IntPtr _windowHandle;
        private uint _currentHotkey = 0x78; // F9 by default
        private bool _isRegistered = false;

        public event EventHandler? HotkeyPressed;

        public void Initialize(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
        }

        public void RegisterHotkey(int hotkeyIndex)
        {
            UnregisterCurrentHotkey();
            _currentHotkey = (uint)(0x70 + hotkeyIndex); // F1-F12
            _isRegistered = RegisterHotKey(_windowHandle, HOTKEY_ID, 0, _currentHotkey);
        }

        public void UnregisterCurrentHotkey()
        {
            if (_isRegistered)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID);
                _isRegistered = false;
            }
        }

        public bool ProcessMessage(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            UnregisterCurrentHotkey();
        }
    }
}