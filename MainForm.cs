using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AutoClicker.Models;
using AutoClicker.Services;

namespace AutoClicker
{
    public partial class MainForm : Form
    {

        // UI Controls
        private TrackBar frequencyTrackBar;
        private NumericUpDown frequencyNumeric;
        private ComboBox hotkeyComboBox;
        private Label statusLabel;
        private Label clickCountLabel;
        private CheckBox randomDelayCheckBox;
        private NumericUpDown randomDelayNumeric;
        private CheckBox stayOnTopCheckBox;
        private GroupBox settingsGroupBox;
        private GroupBox statusGroupBox;

        // Services
        private readonly ClickEngine _clickEngine;
        private readonly HotkeyManager _hotkeyManager;
        private readonly SystemTrayManager _systemTrayManager;
        private readonly SettingsManager _settingsManager;
        private AppSettings _settings;

        public MainForm()
        {
            _clickEngine = new ClickEngine();
            _hotkeyManager = new HotkeyManager();
            _systemTrayManager = new SystemTrayManager(this);
            _settingsManager = new SettingsManager();

            InitializeComponent();
            InitializeServices();
            LoadSettings();
            this.FormClosing += MainForm_FormClosing;
        }

        private void InitializeComponent()
        {
            this.Text = "AutoClicker Pro";
            this.Size = new Size(455, 460);
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
                Size = new Size(420, 180)
            };

            // Frequency controls
            Label frequencyLabel = new Label
            {
                Text = "Click Frequency (CPS):",
                Location = new Point(15, 25),
                Size = new Size(150, 20)
            };

            frequencyTrackBar = new TrackBar
            {
                Location = new Point(15, 50),
                Size = new Size(300, 45),
                Minimum = 1,
                Maximum = 1000,
                Value = 10,
                TickFrequency = 100
            };
            frequencyTrackBar.ValueChanged += FrequencyTrackBar_ValueChanged;

            frequencyNumeric = new NumericUpDown
            {
                Location = new Point(325, 55),
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
                Location = new Point(15, 110),
                Size = new Size(100, 20)
            };

            hotkeyComboBox = new ComboBox
            {
                Location = new Point(125, 107),
                Size = new Size(100, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            hotkeyComboBox.Items.AddRange(new string[] { "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12" });
            hotkeyComboBox.SelectedIndex = 8; // F9
            hotkeyComboBox.SelectedIndexChanged += HotkeyComboBox_SelectedIndexChanged;

            // Random delay
            randomDelayCheckBox = new CheckBox
            {
                Text = "Random Delay:",
                Location = new Point(15, 145),
                Size = new Size(100, 20)
            };
            randomDelayCheckBox.CheckedChanged += RandomDelayCheckBox_CheckedChanged;

            randomDelayNumeric = new NumericUpDown
            {
                Location = new Point(125, 142),
                Size = new Size(70, 25),
                Minimum = 0,
                Maximum = 1000,
                Value = 50
            };
            randomDelayNumeric.ValueChanged += RandomDelayNumeric_ValueChanged;

            Label randomDelayLabel = new Label
            {
                Text = "ms",
                Location = new Point(200, 145),
                Size = new Size(25, 20)
            };

            // Stay on top checkbox
            stayOnTopCheckBox = new CheckBox
            {
                Text = "Stay on Top",
                Location = new Point(250, 145),
                Size = new Size(100, 20)
            };
            stayOnTopCheckBox.CheckedChanged += StayOnTopCheckBox_CheckedChanged;

            settingsGroupBox.Controls.AddRange(new Control[] {
                frequencyLabel, frequencyTrackBar, frequencyNumeric,
                hotkeyLabel, hotkeyComboBox,
                randomDelayCheckBox, randomDelayNumeric, randomDelayLabel, stayOnTopCheckBox
            });

            // Status GroupBox
            statusGroupBox = new GroupBox
            {
                Text = "Status",
                Location = new Point(10, 200),
                Size = new Size(420, 120)
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
            resetCounterButton.Click += (s, e) => _clickEngine.ResetCounter();

            Button minimizeToTrayButton = new Button
            {
                Text = "Minimize to Tray",
                Location = new Point(290, 75),
                Size = new Size(120, 25)
            };
            minimizeToTrayButton.Click += (s, e) => _systemTrayManager.MinimizeToTray();

            statusGroupBox.Controls.AddRange(new Control[] {
                statusLabel, clickCountLabel, resetCounterButton, minimizeToTrayButton
            });

            // Info GroupBox
            GroupBox infoGroupBox = new GroupBox
            {
                Text = "Information",
                Location = new Point(10, 320),
                Size = new Size(420, 80)
            };

            Label authorLabel = new Label
            {
                Text = "Author: LT",
                Location = new Point(15, 25),
                Size = new Size(160, 20),
                Font = new Font("Segoe UI", 9F)
            };

            Label licenseLabel = new Label
            {
                Text = "License: MIT License",
                Location = new Point(15, 45),
                Size = new Size(160, 20),
                Font = new Font("Segoe UI", 9F)
            };

            Label versionLabel = new Label
            {
                Text = "Version: 2.0.0 (.NET 9)",
                Location = new Point(250, 25),
                Size = new Size(160, 20),
                Font = new Font("Segoe UI", 9F)
            };

            infoGroupBox.Controls.AddRange([
                authorLabel, licenseLabel, versionLabel
            ]);

            this.Controls.AddRange([
                settingsGroupBox, statusGroupBox, infoGroupBox
            ]);
        }



        private void InitializeServices()
        {
            _hotkeyManager.Initialize(this.Handle);
            _systemTrayManager.Initialize();

            _clickEngine.ClickPerformed += OnClickPerformed;
            _clickEngine.StatusChanged += OnStatusChanged;
            _hotkeyManager.HotkeyPressed += OnHotkeyPressed;
        }

        private void FrequencyTrackBar_ValueChanged(object? sender, EventArgs e)
        {
            frequencyNumeric.Value = frequencyTrackBar.Value;
            _clickEngine.SetFrequency((int)frequencyNumeric.Value);
            SaveSettings();
        }

        private void FrequencyNumeric_ValueChanged(object? sender, EventArgs e)
        {
            frequencyTrackBar.Value = (int)frequencyNumeric.Value;
            _clickEngine.SetFrequency((int)frequencyNumeric.Value);
            SaveSettings();
        }

        private void HotkeyComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            _hotkeyManager.RegisterHotkey(hotkeyComboBox.SelectedIndex);
            SaveSettings();
        }

        private void OnClickPerformed(object? sender, ClickEventArgs e)
        {
            if (clickCountLabel.InvokeRequired)
            {
                clickCountLabel.Invoke(new Action(() => clickCountLabel.Text = $"Clicks: {e.ClickCount}"));
            }
            else
            {
                clickCountLabel.Text = $"Clicks: {e.ClickCount}";
            }
        }

        private void OnStatusChanged(object? sender, StatusChangedEventArgs e)
        {
            if (statusLabel.InvokeRequired)
            {
                statusLabel.Invoke(new Action(() =>
                {
                    statusLabel.Text = e.Status;
                    statusLabel.ForeColor = e.IsActive ? Color.Green : Color.Blue;
                }));
            }
            else
            {
                statusLabel.Text = e.Status;
                statusLabel.ForeColor = e.IsActive ? Color.Green : Color.Blue;
            }
        }

        private void OnHotkeyPressed(object? sender, EventArgs e)
        {
            _clickEngine.ToggleClicking();
        }

        private void RandomDelayCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            _clickEngine.SetRandomDelay(randomDelayCheckBox.Checked, (int)randomDelayNumeric.Value);
            SaveSettings();
        }

