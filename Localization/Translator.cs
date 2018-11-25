using System;
using System.Windows.Markup;

namespace ImgViewer.Localization
{
    // Перекладач тексту
    [ContentProperty("Key")]
    [MarkupExtensionReturnType(typeof(string))]
    public sealed class Translator : MarkupExtension
    {
        #region Properties
        [ConstructorArgument("Key")] // Ключ фрази
        public int Key { get; set; }
        #endregion

        #region Constructors
        public Translator() {}
        public Translator(int key)
        {
            Key = key;
        }
        #endregion

        // Повернути значення
        public override object ProvideValue(IServiceProvider serviceProvider) => 
            Lang.Current.GetPhrase(Key).Value;
    }
}