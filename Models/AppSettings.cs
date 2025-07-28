using System.Drawing;

namespace AutoClicker.Models
{
    public class AppSettings
    {
        public int Frequency { get; set; } = 10;
        public int HotkeyIndex { get; set; } = 8; // F9
        public bool RandomDelay { get; set; } = false;
        public int RandomDelayValue { get; set; } = 50;
        public bool StayOnTop { get; set; } = false;
    }
}