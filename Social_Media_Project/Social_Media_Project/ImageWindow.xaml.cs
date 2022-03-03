using System;
using System.IO;
using System.Net.Cache;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Social_Media_Project
{
    public partial class ImageWindow : Window
    {
        public ImageWindow(string? imageToSend)
        {
            InitializeComponent();
            image.Source = new BitmapImage(new Uri(imageToSend, UriKind.Absolute), new RequestCachePolicy(RequestCacheLevel.BypassCache)) { CacheOption = BitmapCacheOption.OnLoad };
        }
        public ImageWindow(byte[] tag)
        {
            InitializeComponent();

            var imageFile = new BitmapImage();
            using (var mem = new MemoryStream(tag))
            {
                mem.Position = 0;
                imageFile.BeginInit();
                imageFile.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                imageFile.CacheOption = BitmapCacheOption.OnLoad;
                imageFile.UriSource = null;
                imageFile.StreamSource = mem;
                imageFile.EndInit();
            }
            imageFile.Freeze();
            image.Source = imageFile;
        }

        private void Window_PreviewMouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}
