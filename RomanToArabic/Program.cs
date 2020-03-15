namespace RomanToArabic {
  using System;
  using RomanToArabic.Converters;

  public static class Program {
    public static void Main() {
      Console.WriteLine(new SumRomanToArabicConverter().ConvertFromRomanToArabic("MMXIII"));
      Console.ReadKey();
    }
  }
}