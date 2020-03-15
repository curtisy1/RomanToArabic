namespace RomanToArabic.Dictionaries {
  using System.Collections.Generic;

  public static class RomanToArabicDictionaries {
    public static readonly Dictionary<char, int> SimpleRomanToArabicDictionary = new Dictionary<char, int> {
      { 'I', 1 },
      { 'V', 5 },
      { 'X', 10 },
      { 'L', 50 },
      { 'C', 100 },
      { 'D', 500 },
      { 'M', 1000 }
    };
  }
}