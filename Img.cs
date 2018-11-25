using ImgViewer.Localization;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ImgViewer
{
    public sealed class Img : IDisposable, INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
        #region Properties
        private Stream stream;
        private double _zoom = 0.1;
        private int _mouseX, _mouseY, _angle;

        // Шлях до зображення
        public string Source { get; private set; }
        // Назва зображення
        public string Name { get; private set; }
        // Тип зображення
        public string Extension { get; private set; }
        // Розмір файлу
        public string FileSize { get; private set; }
        // Растрове зображення
        public BitmapImage Bmp { get; private set; }

        // Розміри зображення
        public double Width { get; private set; }
        public double Height { get; private set; }

        // Координати миші на картинці
        public int MouseX
        {
            get => _mouseX;
            set
            {
                _mouseX = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MouseX"));
            }
        }
        public int MouseY
        {
            get => _mouseY;
            set
            {
                _mouseY = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MouseY"));
            }
        }

        // Кут нахилу
        public int Angle
        {
            get => _angle;
            set
            {
                _angle = value;
                // Якщо кут не в діапазоні 0 - 360
                if (_angle >= 360) _angle = 0;
                else if (_angle < 0) _angle = 270;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Angle)));
            }
        }

        // Розміри зображення
        public string Size => $"{Math.Round(Width)} : {Math.Round(Height)}";

        // Масштабування
        public double Zoom
        {
            get => _zoom;
            set
            {
                _zoom = value;
                // Валідація значення
                if (_zoom < MinZoom) _zoom = 0.1;
                else if (_zoom > MaxZoom) _zoom = MaxZoom;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Zoom)));
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(PercentZoom)));
                }
            }
        }
        // Мінімальне масштабування
        public double MinZoom => 0.1;
        // Максимальне масштабування
        public double MaxZoom => 5.1;
        // Масштабування у відсотах
        public string PercentZoom => $"{Math.Round((Zoom - MinZoom) / (MaxZoom - MinZoom) * 100D)}%";

        // Файл ключа адміна
        public static string AdminKeyFile
        {
            get => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                   @"\VasyaV Programs\4 images\admin.key";
        }
        #endregion

        public Img(string path)
        {
            Source = path; // Шлях
            bool isRes = path[0] == '*'; // Чи ресурс
            try
            {
                BitmapImage bmp = new BitmapImage();
                string p = Source.ToLower();
                // Відкриваємо файл або ресурс(якщо на початку зірочка)
                stream = isRes && !p.Contains("icons") && !p.Contains("flags") ?
                  Application.GetResourceStream(new Uri(path.Substring(1), UriKind.Relative)).Stream :
                  new FileStream(Source, FileMode.Open, FileAccess.Read);
                // Відкриваємо зображення
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.StreamSource = stream;
                bmp.EndInit();

                bmp.Freeze(); // Замороження для економії пам'яті
                // Розміри
                Width = bmp.Width;
                Height = bmp.Height;
                Bmp = bmp; // Зберігаємо його

                // Інформація про файл
                FileInfo info = new FileInfo(isRes ? Environment.GetCommandLineArgs()[0] : Source);
                Name = info.Name;
                Extension = info.Extension;
                // Рахуємо розмір файлу
                string a = "b";
                double size = info.Length; // Зміна для перерахунку розміру
                if (size >= 1024) { size /= 1024; a = "Kb"; }
                if (size >= 1024) { size /= 1024; a = "Mb"; }
                if (size >= 1024) { size /= 1024; a = "Gb"; }
                // Пропуски між тисячами
                var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
                nfi.NumberGroupSeparator = " ";

                FileSize = $"{Math.Round(size, 2)} {a} ({info.Length.ToString("#,0", nfi)} b)";
            }
            catch (Exception ex)
            {
                Bmp = null;
                MessageBox.Show(Lang.T(1001, ex.Message), Lang.T(1000),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Освободити пам'ять
        public void Dispose()
        {
            stream.Close();
            stream.Dispose();
            Bmp = null;
            GC.Collect(3, GCCollectionMode.Forced); // Збір сміття
        }

        public override bool Equals(object obj)
        {
            var img = obj as Img;
            return img != null &&
                   Source == img.Source;
        }

        public override int GetHashCode()
        {
            return 924162744 + EqualityComparer<string>.Default.GetHashCode(Source);
        }

        public static bool operator ==(Img img1, Img img2)
        {
            return EqualityComparer<Img>.Default.Equals(img1, img2);
        }

        public static bool operator !=(Img img1, Img img2)
        {
            return !(img1 == img2);
        }
    }
}