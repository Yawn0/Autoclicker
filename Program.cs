using System;
using System.IO;
using System.Windows.Forms;

namespace AutoClicker
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Generate icon if it doesn't exist
            if (!File.Exists("icon.ico"))
            {
                try
                {
                    IconGenerator.CreateIcon();
                }
                catch
                {
                    // Ignore icon generation errors
                }
            }
            
            // Enable visual styles and set compatible text rendering
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Set high DPI awareness for better display on high-resolution screens
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            
            // Run the main form
            Application.Run(new MainForm());
        }
    }
}