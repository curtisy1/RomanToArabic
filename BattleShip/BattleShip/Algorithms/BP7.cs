namespace BattleShip.Algorithms {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Shapes;

    internal class Bp7 {
        private readonly Random _rand = new Random();
        private const int GameSize = 10;
        private List<NextShot> _nextShots = new List<NextShot>();
        private const string Field = "PlayerField";

        // unknown applies after a ship has been sunk
        private enum Direction { Vertical = -1, Unknown = 0, Horizontal = 1 }

        private Direction _hitDirection, _lastShotDirection;

        // same as for direction
        private enum ShotResult { Unknown, Miss, Hit }

        // every rectangle gets a predefined shotresult (of unknown)
        private readonly ShotResult[,] _board = new ShotResult[GameSize, GameSize];

        // possible candidate for a next shot, i.e. if a rectangle has been hit before, try left, right, below, above
        private struct NextShot {
            public readonly Rectangle Rect;
            public readonly Direction Direction;

            public NextShot(Rectangle r, Direction d) {
                this.Rect = r;
                this.Direction = d;
            }
        }

        // shot next to the hit field, depending on the area being untouched
        private struct ScanShot {
            public readonly Rectangle Rect;
            public readonly int OpenSpaces;

            public ScanShot(Rectangle r, int o) {
                this.Rect = r;
                this.OpenSpaces = o;
            }
        }

        // inititalizes the board, fills every possible rectangle position with unknown
        public void SetState() {
            for (var i = 0; i < GameSize; i++) {
                for (var j = 0; j < GameSize; j++) {
                    this._board[i, j] = ShotResult.Unknown;
                }
            }
        }

        // main algorithm, finds a rectangle to shoot
        public Rectangle GetShot() {
            Rectangle rect;

            // if a hit was prior to this shot, get the first field next to the hit field
            if (this._nextShots.Count > 0) {

                // the direction can be known, when all surrounding fields are already shot and only one is left. If thats the case, only get the first horizontal or vertical rect aligned
                if (this._hitDirection != Direction.Unknown) {
                    this._nextShots = this._hitDirection == Direction.Horizontal ? this._nextShots.OrderByDescending(x => x.Direction).ToList() : this._nextShots.OrderBy(x => x.Direction).ToList();
                }

                // it can happen that a rectangle in this list is already shot, so do this until we find one with an Unknown result
                while (this._nextShots.Count > 0) {

                    // get the first rect in the list and convert it's coordinates to integers
                    rect = this._nextShots.First().Rect;
                    var coords = Calculation.CharCoordsToInt(rect.Uid);
                    int x = coords[0], y = coords[1];

                    // don't consider already hit rectangles
                    if (this._board[x, y] == ShotResult.Unknown) {
                        this._lastShotDirection = this._nextShots.First().Direction;
                        this._nextShots.RemoveAt(0);
                        return rect;
                    }

                    this._nextShots.RemoveAt(0);
                }
            }

            // if we have no shots left to consider, get one at random, where the state is still unknown
            var shots = new List<ScanShot>();
            for (var x = 0; x < GameSize; x++) {
                for (var y = 0; y < GameSize; y++) {
                    if (this._board[x, y] == ShotResult.Unknown) {
                        shots.Add(new ScanShot(new Rectangle { Uid = $"x: {x}, y: {y}, {Field}" }, this.OpenSpaces(x, y)));
                    }
                }
            }

            // get the rectangle with the most open spaces to cover, to maximaize hit probability
            shots = shots.OrderByDescending(x => x.OpenSpaces).ToList();
            var maxOpenSpaces = shots.FirstOrDefault().OpenSpaces;

            var scanShots2 = shots.Where(x => x.OpenSpaces == maxOpenSpaces).ToList();
            rect = scanShots2[this._rand.Next(scanShots2.Count)].Rect;

            return rect;
        }

        // calculates the open spaces relative to the current rectangle
        private int OpenSpaces(int x, int y) {
            var ctr = 0;

            // spaces to the left
            var pX = x - 1;
            var pY = y;
            while (pX >= 0 && this._board[pX, pY] == ShotResult.Unknown) {
                ctr++;
                pX--;
            }

            // spaces to the right
            pX = x + 1;
            pY = y;
            while (pX < GameSize && this._board[pX, pY] == ShotResult.Unknown) {
                ctr++;
                pX++;
            }

            // spaces to the top
            pX = x;
            pY = y - 1;
            while (pY >= 0 && this._board[pX, pY] == ShotResult.Unknown) {
                ctr++;
                pY--;
            }

            // spaces to the bottom
            pX = x;
            pY = y + 1;
            while (pY < GameSize && this._board[pX, pY] == ShotResult.Unknown) {
                ctr++;
                pY++;
            }

            return ctr;
        }

        // if a shot hit, do a few things, set the result of the rect to Hit and add all rectangles next to it to the next posible shots
        public void ShotHit(Rectangle rect, bool sunk) {
            var coords = Calculation.CharCoordsToInt(rect.Uid);
            var x = coords[0];
            var y = coords[1];

            if (x < 0 || y < 0) {
                this._board[x, y] = ShotResult.Unknown;
                this._hitDirection = Direction.Unknown;
                return;
            }

            // set the state of this rectangle on the board to Hit
            this._board[x, y] = ShotResult.Hit;

            // if the ship wasn't sunk, add all surrounding rects to a list of shots to try next, else try a random position again
            if (!sunk) {
                this._hitDirection = this._lastShotDirection;
                if (x != 0) {
                    this._nextShots.Add(new NextShot(new Rectangle { Uid = $"x: {x - 1}, y: {y}, {Field}" }, Direction.Horizontal));
                }

                if (y != 0) {
                    this._nextShots.Add(new NextShot(new Rectangle { Uid = $"x: {x}, y: {y - 1}, {Field}" }, Direction.Vertical));
                }

                if (x != GameSize - 1) {
                    this._nextShots.Add(new NextShot(new Rectangle { Uid = $"x: {x + 1}, y: {y}, {Field}" }, Direction.Horizontal));
                }

                if (y != GameSize - 1) {
                    this._nextShots.Add(new NextShot(new Rectangle { Uid = $"x: {x}, y: {y + 1}, {Field}" }, Direction.Vertical));
                }
            } else {
                this._hitDirection = Direction.Unknown;
                this._nextShots.Clear();
            }
        }

        // if a shot missed, set the state to Miss
        public void ShotMiss(Rectangle rect) {
            var coords = Calculation.CharCoordsToInt(rect.Uid);
            var x = coords[0];
            var y = coords[1];

            if (x < 0 || y < 0) {
                this._board[x, y] = ShotResult.Unknown;
                return;
            }

            this._board[x, y] = ShotResult.Miss;
        }
    }
}
