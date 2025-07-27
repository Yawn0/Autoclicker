using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace AutoClicker
{
    public static class IconGenerator
    {
        public static void CreateIcon()
        {
            // Create a 64x64 bitmap
            using (var bitmap = new Bitmap(64, 64))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphics.Clear(Color.Transparent);

                // Draw background circle (blue)
                using (var blueBrush = new SolidBrush(Color.FromArgb(255, 74, 144, 226)))
                using (var bluePen = new Pen(Color.FromArgb(255, 46, 92, 138), 2))
                {
                    graphics.FillEllipse(blueBrush, 2, 2, 60, 60);
                    graphics.DrawEllipse(bluePen, 2, 2, 60, 60);
                }

                // Draw mouse cursor shape (white)
                Point[] cursorPoints = {
                    new Point(20, 16), new Point(20, 40), new Point(24, 36),
                    new Point(28, 44), new Point(32, 42), new Point(28, 34),
                    new Point(34, 34), new Point(20, 16)
                };
                
                using (var whiteBrush = new SolidBrush(Color.White))
                using (var grayPen = new Pen(Color.FromArgb(255, 51, 51, 51), 1))
                {
                    graphics.FillPolygon(whiteBrush, cursorPoints);
                    graphics.DrawPolygon(grayPen, cursorPoints);
                }

                // Draw click indicator circles (gold)
                using (var goldBrush = new SolidBrush(Color.FromArgb(200, 255, 215, 0)))
                {
                    graphics.FillEllipse(goldBrush, 39, 19, 6, 6);
                    graphics.FillEllipse(goldBrush, 44, 30, 4, 4);
                    graphics.FillEllipse(goldBrush, 41, 39, 6, 6);
                }

                // Save as ICO file
                SaveAsIcon(bitmap, "icon.ico");
            }
        }

        private static void SaveAsIcon(Bitmap bitmap, string fileName)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Create))
            using (var writer = new BinaryWriter(fileStream))
            {
                // ICO header
                writer.Write((short)0); // Reserved
                writer.Write((short)1); // Type (1 = ICO)
                writer.Write((short)1); // Number of images

                // Image directory entry
                writer.Write((byte)64); // Width
                writer.Write((byte)64); // Height
                writer.Write((byte)0);  // Color count
                writer.Write((byte)0);  // Reserved
                writer.Write((short)1); // Color planes
                writer.Write((short)32); // Bits per pixel

                // Convert bitmap to PNG for embedding
                using (var pngStream = new MemoryStream())
                {
                    bitmap.Save(pngStream, ImageFormat.Png);
                    var pngData = pngStream.ToArray();

                    writer.Write(pngData.Length); // Size of image data
                    writer.Write(22); // Offset to image data

                    // Write PNG data
                    writer.Write(pngData);
                }
            }
        }
    }
}