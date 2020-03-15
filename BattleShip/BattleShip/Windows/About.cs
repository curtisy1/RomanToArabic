namespace BattleShip.Windows {
    using System.Windows;
    using System.Windows.Controls;
    using Algorithms;

    internal class About : Window {
        public About() {
            Calculation.GetScreenCenter(this);

            var grid = new Grid();
            this.Content = grid;

            var rules = new TextBlock {
                Text = "To play this game, press the 'Play' button on the main screen." +
                       "\nIf you want to adjust some settings first, press the 'Settings' button." +
                       "\nThe game consists of an enemy screen (left or single field) and a player (right or none) field." +
                       "\nYou need to find ships by clicking the green squares on the enemy screen." +
                       "\nA ship can only be placed horizontal or vertical," +
                       "\nnever diagonally and has at least 2 but not more than 5 tiles." +
                       "\nYou win if you sink all ships before the enemy.",
            };

            var creator = new TextBlock {
                Text = "This game was made by Alexander Oberländer using WPF (without XAML) as a base." +
                       "\nThis product is published under MIT License",
                VerticalAlignment = VerticalAlignment.Center,
            };

            var backButton = new Button {
                Content = "Back",
                Width = 70,
                Height = 70,
                VerticalAlignment = VerticalAlignment.Bottom,
            };
            backButton.Click += this.BackToMainWindow;

            grid.Children.Add(rules);
            grid.Children.Add(creator);
            grid.Children.Add(backButton);
        }

        private void BackToMainWindow(object sender, RoutedEventArgs e) {
            var window = new StartUp();
            this.Close();
            window.Show();
        }
    }
}
