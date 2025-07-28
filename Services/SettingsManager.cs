using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using AutoClicker.Models;

namespace AutoClicker.Services
{
    public class SettingsManager
    {
        private readonly string _settingsPath;

        public SettingsManager()
        {
            _settingsPath = Path.Combine(Application.StartupPath, "settings.json");
        }

        public AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    string json = File.ReadAllText(_settingsPath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                else
                {
                    return new AppSettings();
                }
            }
            catch
            {
                return new AppSettings();
            }
        }

        public void SaveSettings(AppSettings settings)
        {
            try
            {
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsPath, json);
            }
            catch
            {
                // Ignore save errors
            }
        }
    }
}