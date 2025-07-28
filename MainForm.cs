using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Text.Json;

namespace AutoClicker
{
    public partial class MainForm : Form
    {
        // Win32 API imports for mouse clicking and global hotkeys
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out Point lpPoint);

        // Mouse event constants
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 9000;

        // UI Controls
        private TrackBar frequencyTrackBar;
        private NumericUpDown frequencyNumeric;
        private ComboBox hotkeyComboBox;
        private Label statusLabel;
        private Label clickCountLabel;
        private CheckBox randomDelayCheckBox;
        private NumericUpDown randomDelayNumeric;
        private NotifyIcon notifyIcon;
        private ContextMenuStrip trayMenu;
        private GroupBox settingsGroupBox;
        private GroupBox statusGroupBox;

        // Application state
        private bool isClicking = false;
        private System.Threading.Timer clickTimer;
        private int clickCount = 0;
        private Random random = new Random();
        private uint currentHotkey = 0x78; // F9 by default
        private AppSettings settings;

        public MainForm()
        {
            InitializeComponent();
            LoadSettings();
            SetupNotifyIcon();
            RegisterGlobalHotkey();
            this.FormClosing += MainForm_FormClosing;
        }

        private void InitializeComponent()
        {
            this.Text = "AutoClicker Pro";
            this.Size = new Size(450, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            // Load custom icon if available, fallback to system icon
            try
            {
                if (File.Exists("icon.ico"))
                {
                    this.Icon = new Icon("icon.ico");
                }
                else
                {
                    this.Icon = SystemIcons.Application;
                }
            }
            catch
            {
                this.Icon = SystemIcons.Application;
            }

            // Settings GroupBox
            settingsGroupBox = new GroupBox
            {
                Text = "Settings",
                Location = new Point(10, 10),
                Size = new Size(420, 200)
            };

            // Frequency controls
            Label frequencyLabel = new Label
            {
                Text = "Click Frequency (CPS):",
                Location = new Point(10, 25),
                Size = new Size(120, 20)
            };

            frequencyTrackBar = new TrackBar
            {
                Location = new Point(10, 50),
                Size = new Size(300, 45),
                Minimum = 1,
                Maximum = 1000,
                Value = 10,
                TickFrequency = 100
            };
            frequencyTrackBar.ValueChanged += FrequencyTrackBar_ValueChanged;

            frequencyNumeric = new NumericUpDown
            {
                Location = new Point(320, 55),
                Size = new Size(80, 25),
                Minimum = 1,
                Maximum = 1000,
                Value = 10
            };
            frequencyNumeric.ValueChanged += FrequencyNumeric_ValueChanged;

            // Hotkey selection
            Label hotkeyLabel = new Label
            {
                Text = "Toggle Hotkey:",
                Location = new Point(10, 105),
                Size = new Size(100, 20)
            };

            hotkeyComboBox = new ComboBox
            {
                Location = new Point(120, 102),
                Size = new Size(100, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            hotkeyComboBox.Items.AddRange(new string[] { "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12" });
            hotkeyComboBox.SelectedIndex = 8; // F9
            hotkeyComboBox.SelectedIndexChanged += HotkeyComboBox_SelectedIndexChanged;

            // Random delay
            randomDelayCheckBox = new CheckBox
            {
                Text = "Random Delay (±ms):",
                Location = new Point(10, 135),
                Size = new Size(130, 20)
            };

            randomDelayNumeric = new NumericUpDown
            {
                Location = new Point(150, 132),
                Size = new Size(70, 25),
                Minimum = 0,
                Maximum = 1000,
                Value = 50
            };

            settingsGroupBox.Controls.AddRange(new Control[] {
                frequencyLabel, frequencyTrackBar, frequencyNumeric,
                hotkeyLabel, hotkeyComboBox,
                randomDelayCheckBox, randomDelayNumeric,
            });

            // Status GroupBox
            statusGroupBox = new GroupBox
            {
                Text = "Status",
                Location = new Point(10, 310),
                Size = new Size(420, 110)
            };

            statusLabel = new Label
            {
                Text = "Status: Ready",
                Location = new Point(10, 25),
                Size = new Size(200, 20),
                Font = new Font("Arial", 9, FontStyle.Bold)
            };

            clickCountLabel = new Label
            {
                Text = "Clicks: 0",
                Location = new Point(10, 50),
                Size = new Size(200, 20)
            };

            Button resetCounterButton = new Button
            {
                Text = "Reset Counter",
                Location = new Point(180, 75),
                Size = new Size(100, 25)
            };
            resetCounterButton.Click += (s, e) => {
                clickCount = 0;
                UpdateClickCount();
            };

            Button minimizeToTrayButton = new Button
            {
                Text = "Minimize to Tray",
                Location = new Point(290, 75),
                Size = new Size(120, 25)
            };
            minimizeToTrayButton.Click += (s, e) => {
                this.Hide();
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(2000, "AutoClicker Pro", "Application minimized to tray", ToolTipIcon.Info);
            };

            statusGroupBox.Controls.AddRange(new Control[] {
                statusLabel, clickCountLabel, resetCounterButton, minimizeToTrayButton
            });

            this.Controls.AddRange(new Control[] {
                settingsGroupBox, statusGroupBox
            });
        }

        private void SetupNotifyIcon()
        {
            Icon trayIcon;
            try
            {
                if (File.Exists("icon.ico"))
                {
                    trayIcon = new Icon("icon.ico");
                }
                else
                {
                    trayIcon = SystemIcons.Application;
                }
            }
            catch
            {
                trayIcon = SystemIcons.Application;
            }
            
            notifyIcon = new NotifyIcon
            {
                Icon = trayIcon,
                Text = "AutoClicker Pro",
                Visible = false
            };

            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Show", null, (s, e) => {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                notifyIcon.Visible = false;
            });
            trayMenu.Items.Add("-");
            trayMenu.Items.Add("Exit", null, (s, e) => {
                Application.Exit();
            });

            notifyIcon.ContextMenuStrip = trayMenu;
            notifyIcon.DoubleClick += (s, e) => {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                notifyIcon.Visible = false;
            };
        }

        private void FrequencyTrackBar_ValueChanged(object? sender, EventArgs e)
        {
            frequencyNumeric.Value = frequencyTrackBar.Value;
        }

        private void FrequencyNumeric_ValueChanged(object? sender, EventArgs e)
        {
            frequencyTrackBar.Value = (int)frequencyNumeric.Value;
        }

        private void HotkeyComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UnregisterHotKey(this.Handle, HOTKEY_ID);
            currentHotkey = (uint)(0x70 + hotkeyComboBox.SelectedIndex); // F1-F12
            RegisterGlobalHotkey();
        }

