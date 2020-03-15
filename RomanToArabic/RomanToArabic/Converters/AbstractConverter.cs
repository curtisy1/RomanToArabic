namespace RomanToArabic.Converters {
  using System.Collections.Generic;

  public abstract class AbstractConverter {
    public int ConvertFromRomanToArabic(string roman) => this.CalculateArabicNumber(this.GetArabicNumbersForRoman(roman));
    
    protected abstract List<int> GetArabicNumbersForRoman(string roman);

    protected abstract int CalculateArabicNumber(IReadOnlyList<int> arabicNumbers);
  }
}