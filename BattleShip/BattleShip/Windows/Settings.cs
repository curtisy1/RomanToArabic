namespace BattleShip.Windows {
    using System.Windows;
    using System.Windows.Controls;
    using Algorithms;
    using Extensions;
    using Models;

    internal class Settings : Window {
        private readonly SettingsModel _settings = new SettingsModel();

        public Settings() {
            Calculation.GetScreenCenter(this);

            var grid = new Grid();
            this.Content = grid;

            var difficultyDropDown = this._settings.DifficultyDropDown;
            difficultyDropDown.SelectedIndex = SavedSettings.Difficulty;
            difficultyDropDown.SelectionChanged += ValidateDropdownApply;

            var againstCom = this._settings.AgainstCom;
            againstCom.IsChecked = SavedSettings.AgainstCom;
            againstCom.Click += ValidateCheckBoxApply;

            var showTimePlayed = this._settings.ShowTime;
            showTimePlayed.IsChecked = SavedSettings.ShowTime;
            showTimePlayed.Click += ValidateCheckBoxApply;

            var showNumberOfTurns = this._settings.ShowTurns;
            showNumberOfTurns.IsChecked = SavedSettings.ShowTurns;
            showNumberOfTurns.Click += ValidateCheckBoxApply;

            var backButton = this._settings.BackButton;
            var applyButton = this._settings.ApplyButton;

            backButton.Click += this.BackToMainWindow;
            applyButton.Click += this.ApplySettings;

            grid.Children.Add(againstCom);
            grid.Children.Add(showNumberOfTurns);
            grid.Children.Add(showTimePlayed);
            grid.Children.Add(backButton);
            grid.Children.Add(applyButton);
            grid.Children.Add(difficultyDropDown);
        }

        private void BackToMainWindow(object sender, RoutedEventArgs e) {
            var window = new StartUp();
            this.Close();
            window.Show();
        }

        private void ApplySettings(object sender, RoutedEventArgs e) {
            SavedSettings.AgainstCom = this._settings.AgainstCom.IsChecked == true;
            SavedSettings.ShowTime = this._settings.ShowTime.IsChecked == true;
            SavedSettings.ShowTurns = this._settings.ShowTurns.IsChecked == true;
            SavedSettings.Difficulty = this._settings.DifficultyDropDown.SelectedIndex;
            this.BackToMainWindow(sender, e);
        }

        private static void ValidateCheckBoxApply(object sender, RoutedEventArgs e) {
            var grid = DependencyObjectExtensions.GetParent<Grid>((CheckBox)sender);
            var applyButton = grid.FirstOrDefaultChild<Button>(c => c.Content.ToString() == "Apply");
            applyButton.IsEnabled = true;
        }

        private static void ValidateDropdownApply(object sender, RoutedEventArgs e) {
            var grid = DependencyObjectExtensions.GetParent<Grid>((ComboBox)sender);
            var applyButton = grid.FirstOrDefaultChild<Button>(c => c.Content.ToString() == "Apply");
            applyButton.IsEnabled = true;
        }
    }
}
