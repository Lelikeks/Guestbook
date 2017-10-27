using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Guestbook
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double LeftRatio { get { return ParseDouble(Properties.Settings.Default.ButtonLeft); } }
        double TopRatio { get { return ParseDouble(Properties.Settings.Default.ButtonTop); } }
        double WidthRatio { get { return ParseDouble(Properties.Settings.Default.ButtonWidth); } }
        double HeightRatio { get { return ParseDouble(Properties.Settings.Default.ButtonHeight); } }

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                SetImage();

                if (Properties.Settings.Default.ButtonVisible)
                {
                    save.Opacity = 0.75;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("{0}: {1}", DateTime.Now, ex.ToString()));
            }
        }

        private static double ParseDouble(string value)
        {
            var nfi = (NumberFormatInfo)Thread.CurrentThread.CurrentCulture.NumberFormat.Clone();
            nfi.NumberDecimalSeparator = ".";

            return double.Parse(value, nfi);
        }

        private void SetImage()
        {
            var imagePath = Properties.Settings.Default.Background;
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                MessageBox.Show(@"Background parameter didn't set in the config file. There should be a path to a picture (e.g. C:\Images\livre2.jpg)");
            }
            else
            {
                if (!Path.IsPathRooted(imagePath))
                {
                    imagePath = Path.GetFullPath(imagePath);
                }
                if (!File.Exists(imagePath))
                {
                    MessageBox.Show(string.Format("Background image at the path {0} doesn't exists.", imagePath));
                    return;
                }
                inkCanvas.Background = new ImageBrush(new BitmapImage(new Uri(imagePath)));
            }
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)inkCanvas.ActualWidth, (int)inkCanvas.ActualHeight, 96, 96, PixelFormats.Default);
                renderBitmap.Render(inkCanvas);

                if (Properties.Settings.Default.UseAnimation)
                {
                    dirty.Source = renderBitmap;
                    dirty.Visibility = Visibility.Visible;

                    inkCanvas.Strokes.Clear();

                    var x = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.6))
                    {
                        EasingFunction = new PowerEase { EasingMode = EasingMode.EaseIn, Power = 2 }
                    };
                    scale.BeginAnimation(ScaleTransform.ScaleXProperty, x);
                }
                else
                {
                    inkCanvas.Strokes.Clear();
                }

                if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.ImagesFolder) && !Directory.Exists(Properties.Settings.Default.ImagesFolder))
                {
                    Directory.CreateDirectory(Properties.Settings.Default.ImagesFolder);
                }

                var fileName = Path.Combine(Properties.Settings.Default.ImagesFolder, string.Format("{0}.jpg", DateTime.Now.ToString("yyyyMMdd_Hmmssfff")));
                Exporter.ExportToJpeg(fileName, renderBitmap);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("{0}: {1}", DateTime.Now, ex.ToString()));
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                Canvas.SetLeft(save, inkCanvas.ActualWidth * LeftRatio);
                Canvas.SetTop(save, inkCanvas.ActualHeight * TopRatio);
                save.Width = inkCanvas.ActualWidth * WidthRatio;
                save.Height = inkCanvas.ActualHeight * HeightRatio;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("{0}: {1}", DateTime.Now, ex.ToString()));
            }
        }
    }
}
