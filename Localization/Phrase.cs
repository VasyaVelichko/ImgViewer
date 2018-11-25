using System;

namespace ImgViewer.Localization
{
    // Фраза
    public struct Phrase
    {
        #region Properties
        // Ключ
        public int Key { get; set; }
        // Значення
        public string Value { get; set; }
        #endregion

        #region Methods
        public string Format(params object[] args) => string.Format(Value, args);
        public override int GetHashCode() => 990326508 + Key.GetHashCode();

        public override bool Equals(object obj)
        {
            if (!(obj is Phrase)) return false;
            return Equals((Phrase)obj);
        }

        public bool Equals(Phrase p) => Key == p.Key;

        public override string ToString()
        {
            return Value;
        }
        #endregion

        #region Operators
        public static bool operator ==(Phrase p1, Phrase p2) => p1.Equals(p2);
        public static bool operator !=(Phrase p1, Phrase p2) => !p1.Equals(p2);
        #endregion
    }
}