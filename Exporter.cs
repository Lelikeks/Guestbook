using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Guestbook
{
    static class Exporter
    {
        public static void ExportToJpeg(string path, RenderTargetBitmap renderBitmap)
        {
            if (path == null) return;

            using (FileStream fs = File.Open(path, FileMode.Create))
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                encoder.Save(fs);
            }
        }

        public static void ExportToJpeg(string path, InkCanvas surface)
        {
            if (path == null) return;

            Size size = new Size(surface.ActualWidth, surface.ActualHeight);

            RenderTargetBitmap renderBitmap =
             new RenderTargetBitmap(
               (int)size.Width,
               (int)size.Height,
               96,
               96,
               PixelFormats.Default);
            renderBitmap.Render(surface);

            ExportToJpeg(path, renderBitmap);
        }
    }
}
