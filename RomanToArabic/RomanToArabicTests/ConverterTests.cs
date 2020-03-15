namespace RomanToArabicTests {
  using NUnit.Framework;
  using RomanToArabic.Converters;

  [TestFixture]
  public class ConverterTests {
    [TestCase("I", 1)]
    [TestCase("II", 2)]
    [TestCase("IV", 4)]
    [TestCase("V", 5)]
    [TestCase("IX", 9)]
    [TestCase("XLII", 42)]
    [TestCase("XCIX", 99)]
    [TestCase("MMXIII", 2013)]
    [TestCase("XIV", 14)]
    public void ConvertFromRomanToArabic_Should_ConvertCorrectly(string roman, int expectedArabic) {
      Assert.That(new SimpleRomanToArabicConverter().ConvertFromRomanToArabic(roman), Is.EqualTo(expectedArabic));
    }
  }
}