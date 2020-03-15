namespace BattleShip.Windows {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using System.Windows.Threading;
    using Algorithms;
    using Extensions;
    using Models;

    internal class MainWindow : Window {
        private readonly DispatcherTimer _dispatcherTimer = new DispatcherTimer();
        private readonly Stopwatch _stopWatch = new Stopwatch();
        private static readonly ShipModel Model = new ShipModel();

        public MainWindow() {
            // initialize a stopwatch
            this._dispatcherTimer.Tick += this.UpdateTime;
            this._dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1);

            var window = this;
            var grid = new Grid {
                Background = new SolidColorBrush(Colors.LightGoldenrodYellow)
            };

            Calculation.GetScreenCenter(window);

            this.InitBoard(grid, true);
            this.Content = grid;
            this.SetWindowForSettings(window, grid);
        }

        // fills the grid with rectangles
        private void InitBoard(Panel grid, bool isCom) {
            var panel = new WrapPanel {
                MaxHeight = Constants.Window.Height,
                MaxWidth = Constants.Window.Width,
                HorizontalAlignment = isCom ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Name = isCom ? "ComField" : "PlayerField"
            };
            var dimension = Math.Sqrt(Constants.Window.Width * 4) + 1;
            var rectangles = new List<Rectangle>(); // put all of our rectangles into one object to randomly place ships

            for (var i = 0; i < 10; i++) {
                for (var j = 0; j < 10; j++) {
                    var rect = new Rectangle {
                        Width = dimension,
                        Height = dimension,
                        Fill = new SolidColorBrush(Color.FromRgb(0, 127, 0)),
                        Stroke = new SolidColorBrush(Colors.White),
                        Uid = $"x: {j}, y: {i}, {panel.Name}",
                        Name = "NoShip",
                        IsEnabled = isCom
                    };

                    rect.MouseLeftButtonUp += Calculate;
                    rectangles.Add(rect);
                    panel.Children.Add(rect);
                }
            }

            if (isCom) {
                Calculation.AiPlaceShips(rectangles);
            } else {
                this.InitShipSelectionForPlayer(grid);
                Calculation.SetupAlgorithm();
            }

            grid.Children.Add(panel);
        }

        // place all the required input fields for our ships to place in the middle of the game screen
        private void InitShipSelectionForPlayer(Panel grid) {
            // this aligns every underlying item in a single row
            var stackPanel = new StackPanel {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            var alreadyUsedShips = new List<string>();
            var usageNotice = new TextBlock {
                TextAlignment = TextAlignment.Center,
                Text = "\n Enter either x or y coordinates in ranges(0-4) \n or comma seperated(0,1,2,3,4)"
            };
            var errorNotice = new TextBlock {
                Text = "\n Input doesn't match required format \n or is a combination of both. \n Please change it to one of the above!",
                Visibility = Visibility.Hidden,
                TextAlignment = TextAlignment.Center,
                Foreground = new SolidColorBrush(Colors.Red),
                Name = "ErrorBlock"
            };

            foreach (var ship in Model.ShipList) {
                if (alreadyUsedShips.Contains(ship.Name)) {
                    continue;
                }

                alreadyUsedShips.Add(ship.Name);
                var typeCount = Model.ShipList.Count(s => s.Name == ship.Name);
                var shipType = new TextBlock {
                    Height = 20,
                    Width = 130,
                    Text = $"{ship.Name} ({ship.Length} Tiles)",
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                stackPanel.Children.Add(shipType);

                for (var i = 0; i < typeCount; i++) {
                    // holds items supposed to be in a single row
                    var wrapPanel = new WrapPanel {
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    var textBlockX = new TextBlock {
                        Text = "x: ",
                        Uid = $"txtX: {ship.Name} {i}"
                    };

                    var textBlockY = new TextBlock {
                        Text = "y: ",
                        Uid = $"txtY: {ship.Name} {i}"
                    };

                    var xCoordInput = new TextBox {
                        Height = 20,
                        Width = 20,
                        AcceptsReturn = false,
                        Margin = new Thickness(0, 0, 5, 0),
                        Uid = $"x: {ship.Name} {i}"
                    };

                    var yCoordInput = new TextBox {
                        Height = 20,
                        Width = 20,
                        AcceptsReturn = false,
                        Margin = new Thickness(0, 0, 5, 0),
                        Uid = $"y: {ship.Name} {i}"
                    };

                    var submitButton = new Button {
                        Content = "Place ship",
                        Uid = $"btn: {ship.Name} {i}"
                    };
                    submitButton.Click += this.ValidateCoordsInput;

                    wrapPanel.Children.Add(textBlockX);
                    wrapPanel.Children.Add(xCoordInput);
                    wrapPanel.Children.Add(textBlockY);
                    wrapPanel.Children.Add(yCoordInput);
                    wrapPanel.Children.Add(submitButton);
                    stackPanel.Children.Add(wrapPanel);
                }
            }

            stackPanel.Children.Add(usageNotice);
            stackPanel.Children.Add(errorNotice);
            grid.Children.Add(stackPanel);
        }

        // check if the entered coordinates are in a valid format. Since WPF doesn't really support Drag and Drop and I could've made this a standalone project probably..
        private void ValidateCoordsInput(object sender, RoutedEventArgs e) {
            var xCoords = new List<int>();
            var yCoords = new List<int>();
            var sequenceValid = true;

            var buttonId = ((Button)sender).Uid;
            var requiredUidField = buttonId.Substring(buttonId.IndexOf(":", StringComparison.CurrentCulture) + 1).TrimStart();
            var shipType = requiredUidField.Remove(requiredUidField.Length - 2);

            // represents the range of inputs
            var xCoordString = this.FirstOrDefaultChild<TextBox>(tb => tb.Uid == $"x: {requiredUidField}")?.Text;
            var yCoordString = this.FirstOrDefaultChild<TextBox>(tb => tb.Uid == $"y: {requiredUidField}")?.Text;

            if (xCoordString != null) {
                if (xCoordString == string.Empty) {
                    sequenceValid = false;
                }
                if (xCoordString.Contains('-') && xCoordString.Contains(',')) {
                    sequenceValid = false;
                } else if (xCoordString.Contains(',')) {
                    sequenceValid = this.SequenceInputToInt(xCoordString, xCoords);
                } else if (xCoordString.Contains('-')) {
                    sequenceValid = this.RangeInputToInt(xCoordString, xCoords);
                } else if (xCoordString.Length == 1) {
                    sequenceValid = this.SingleInputToInt(xCoordString, xCoords);
                }
            }

            if (yCoordString != null && sequenceValid) {
                if (yCoordString == string.Empty) {
                    sequenceValid = false;
                }
                if (yCoordString.Contains('-') && yCoordString.Contains(',')) {
                    sequenceValid = false;
                } else if (yCoordString.Contains(',')) {
                    sequenceValid = this.SequenceInputToInt(yCoordString, yCoords);
                } else if (yCoordString.Contains('-')) {
                    sequenceValid = this.RangeInputToInt(yCoordString, yCoords);
                } else if (yCoordString.Length == 1) {
                    sequenceValid = this.SingleInputToInt(yCoordString, yCoords);
                }
            }

            if (xCoords.Count <= 1 && yCoords.Count <= 1 || xCoords.Count == yCoords.Count) {
                sequenceValid = false;
            }

            if (!sequenceValid) {
                this.ToggleErrorBlock(true);
            } else {
                this.ToggleErrorBlock(false);

                var rectangles = this.VisualChildrenOrDefault(new List<Rectangle>(), r => r.Uid.Contains("PlayerField"));
                if (!Calculation.UserPlaceShips(rectangles, xCoords, yCoords, shipType)) {
                    this.ToggleErrorBlock(true);
                } else {
                    this.RemoveAssociatedInputs(sender);
                }
            }
        }

        // don't cause confusion or let the user enter another thing after a valid input, rather remove the fields
        private void RemoveAssociatedInputs(object sender) {
            var button = (Button)sender;
            var buttonId = button.Uid;
            var requiredUidField = buttonId.Substring(buttonId.IndexOf(":", StringComparison.CurrentCulture) + 1).TrimStart();

            // represents the range of inputs
            var xInput = this.FirstOrDefaultChild<TextBox>(tb => tb.Uid == $"x: {requiredUidField}");
            var yInput = this.FirstOrDefaultChild<TextBox>(tb => tb.Uid == $"y: {requiredUidField}");
            var xText = this.FirstOrDefaultChild<TextBlock>(tb => tb.Uid == $"txtX: {requiredUidField}");
            var yText = this.FirstOrDefaultChild<TextBlock>(tb => tb.Uid == $"txtY: {requiredUidField}");
            var parent = (WrapPanel)button.Parent;

            parent.Children.Remove(button);
            parent.Children.Remove(xInput);
            parent.Children.Remove(yInput);
            parent.Children.Remove(xText);
            parent.Children.Remove(yText);
        }

        private void ToggleErrorBlock(bool show, string text = null) {
            var errorBlock = this.FirstOrDefaultChild<TextBlock>(tb => tb.Name == "ErrorBlock");
            errorBlock.Visibility = show ? Visibility.Visible : Visibility.Hidden;

            if (text != null) {
                errorBlock.Text = text;
            }
        }

        private bool SequenceInputToInt(string coordString, List<int> coords) {
            var sequence = coordString.Split(',');
            foreach (var s in sequence) {
                if (int.TryParse(s, out var result)) {
                    coords.Add(result);
                }
            }

            return this.CheckSequence(coords);
        }

        private bool SingleInputToInt(string coordString, List<int> coords) {
            if (int.TryParse(coordString, out var result)) {
                if (result < 0 || result > 9) {
                    return false;
                }

                coords.Add(result);
                return true;
            }

            return false;
        }

        private bool RangeInputToInt(string coordString, List<int> coords) {
            var range = coordString.Split('-');
            var firstValid = int.TryParse(range.FirstOrDefault(), out var from);
            var lastValid = int.TryParse(range.LastOrDefault(), out var to);
            if (!firstValid || !lastValid) {
                return false;
            }

            for (var i = from; i <= to; i++) {
                coords.Add(i);
            }

            return this.CheckSequence(coords);
        }

        private bool CheckSequence(List<int> coords) {
            var isValid = true;
            for (var i = 0; i < coords.Count; i++) {
                if (i + 1 >= coords.Count) {
                    break;
                }

                if (coords[i] + 1 != coords[i + 1]) {
                    isValid = false;
                    break;
                }
            }

            return isValid;
        }

        private static void Calculate(object sender, MouseButtonEventArgs e) {
            Calculation.CheckHit((Rectangle)sender, out var notNeeded);
        }

        private void SetWindowForSettings(Window window, Grid grid) {
            var additionalHeight = Constants.Window.AdditionalHeight;
            var additionalWidth = Constants.Window.AdditionalWidth;

            // if we play against an AI, we need double the field
            if (SavedSettings.AgainstCom) {
                window.Width = window.Width * 2 + window.Width / 2;
                this.InitBoard((Grid)this.Content, false);
            }

            if (SavedSettings.ShowTime || SavedSettings.ShowTurns) {
                window.Height += 20;
            }

            if (SavedSettings.ShowTime) {
                window.Height += additionalHeight;
                var timeBlock = new TextBlock {
                    Width = additionalWidth,
                    Height = additionalHeight,
                    Text = "00:00",
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Name = "Clock"
                };
                grid.Children.Add(timeBlock);

                this._dispatcherTimer.Start();
                this._stopWatch.Start();
            }

            if (SavedSettings.ShowTurns) {
                var counterBlock = new TextBlock {
                    Width = additionalWidth,
                    Height = additionalHeight,
                    Text = "0",
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Name = "Counter"
                };
                grid.Children.Add(counterBlock);
            }
        }

        private void UpdateTime(object sender, EventArgs e) {
            if (this._stopWatch.IsRunning) {
                var timeBlock = this.FirstOrDefaultChild<TextBlock>(c => c.Name == "Clock");
                var ts = this._stopWatch.Elapsed;
                var currentTime = $"{ts.Minutes:00}:{ts.Seconds:00}";
                timeBlock.Text = currentTime;
            }
        }
    }
}
