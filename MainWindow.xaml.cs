using ImgViewer.Properties;
using ImgViewer.Localization;
using Microsoft.Win32;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using _Img = ImgViewer.Img;

namespace ImgViewer
{
    public sealed partial class MainWindow : Window
    {
        #region Змінні
        // Фільтри зображень
        const string ImageFilter = "*.jpg;*.png;*.gif;*.ico;*.jpeg;*.pcx;*.tiff;*.bmp;*.jp2;*.jxr";
        readonly string[] ImageExtensions = ImageFilter.Replace("*", "").Split(';');

        private bool isFullScreen = false; // Чи повний екран
        private bool isMaximize = false; // Чи було розгорнуте вікно
        private _Img current; // Поточне зображення
        #endregion

        #region Команди
        public static RoutedCommand NextImage = new RoutedCommand();
        public static RoutedCommand PreviousImage = new RoutedCommand();
        public static RoutedCommand Zoom_Plus = new RoutedCommand();
        public static RoutedCommand Zoom_Minus = new RoutedCommand();
        public static RoutedCommand Turn_Left = new RoutedCommand();
        public static RoutedCommand Turn_Right = new RoutedCommand();
        public static RoutedCommand Scroll = new RoutedCommand();
        #endregion

        public MainWindow()
        {
            // Завантажуємо мову
            try
            {
                // Вибір мови, якщо вона ще не вибрана
                if (Settings.Default.CurrentLang == "*" && !SelectLangDlg())
                {
                    this.Close();
                    return;
                }
                // Завантажуємо вибрану мову
                Lang.LoadLang(Settings.Default.CurrentLang);
            }
            catch (Exception ex)
            {
                // Повідомлення про помилку
                MessageBoxResult result =
                MessageBox.Show($"Cannot load language, error message:\n{ex.Message}\nDo you want to change the language?",
                    "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                // Якщо користувач хоче відновити мову за замовчуванням
                if (result == MessageBoxResult.Yes)
                {
                    // Відновлюємо мову
                    Settings.Default.CurrentLang = "*";
                    Settings.Default.Save();
                    // Відкриваємо цю програму заново
                    Process.Start(Environment.GetCommandLineArgs()[0]);
                }

                this.Close();
                return;
            }

            InitializeComponent(); // Ініціалізація компнентів
            this.DataContext = current; // Прив'язка зображення

            // Відновлємо параметри вікна
            this.Width = Settings.Default.Width;
            this.Height = Settings.Default.Height;
            this.Top = Settings.Default.Top;
            this.Left = Settings.Default.Left;
            this.WindowState = Settings.Default.IsMaximized ?
                WindowState.Maximized : WindowState.Normal;
            if (Settings.Default.IsFullScreen) ToFullScreen();

            string[] args = Environment.GetCommandLineArgs(); // Аргументи
            // Якщо програму відкрито через файл -> відкриваємо файл
            if (args.Length > 1)
            {
                string file = args[1];
                // Перевіряємо на пасхалку
                try
                {
                    if (file == _Img.AdminKeyFile)
                    {
                        switch (File.ReadAllText(file))
                        {
                            case "begin write('Hello world!'); end.": file = "*EG/pascal.jpg"; break;
                            case "dima = 7x+3x": file = "*EG/beaver.jpg"; break;
                            case "Two brothers I_I": file = "*EG/havryk.jpg"; break;
                            // ♥
                            case "144429144328144625144645144625142744201447472014464314474614462614464314474620144727144630144626144630232323":
                                file = "*EG/DILY.jpg"; break;
                            default: file = "*icon.ico"; break;
                        }
                    }
                }
                catch (Exception) { }
                Open(file); // Відкриваємо файл
            }
            // Інакше -> відкриваємо останнє зображення
            else Open(Settings.Default.LastImage);
        }

        // Відкрити зображення
        private bool Open(string image)
        {
            current?.Dispose();
            current = new Img(image); // Відкриття зображення
            this.DataContext = current;

            // Якщо помилка
            if (current.Bmp == null)
            {
                current = null;
                return false;
            }
            return true;
        }

        // Наступне/попереднє
        private void Jump(bool isNext)
        {
            // Усі файли у папці із зображенням
            string[] files = Directory.GetFiles(Path.GetDirectoryName(current.Source));
            // Індекс поточного зображення
            int index = Array.IndexOf(files, current.Source);
            // Копія індексу
            int temp_index = index;
            // Напрямок руху
            int a = isNext ? 1 : -1;

            // Пробуємо перейти
            do
            {
                index += a;
                // Якщо індекс виходить за межі масиву
                if (index >= files.Length) index = 0;
                else if (index < 0) index = files.Length - 1;
            }
            while (Array.IndexOf(ImageExtensions, Path.GetExtension(files[index])) == -1);
            if (index == temp_index) return; // Якщо індекси збігаються - вихід

            string temp = current.Source; // Резервна копія шляху до зображення
            // Відкриваємо зображення, при помилці - відкриваємо минуле
            if (!Open(files[index])) Open(temp);
        }

        // Зробити повноекранним або навпаки
        private void ToFullScreen()
        {
            // Якщо вікно вже повноекранне
            if (isFullScreen)
            {
                WindowStyle = WindowStyle.SingleBorderWindow; // Рамка
                ResizeMode = ResizeMode.CanResize; // Дозвіл зміни розміру
                // Повертаємо минулий стан вікна
                WindowState = isMaximize ? WindowState.Maximized : WindowState.Normal;
                // Зображення
                Grid.SetRow(Brd, 1);
                Grid.SetRowSpan(Brd, 1);
                // Панель інструментів
                Grid.SetRowSpan(ToolBar, 1);
                // Ставимо прокрутку
                Brd.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                Brd.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                ToolBarAnim(false);
            }
            // Якщо вікно не повноекранне
            else
            {
                isMaximize = WindowState == WindowState.Maximized;
                WindowState = WindowState.Normal; // Стан вікна
                // Стан вікна
                WindowStyle = WindowStyle.None; // Рамка
                WindowState = WindowState.Maximized; // Стан вікна

                // Зображення
                Grid.SetRow(Brd, 0);
                Grid.SetRowSpan(Brd, 4);
                // Панель інструментів
                Grid.SetRowSpan(ToolBar, 2);
                // Прибираємо прокрутку
                Brd.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                Brd.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                ToolBarAnim(true);
            }
            isFullScreen = !isFullScreen;
        }

        // Сховати/показати панель інстурментів
        private void ToolBarAnim(bool isHide)
        {
            DoubleAnimation anim = new DoubleAnimation(isHide ? 0 : 1,
                new Duration(TimeSpan.FromMilliseconds(250)));
            ToolBar.BeginAnimation(OpacityProperty, anim);
        }

        // Діалог з вибором мови
        private bool SelectLangDlg()
        {
            LangWindow dlg = new LangWindow();
            return dlg.ShowDialog() == true;
        }

        #region Commands
        // Відкрити зображення
        private void OpenImg(object sender, ExecutedRoutedEventArgs e)
        {
            // Діалог
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Відкрити зображення";
            ofd.Filter = "Зображення|" + ImageFilter;
            // Відкриваємо діалог
            if (ofd.ShowDialog() == true) Open(ofd.FileName);
        }
        // Зберегти зображення в інший файл
        private void SaveImg(object sender, ExecutedRoutedEventArgs e)
        {
            // Діалог
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Копіювати зображення";
            sfd.Filter = $"Зображення {current.Extension}|*{current.Extension}|Усі файли|*.*";
            // Відкриваємо діалог
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    File.Copy(current.Source, sfd.FileName, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Lang.T(1002, ex.Message), Lang.T(1000),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        // Копіювати зображення в буфер
        private void CopyImg(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Clipboard.SetImage(current.Bmp);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Lang.T(1003, ex.Message), Lang.T(1000),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Властивості
        private void ShowProperties(object sender, ExecutedRoutedEventArgs e)
        {
            PropertiesWindow window = new PropertiesWindow(current);
            window.ShowDialog();
            window.Close();
        }
        // Закрити
        private void Close(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        // Наступне зображення
        private void NextI(object sender, ExecutedRoutedEventArgs e)
        {
            Jump(true);
        }
        // Попереднє зображення
        private void PrevI(object sender, ExecutedRoutedEventArgs e)
        {
            Jump(false);
        }

        // Зменшити масштаб
        private void ZoomM(object sender, ExecutedRoutedEventArgs e)
        {
            current.Zoom -= ZoomSld.LargeChange;
        }
        // Збільшити масштаб
        private void ZoomP(object sender, ExecutedRoutedEventArgs e)
        {
            current.Zoom += ZoomSld.LargeChange;
        }

        // Повернути вліво
        private void TurnL(object sender, ExecutedRoutedEventArgs e)
        {
            current.Angle -= 90;
        }
        // Повернути вправо
        private void TurnR(object sender, ExecutedRoutedEventArgs e)
        {
            current.Angle += 90;
        }

        // В повний екран
        private void FullScreen(object sender, ExecutedRoutedEventArgs e)
        {
            ToFullScreen();
        }

        // Чи завантажене зображення
        private void IsImgLoaded(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = current != null;
        }
        // Чи можна "перестрибнути"
        private void CanJump(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = current != null && current.Source[0] != '*';
        }

        // Скролл
        private void Scrolling(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                // Створюємо папки
                Directory.CreateDirectory(Path.GetDirectoryName(_Img.AdminKeyFile));
                // Створюємо ключ адміна
                FileStream fs = File.Create(_Img.AdminKeyFile);
                fs.Close();
                // Інфо про успішну операцію
                MessageBox.Show($"Admin key created!\nFile name: {_Img.AdminKeyFile}", "Easter egg",
                   MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot create admin key!\n{ex.Message}", "Error",
                   MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Events
        // Змінити мову
        private void Select_Language(object sender, RoutedEventArgs e)
        {
            // Діалог вибору мови
            if (SelectLangDlg() == true)
            {
                // Перезапуск програми
                Process.Start(Environment.GetCommandLineArgs()[0]);
                Application.Current.Shutdown();
            }
        }

        // При пересувані мишки
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (current == null) return; // Якщо не завантажено зображення
            Point point = e.GetPosition(Img);
            current.MouseX = (int)point.X;
            current.MouseY = (int)point.Y;
        }

        // Коли мишка покинула зображення
        private void Img_MouseLeave(object sender, MouseEventArgs e)
        {
            if (current != null)
            {
                current.MouseX = 0;
                current.MouseY = 0;
            }
        }

        // При затримці мишки на зображенні
        private void Img_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && current != null && current.Source[0] != '*')
            {
                // Дані про файл
                DataObject data = new DataObject(DataFormats.FileDrop);
                data.SetFileDropList(new StringCollection { current.Source });
                // Перетаскування
                DragDrop.DoDragDrop(Img, data, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }

        // При киданні чогось на вікно
        private void Img_Drop(object sender, DragEventArgs e)
        {
            // Якщо це файл
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Кинуті файли
                string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
                // Перший кинутий файл
                string file = droppedFiles[0];

                // Відкриваємо його, якщо він не такий самий
                if (file != current.Source) Open(file);
            }
        }

        // Події мишки на панелі інструментів
        private void ToolBar_MouseEnter(object sender, MouseEventArgs e)
        {
            if (isFullScreen) ToolBarAnim(false);
        }
        private void ToolBar_MouseLeave(object sender, MouseEventArgs e)
        {
            if (isFullScreen) ToolBarAnim(true);
        }

        // Масштабування прокруткою
        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Якщо не натиснуто CTRL
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                current.Zoom += e.Delta > 0 ? ZoomSld.SmallChange : -ZoomSld.SmallChange;
                e.Handled = true;
            }
        }

        // Рухати мишкою зображення
        private void Brd_MouseMove(object sender, MouseEventArgs e)
        {
            // Якщо зажата лкм
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Cursor = Cursors.Hand;
            }
            else Cursor = Cursors.Arrow;
        }

        // При закритті вікна
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Зберігаємо параметри вікна
            Settings.Default.Width = this.Width;
            Settings.Default.Height = this.Height;
            Settings.Default.Top = this.Top;
            Settings.Default.Left = this.Left;
            Settings.Default.IsMaximized = this.WindowState == WindowState.Maximized;
            Settings.Default.IsFullScreen = isFullScreen;
            // Зберігаємо останнє зображення
            Settings.Default.LastImage = current.Source ?? "*icon.ico";

            Settings.Default.Save();
        }
        #endregion
    }
}