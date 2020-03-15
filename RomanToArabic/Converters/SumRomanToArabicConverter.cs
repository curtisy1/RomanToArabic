namespace RomanToArabic.Converters {
  using System.Collections.Generic;
  using System.Linq;
  using RomanToArabic.Dictionaries;

  public class SumRomanToArabicConverter : AbstractConverter {
    protected override List<int> GetArabicNumbersForRoman(string roman) => roman.Select(c => RomanToArabicDictionaries.SimpleRomanToArabicDictionary[c]).ToList();

    protected override int CalculateArabicNumber(IReadOnlyList<int> arabicNumbers) {
      var last = 0;
      return arabicNumbers.Reverse().Sum(v => {
        var r = v >= last ? v : -v;
        last = v;
        return r;
      });
    }
  }
}