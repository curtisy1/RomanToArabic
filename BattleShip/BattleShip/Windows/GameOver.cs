namespace BattleShip.Windows {
    using System.Windows;
    using System.Windows.Controls;
    using Algorithms;

    internal class GameOver : Window {
        private bool _checked;

        public GameOver() {
            Calculation.GetScreenCenter(this);

            var grid = new Grid();

            this.Content = grid;

            var replayButton = new Button {
                Padding = new Thickness(25, 12, 25, 12),
                Content = "Play again",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(100, 0, 0, 0)
            };
            replayButton.Click += this.CallMainWindow;
            grid.Children.Add(replayButton);

            var changeSettingsCheckbox = new CheckBox {
                Content = "Change settings before replay?",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 50, 0, 0)
            };
            changeSettingsCheckbox.Click += this.ToggleSettingsBox;
            grid.Children.Add(changeSettingsCheckbox);

            var finishButton = new Button {
                Padding = new Thickness(25, 12, 25, 12),
                Content = "Exit",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 100, 0)
            };
            finishButton.Click += this.ExitGame;
            grid.Children.Add(finishButton);
        }

        private void CallMainWindow(object sender, RoutedEventArgs e) {
            Window window;
            if (this._checked) {
                var settingsWindow = new Settings();
                window = settingsWindow;
            } else {
                var mainWindow = new MainWindow();
                window = mainWindow;
            }

            this.Close();
            window.Show();
        }

        private void ExitGame(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        private void ToggleSettingsBox(object sender, RoutedEventArgs e) {
            this._checked = ((CheckBox)sender).IsChecked ?? false;
        }
    }
}
