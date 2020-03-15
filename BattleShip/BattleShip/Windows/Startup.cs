namespace BattleShip.Windows {
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using Algorithms;

    public class StartUp : Window {
        public StartUp() {
            Calculation.GetScreenCenter(this);

            var grid = new Grid();
            this.Content = grid;

            var introLabel = new Label {
                Content = "Welcome to battleships!\nTo start, please set your preferred settings first.",
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var aboutTag = new Button {
                Content = "?",
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
            };
            aboutTag.Click += this.CallAboutWindow;
            grid.Children.Add(aboutTag);
            grid.Children.Add(introLabel);

            var playButton = new Button {
                Padding = new Thickness(25, 12, 25, 12),
                Content = "Play",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(100, 0, 0, 0)
            };
            playButton.Click += this.CallMainWindow;
            grid.Children.Add(playButton);

            var settingsButton = new Button {
                Padding = new Thickness(25, 12, 25, 12),
                Content = "Settings",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 100, 0)
            };
            settingsButton.Click += this.CallSettingsWindow;
            grid.Children.Add(settingsButton);
        }

        private void CallMainWindow(object sender, RoutedEventArgs e) {
            var mainWindow = new MainWindow();
            this.Close();
            mainWindow.Show();
        }

        private void CallSettingsWindow(object sender, RoutedEventArgs e) {
            var settingsWindow = new Settings();
            this.Close();
            settingsWindow.Show();
        }

        private void CallAboutWindow(object sender, RoutedEventArgs e) {
            var aboutWindow = new About();
            this.Close();
            aboutWindow.Show();
        }

        [STAThread]
        public static void Main() {
            var app = new Application();
            app.Run(new StartUp());
        }
    }
}
