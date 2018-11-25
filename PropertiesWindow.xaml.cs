using System;
using System.Windows;

namespace ImgViewer
{
    /// <summary>
    /// Вікно властивостей зображення
    /// </summary>
    public sealed partial class PropertiesWindow : Window
    {
        public PropertiesWindow(Img img)
        {
            this.DataContext = img;
            InitializeComponent();
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}