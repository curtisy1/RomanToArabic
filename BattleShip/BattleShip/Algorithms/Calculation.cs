namespace BattleShip.Algorithms {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using Extensions;
    using Models;
    using Windows;
    using HorizontalAlignment = System.Windows.HorizontalAlignment;

    internal static class Calculation {
        private static ShipModel _comModel = new ShipModel();
        private static readonly ShipModel PlayerModel = new ShipModel();
        private static Window _window; // for use in algorithms
        private static dynamic _algorithm; // TODO: this is ugly, better make an Algorithm class or interface

        // check if the player or the computer hit a rectangle containing a ship and return true or false
        public static void CheckHit(Rectangle rect, out bool hasHit, bool fromAi = false) {

            // this is a check to make sure the player has placed all of his ships before starting to sink enemy ships
            if (SavedSettings.AgainstCom && PlayerModel.ShipList.Any(s => s.ShipRectangle.Count == 0)) {
                ChangeErrorText(rect);
                hasHit = false;
                return;
            }

            // we abuse the name property of rectangles as the shipname. NoShip means it's water, everything else is a hit
            if (rect.Name != "NoShip") {
                rect.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                rect.IsEnabled = false;
                hasHit = true;
            } else {
                rect.IsEnabled = false;
                rect.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 225));
                hasHit = false;
            }

            // if it was a hit, update the model to make sure the ship loses in length
            if (hasHit) {
                var model = fromAi ? PlayerModel : _comModel;
                GetAndUpdateHitShip(rect.Uid, model);
            }

            // if turn counting is enabled, count the turns and update them
            CountTurns();
            if (SavedSettings.ShowTurns) {
                UpdateCounter(DependencyObjectExtensions.GetParent<Window>(rect));
            }

            // if either the com or the player has no remaining ships, the game must be over
            if (_comModel.ShipList.Count == 0 || PlayerModel.ShipList.Count == 0) {
                var window = DependencyObjectExtensions.GetParent<Window>(rect);
                CallGameOverWindow(window);
                return;
            }

            // after the player clicked a field, let the algorithm try its best
            if (SavedSettings.AgainstCom && !fromAi) {
                CombinedAlgorithm();
            }
        }

        // change the error text to something more useful
        private static void ChangeErrorText(Rectangle rect) {
            var window = DependencyObjectExtensions.GetParent<Window>(rect);
            var errorBlock = window.FirstOrDefaultChild<TextBlock>(w => w.Name == "ErrorBlock");
            errorBlock.Text = "\n Please place all your ships first!";
            errorBlock.Visibility = Visibility.Visible;
        }

        // this is the hard part. We have our board now, so place the ships there recursively by brute forcing our way through
        public static void AiPlaceShips(List<Rectangle> rectangles) {
            var rand = new Random();
            _comModel = new ShipModel();

            // place one ship each, obviously
            foreach (var ship in _comModel.ShipList) {
                int y, x;
                var horizontal = Convert.ToBoolean(rand.Next(0, 2)); // randomize the orientation

                ship.UidAddition = GetGridName(rectangles); // this is always the same so set it outside of internal loop

                do {
                    if (horizontal) {
                        y = rand.Next(0, 10);
                        x = rand.Next(0, 10 - ship.Length);
                    } else {
                        x = rand.Next(0, 10);
                        y = rand.Next(0, 10 - ship.Length);
                    }
                } while (!RectangleSurroundingsFree(horizontal, ship, rectangles, x, y));

                // by now we found a place to reside our ship in, so update every rectangle that's part of it and add the coordinates to the ship
                foreach (var rect in ship.ShipRectangle) {
                    var coords = FormatCoords(rect.Uid);
                    ship.XCoord.Add(coords[0]);
                    ship.YCoord.Add(coords[1]);
                    rect.Name = ship.Name;
                }
            }
        }

        // pretty much the same logic as above, without the randomizing because it's the user's input
        public static bool UserPlaceShips(List<Rectangle> rectangles, List<int> xCoords, List<int> yCoords, string shipName) {
            var horizontal = xCoords.Count > 1;
            var shipsForPlayer = PlayerModel.ShipList;
            var ship = shipsForPlayer.FirstOrDefault(s => s.Name == shipName && !s.ShipRectangle.Any());

            // this should actually never happen.. but just in case
            if (ship == null) {
                return false;
            }

            // if neither of the coordinate count match the shiplength, that's an input error
            if (ship.Length != xCoords.Count && ship.Length != yCoords.Count) {
                return false;
            }

            ship.UidAddition = GetGridName(rectangles);

            // again, check if the placement is valid
            foreach (var x in xCoords) {
                foreach (var y in yCoords) {
                    if (!RectangleSurroundingsFree(horizontal, ship, rectangles, x, y, true)) {
                        return false;
                    }
                }
            }

            // and set the ship coordinates and the rectangle names correctly. Color the ship as well for better visibility
            foreach (var rect in ship.ShipRectangle) {
                var coords = FormatCoords(rect.Uid);
                ship.XCoord.Add(coords[0]);
                ship.YCoord.Add(coords[1]);
                rect.Name = ship.Name;
                rect.Fill = new SolidColorBrush(Colors.Wheat);
            }

            return true;
        }

        // this check every rectangle right, left, top, below  for a possible ship that's already there. 
        private static bool RectangleSurroundingsFree(bool horizontal, Ship ship, List<Rectangle> rectangles, int xCoord, int yCoord, bool isPlayer = false) {
            var rectUidAddition = GetGridName(rectangles); // can either be PlayerField or ComField
            var length = isPlayer ? 1 : ship.Length; // we need to check every rectangle, since the user enters ranges, is has to be different there

            if (yCoord >= 10 || xCoord >= 10) {
                return false;
            }

            if (horizontal) {
                for (var i = xCoord; i < xCoord + length; i++) {
                    var surroundingRectList = new List<Rectangle> {
                        rectangles.Find(r => r.Uid.Equals($"x: {i}, y: {yCoord}, {rectUidAddition}"))
                    };

                    if (i + 1 < 10) {
                        surroundingRectList.Add(rectangles.Find(r => r.Uid.Equals($"x: {i + 1}, y: {yCoord}, {rectUidAddition}")));
                    }

                    if (i - 1 > 0) {
                        surroundingRectList.Add(rectangles.Find(r => r.Uid.Equals($"x: {i - 1}, y: {yCoord}, {rectUidAddition}")));
                    }

                    if (yCoord + 1 < 10) {
                        surroundingRectList.Add(rectangles.Find(r => r.Uid.Equals($"x: {i}, y: {yCoord + 1}, {rectUidAddition}")));
                    }

                    if (yCoord - 1 > 0) {
                        surroundingRectList.Add(rectangles.Find(r => r.Uid.Equals($"x: {i}, y: {yCoord - 1}, {rectUidAddition}")));
                    }

                    // now we have some surrounding rectangles, if any one of them has a different name than NoShip, do the same again with the next possible position
                    foreach (var rect in surroundingRectList) {
                        if (!rect.Name.Equals("NoShip")) {
                            ship.ShipRectangle.Clear();
                            return !isPlayer && RectangleSurroundingsFree(true, ship, rectangles, xCoord, yCoord + 1);
                        }
                    }

                    // or else, add the rectangles to the ship
                    ship.ShipRectangle.Add(surroundingRectList[0]);
                }
            } else { // same for vertical
                for (var i = yCoord; i < yCoord + length; i++) {
                    var surroundingRectList = new List<Rectangle> {
                        rectangles.Find(r => r.Uid.Equals($"x: {xCoord}, y: {i}, {rectUidAddition}"))
                    };

                    if (xCoord + 1 < 10) {
                        surroundingRectList.Add(rectangles.Find(r => r.Uid.Equals($"x: {xCoord + 1}, y: {i}, {rectUidAddition}")));
                    }

                    if (xCoord - 1 > 0) {
                        surroundingRectList.Add(rectangles.Find(r => r.Uid.Equals($"x: {xCoord - 1}, y: {i}, {rectUidAddition}")));
                    }

                    if (i + 1 < 10) {
                        surroundingRectList.Add(rectangles.Find(r => r.Uid.Equals($"x: {xCoord}, y: {i + 1}, {rectUidAddition}")));
                    }

                    if (i - 1 > 0) {
                        surroundingRectList.Add(rectangles.Find(r => r.Uid.Equals($"x: {xCoord}, y: {i - 1}, {rectUidAddition}")));
                    }

                    foreach (var rect in surroundingRectList) {
                        if (!rect.Name.Equals("NoShip")) {
                            ship.ShipRectangle.Clear();
                            return !isPlayer && RectangleSurroundingsFree(false, ship, rectangles, xCoord + 1, yCoord);
                        }
                    }

                    ship.ShipRectangle.Add(surroundingRectList[0]);
                }
            }

            return true;
        }

        private static string GetGridName(IEnumerable<Rectangle> rectangles) {
            return rectangles.FirstOrDefault()?.Parent?.GetValue(FrameworkElement.NameProperty).ToString(); // this should always return something. Otherwise we seriously messed up somewhere..
        }

        // gets the ship belonging to the hit rectangle and updates the hitcount
        private static void GetAndUpdateHitShip(string coords, ShipModel model) {
            var affectedShip = GetHitShip(coords, model);
            affectedShip.HitCount++;

            // if the hitcount is the same as the shiplength, it's sunk. Then remove it from the list entirely
            if (ShipSunk(affectedShip)) {
                model.ShipList.Remove(affectedShip);
            }
        }

        // find the ship by Uid
        private static Ship GetHitShip(string coords, ShipModel model) {
            var ships = model.ShipList;
            var coordinates = FormatCoords(coords);
            var x = coordinates[0];
            var y = coordinates[1];
            var uidAddition = coords.Split(',').Last().TrimStart();

            return ships.FirstOrDefault(s => s.XCoord.Any(xc => xc.Equals(x)) && s.YCoord.Any(yc => yc.Equals(y)) && s.UidAddition.Equals(uidAddition));
        }

        private static bool ShipSunk(Ship ship) {
            return ship.HitCount == ship.Length;
        }

        // gets the coordinates and filters x and y numbers
        private static char[] FormatCoords(string coords) {
            var x = coords.First(char.IsDigit);
            var y = coords.Where(char.IsDigit).ElementAt(1);

            return new[] { x, y };
        }

        // formats the coordinats to integer values or returns negative if they are invalid
        public static int[] CharCoordsToInt(string uid) {
            var coords = FormatCoords(uid);
            var valid = int.TryParse(coords[0].ToString(), out var x);
            valid = int.TryParse(coords[1].ToString(), out var y) && valid;

            return valid ? new[] { x, y } : new[] { -1, -1 };
        }

        public static void GetScreenCenter(Window window) {
            //get the current monitor
            var currentMonitor = Screen.FromHandle(new WindowInteropHelper(window).Handle);

            //find out if our app is being scaled by the monitor
            var source = PresentationSource.FromVisual(window);
            var dpiScaling = source?.CompositionTarget?.TransformFromDevice.M11 ?? 1;

            //get the available area of the monitor
            var workArea = currentMonitor.WorkingArea;
            var workAreaWidth = (int)Math.Floor(workArea.Width * dpiScaling);
            var workAreaHeight = (int)Math.Floor(workArea.Height * dpiScaling);

            //move to the centre
            window.Left = (workAreaWidth - Constants.Window.Width * dpiScaling) / 2 + workArea.Left * dpiScaling;
            window.Top = (workAreaHeight - Constants.Window.Height * dpiScaling) / 2 + workArea.Top * dpiScaling;
            SetWindowProps(window);
        }

        // Does some necessary window settings, like setting height and width, the name and the content alignment
        private static void SetWindowProps(Window window) {
            window.Title = "BattleShips";
            window.ResizeMode = ResizeMode.NoResize;
            window.Width = Constants.Window.Width;
            window.Height = Constants.Window.Height;
            window.HorizontalContentAlignment = HorizontalAlignment.Center;
            window.VerticalContentAlignment = VerticalAlignment.Center;
            _window = window;
        }

        private static void CallGameOverWindow(Window mainWindow) {
            var gameOverWindow = new GameOver();
            mainWindow.Close();
            gameOverWindow.Show();
        }

        private static void CountTurns() {
            Turns.Count++;
        }

        private static void UpdateCounter(DependencyObject window) {
            var counter = window.FirstOrDefaultChild<TextBlock>(c => c.Name == "Counter");
            counter.Text = Turns.Count.ToString();
        }

        private static void CombinedAlgorithm() {
            var hitRect = _algorithm.GetShot();
            var hitShip = GetHitShip(hitRect.Uid, PlayerModel);
            var boardRect = _window.FirstOrDefaultChild<Rectangle>(r => r.Uid == hitRect.Uid);

            CheckHit(boardRect, out var hit, true);
            if (hit) {
                _algorithm.ShotHit(hitRect, ShipSunk(hitShip));
            } else {
                _algorithm.ShotMiss(hitRect);
            }
        }

        public static void SetupAlgorithm() {
            // default to easy mode if user doesn't choose anything
            if (SavedSettings.Difficulty == 1 || SavedSettings.Difficulty == 0) {
                _algorithm = new Bp7();
                _algorithm.SetState();
            } else if (SavedSettings.Difficulty == 2) {
                _algorithm = new Farnsworth();
                _algorithm.Init(PlayerModel.ShipList.Select(s => s.Length).ToList());
            }
        }
    }
}
