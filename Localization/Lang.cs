using ImgViewer.Properties;
using System;
using System.IO;
using System.Collections.Generic;

namespace ImgViewer.Localization
{
    // Мова
    public struct Lang
    {
        #region Properties
        const int PhrasesCount = 31; // Кількість фраз у мові

        // Фрази
        private List<Phrase> Phrases { get; set; }

        // Ключ мови
        public string Key { get; private set; }

        // Назва
        public string Name { get; set; }
        // Ікнонка прапору
        public string FlagIcon { get; set; }
        // Поточна мова
        public static Lang Current { get; private set; }
        // Папка з мовами
        public static string LangDir => Path.GetDirectoryName(
            Environment.GetCommandLineArgs()[0]) + @"\Languages";
        #endregion

        #region Methods
        // Отримати фразу по ключу
        public Phrase GetPhrase(int key)
        {
            if (Current.Phrases == null) return new Phrase();
            // Індекс фрази
            int index = Current.Phrases.IndexOf(new Phrase { Key = key });
            // Повертаємо результат, якщо знайдено фразу
            return index == -1 ? new Phrase { Value = "" } : Current.Phrases[index];
        }

        // Завантажити мову по імені
        public static void LoadLang(string name) => Current = LangFromFile($@"{LangDir}\{name}.lang");
        // Завантажити мову по файлу
        public static Lang LangFromFile(string file)
        {
            // Клас, у який ми завантажуємо дані
            Lang lang = new Lang { Phrases = new List<Phrase>() };
            // Зчитуємо файл
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            using (StreamReader sr = new StreamReader(fs))
            {
                // Поки не кінець файлу
                while (!sr.EndOfStream)
                {
                    // Отримуємо пару "ключ = значення"
                    string[] arr = sr.ReadLine().Split('=');
                    if (arr.Length != 2) continue; // Якщо не є цією парою
                    string key = arr[0].Trim().ToLower(); // Ключ
                    // Значення
                    string value = arr[1].Trim().Replace(@"\s", " ").Replace(@"\n", "\n");
                    // Обираємо потрібну дію
                    switch (key)
                    {
                        // Назва мови
                        case "name": lang.Name = value; break;
                        // Іконка прапору мови
                        case "icon": lang.FlagIcon = value; break;
                        // Інакше
                        default:
                            int a = 0; // Ключ у вигляді числа
                            // Якщо число -> додаємо фразу у мову 
                            if (int.TryParse(key, out a))
                            {
                                Phrase p = new Phrase { Key = a, Value = value };
                                lang.Phrases.Add(p);
                            }
                            break;
                    }
                }
            }

            #if !DEBUG
            // Якщо недостатньо фраз
            if (lang.Phrases.Count != PhrasesCount)
                throw new InvalidDataException("Language is invalid");
            #endif
            // Ключ мови
            lang.Key = Path.GetFileNameWithoutExtension(file);

            return lang;
        }

        // Перекласти фразу, отриману по ключу
        public static string T(int phraseKey, params object[] args) =>
            Current.GetPhrase(phraseKey).Format(args);
        #endregion
    }
}