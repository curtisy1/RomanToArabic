namespace BattleShip.Constants {
	using System.Windows.Controls;

	internal static class Difficulty {
		internal static dynamic Easy { get; } = new ComboBoxItem {
			Content = "Easy",
		};

		internal static dynamic Medium { get; } = new ComboBoxItem {
			Content = "Medium",
		};

		internal static dynamic Hard { get; } = new ComboBoxItem {
			Content = "Difficult",
		};
	}
}