        private void ToggleClicking()
        {
            if (isClicking)
            {
                StopClicking();
            }
            else
            {
                StartClicking();
            }
        }

        private void StartClicking()
        {
            isClicking = true;
            statusLabel.Text = "Status: Clicking Active (Use hotkey to stop)";
            statusLabel.ForeColor = Color.Green;

            int interval = 1000 / (int)frequencyNumeric.Value;
            clickTimer = new System.Threading.Timer(PerformClick, null, 0, interval);
        }

        private void StopClicking()
        {
            isClicking = false;
            clickTimer?.Dispose();
            statusLabel.Text = "Status: Ready (Use hotkey to start)";
            statusLabel.ForeColor = Color.Black;
        }

        private void PerformClick(object? state)
        {
            if (!isClicking) return;

            // Always use current cursor position
            GetCursorPos(out Point targetPos);

            // Apply random delay if enabled
            if (randomDelayCheckBox.Checked)
            {
                int delay = random.Next(-(int)randomDelayNumeric.Value, (int)randomDelayNumeric.Value + 1);
                if (delay > 0)
                {
                    Thread.Sleep(delay);
                }
            }

            // Perform the click
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)targetPos.X, (uint)targetPos.Y, 0, 0);
            
            clickCount++;
            this.Invoke(new Action(UpdateClickCount));
        }

        private void UpdateClickCount()
        {
            clickCountLabel.Text = $"Clicks: {clickCount:N0}";
        }

        private void RegisterGlobalHotkey()
        {
            RegisterHotKey(this.Handle, HOTKEY_ID, 0, currentHotkey);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_HOTKEY)
            {
                ToggleClicking();
            }
        }

        private void LoadSettings()
        {
            try
            {
                string settingsPath = Path.Combine(Application.StartupPath, "settings.json");
                if (File.Exists(settingsPath))
                {
                    string json = File.ReadAllText(settingsPath);
                    settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                else
                {
                    settings = new AppSettings();
                }
            }
            catch
            {
                settings = new AppSettings();
            }
        }

        private void SaveSettings()
        {
            try
            {
                settings.Frequency = (int)frequencyNumeric.Value;
                settings.HotkeyIndex = hotkeyComboBox.SelectedIndex;
                settings.RandomDelay = randomDelayCheckBox.Checked;
                settings.RandomDelayValue = (int)randomDelayNumeric.Value;

                string settingsPath = Path.Combine(Application.StartupPath, "settings.json");
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(settingsPath, json);
            }
            catch { }
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            StopClicking();
            UnregisterHotKey(this.Handle, HOTKEY_ID);
            SaveSettings();
            notifyIcon?.Dispose();
        }
    }

    public class AppSettings
    {
        public int Frequency { get; set; } = 10;
        public int HotkeyIndex { get; set; } = 8; // F9
        public bool RandomDelay { get; set; } = false;
        public int RandomDelayValue { get; set; } = 50;
    }
}