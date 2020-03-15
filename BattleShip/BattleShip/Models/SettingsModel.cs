namespace BattleShip.Models {
    using System.Windows;
    using System.Windows.Controls;

    internal class SettingsModel {
        internal CheckBox AgainstCom { get; } = new CheckBox {
            Content = "Play against COM?",
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(96, 50, 107, 0)
        };

        internal CheckBox ShowTime { get; } = new CheckBox {
            Content = "Show passed time?",
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(96, 50, 213, 0)
        };

        internal CheckBox ShowTurns { get; } = new CheckBox {
            Content = "Show number of turns?",
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(96, 50, 0, 0)
        };

        internal readonly Button BackButton = new Button {
            Content = "Back",
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Left,
            Padding = new Thickness(25, 12, 25, 12)
        };

        internal readonly Button ApplyButton = new Button {
            Content = "Apply",
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Right,
            Padding = new Thickness(25, 12, 25, 12),
            IsEnabled = false
        };

        internal ComboBox DifficultyDropDown { get; } = new ComboBox {
            Text = "Difficulty",
            Width = 150,
            IsReadOnly = true,
            IsEditable = true,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Left,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 50, 107, 0),
            Items = {
                new ComboBoxItem {
                    Content = "-- Choose Difficulty --",
                    IsEnabled = false,
                },
                new ComboBoxItem {
                    Content = "Easy",
                },
                new ComboBoxItem {
                    Content = "Hard",
                },
            },
        };
    }


    internal static class SavedSettings {
        public static bool AgainstCom { get; set; }

        public static bool ShowTime { get; set; }

        public static bool ShowTurns { get; set; }

        public static int Difficulty { get; set; }
    }
}
