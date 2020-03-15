namespace RomanToArabic.Converters {
  using System.Collections.Generic;
  using System.Linq;
  using RomanToArabic.Dictionaries;

  public class SimpleRomanToArabicConverter : AbstractConverter {
    protected override List<int> GetArabicNumbersForRoman(string roman) => roman.Select(c => RomanToArabicDictionaries.SimpleRomanToArabicDictionary[c]).ToList();

    protected override int CalculateArabicNumber(IReadOnlyList<int> arabicNumbers) {
      var arabicNumber = arabicNumbers.First();

      for (var i = 1; i < arabicNumbers.Count; i++) {
        arabicNumber += arabicNumbers[i - 1] < arabicNumbers[i] ? arabicNumbers[i] - 2 * arabicNumbers[i - 1] : arabicNumbers[i];
      }

      return arabicNumber;
    }
  }
}