using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AutoClicker.Services
{
    public class SystemTrayManager
    {
        private NotifyIcon? _notifyIcon;
        private ContextMenuStrip? _trayMenu;
        private Form _parentForm;

        public SystemTrayManager(Form parentForm)
        {
            _parentForm = parentForm;
        }

        public void Initialize()
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
            
            _notifyIcon = new NotifyIcon
            {
                Icon = trayIcon,
                Text = "AutoClicker Pro",
                Visible = false
            };

            _trayMenu = new ContextMenuStrip();
            _trayMenu.Items.Add("Show", null, OnShowClicked);
            _trayMenu.Items.Add("-");
            _trayMenu.Items.Add("Exit", null, OnExitClicked);

            _notifyIcon.ContextMenuStrip = _trayMenu;
            _notifyIcon.DoubleClick += OnNotifyIconDoubleClick;
        }

        public void MinimizeToTray()
        {
            if (_notifyIcon != null)
            {
                _parentForm.Hide();
                _notifyIcon.Visible = true;
                _notifyIcon.ShowBalloonTip(2000, "AutoClicker Pro", "Application minimized to tray", ToolTipIcon.Info);
            }
        }

        public void RestoreFromTray()
        {
            if (_notifyIcon != null)
            {
                _parentForm.Show();
                _parentForm.WindowState = FormWindowState.Normal;
                _notifyIcon.Visible = false;
            }
        }

        private void OnShowClicked(object? sender, EventArgs e)
        {
            RestoreFromTray();
        }

        private void OnExitClicked(object? sender, EventArgs e)
        {
            Application.Exit();
        }

        private void OnNotifyIconDoubleClick(object? sender, EventArgs e)
        {
            RestoreFromTray();
        }

        public void Dispose()
        {
            _notifyIcon?.Dispose();
            _trayMenu?.Dispose();
        }
    }
}