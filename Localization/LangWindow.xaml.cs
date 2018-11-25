using ImgViewer.Properties;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace ImgViewer.Localization
{
    public partial class LangWindow : Window
    {
        // Мови
        private List<Lang> Langs = new List<Lang>();

        public LangWindow()
        {
            // Завантажуємо мови
            foreach (string file in Directory.GetFiles(Lang.LangDir, "*.lang"))
            {
                try
                {
                    Langs.Add(Lang.LangFromFile(file));
                }
                catch (Exception) { }
            }
            // Якщо жодної мови не вдалося завантажити:
            if (Langs.Count == 0)
            {
                MessageBox.Show("Language files was damaged.\n{...}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            InitializeComponent();
            LangBox.ItemsSource = Langs; // Прив'язка мов до списку
        }

        // При натиску на кнопку ОК
        private void OK_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Lang.LoadLang(Langs[LangBox.SelectedIndex].Key);
            Langs.Clear();
            // Зберігаємо мову в налаштуваннях
            Settings.Default.CurrentLang = Lang.Current.Key;
            Settings.Default.Save();
            // Результат діалогу
            DialogResult = true;
            this.Close();
        }

        // Чи можна натиснути ОК
        private void IsLangSelect(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = LangBox.SelectedIndex != -1;
        }
    }
}