        private void RandomDelayNumeric_ValueChanged(object? sender, EventArgs e)
        {
            _clickEngine.SetRandomDelay(randomDelayCheckBox.Checked, (int)randomDelayNumeric.Value);
            SaveSettings();
        }

        private void StayOnTopCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            this.TopMost = stayOnTopCheckBox.Checked;
            SaveSettings();
        }

        protected override void WndProc(ref Message m)
        {
            if (!_hotkeyManager.ProcessMessage(ref m))
            {
                base.WndProc(ref m);
            }
        }

        private void LoadSettings()
        {
            _settings = _settingsManager.LoadSettings();

            // Apply loaded settings to UI
            frequencyNumeric.Value = _settings.Frequency;
            frequencyTrackBar.Value = _settings.Frequency;
            hotkeyComboBox.SelectedIndex = _settings.HotkeyIndex;
            randomDelayCheckBox.Checked = _settings.RandomDelay;
            randomDelayNumeric.Value = _settings.RandomDelayValue;
            stayOnTopCheckBox.Checked = _settings.StayOnTop;
            this.TopMost = _settings.StayOnTop;

            // Apply settings to services
            _clickEngine.SetFrequency(_settings.Frequency);
            _clickEngine.SetRandomDelay(_settings.RandomDelay, _settings.RandomDelayValue);
            _hotkeyManager.RegisterHotkey(_settings.HotkeyIndex);
        }

        private void SaveSettings()
        {
            _settings.Frequency = (int)frequencyNumeric.Value;
            _settings.HotkeyIndex = hotkeyComboBox.SelectedIndex;
            _settings.RandomDelay = randomDelayCheckBox.Checked;
            _settings.RandomDelayValue = (int)randomDelayNumeric.Value;
            _settings.StayOnTop = stayOnTopCheckBox.Checked;

            _settingsManager.SaveSettings(_settings);

            // Update services with new settings
            _clickEngine.SetRandomDelay(_settings.RandomDelay, _settings.RandomDelayValue);
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _hotkeyManager.Dispose();
            _systemTrayManager.Dispose();
            _clickEngine.Dispose();
        }
    }
}