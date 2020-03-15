namespace BattleShip.Algorithms {
    using System;
    using System.Collections.Generic;
    using System.Windows.Shapes;

    internal class Farnsworth {
        // UNKNOWN - we know nothing about this location
        // MISS    - an empty space
        // HIT     - a ship is here
        // SUNK    - a ship is here, and we've sunk it, but all we know is this location is SUNK
        // SUNKSHIP - we've sunk the ship here and we know where the rest of the ship is
        private enum State { Unknown = 0, Miss, Hit, Sunk, Sunkship }

        // what we know about the world:
        private const int GameSize = 10;
        private const string Field = "PlayerField";
        private State[,] _state;

        // the size of the remaining ships that we haven't sunk
        private List<int> _remainingShips;

        // an optional list of points that we consider first when deciding where to explore
        private List<Rectangle> _mustExplore;

        // the amount of open space around each (x,y)
        private int[,] _left, _right, _below, _above;

        // metric describing how much space/possible hits each (x,y) has
        private int[,] _space, _hits;

        private readonly Random _rand = new Random();

        public void Init(List<int> shipLengths) {
            this._left = new int[GameSize, GameSize];
            this._right = new int[GameSize, GameSize];
            this._above = new int[GameSize, GameSize];
            this._below = new int[GameSize, GameSize];
            this._space = new int[GameSize, GameSize];
            this._hits = new int[GameSize, GameSize];
            this._remainingShips = shipLengths;
            this._mustExplore = new List<Rectangle>();
            this._state = new State[GameSize, GameSize];
            this.SetState();
        }

        // first set up our state to all ships unknown
        private void SetState() {
            for (var i = 0; i < GameSize; i++) {
                for (var j = 0; j < GameSize; j++) {
                    this._state[i, j] = State.Unknown;
                }
            }
        }

        // where should we shoot?
        public Rectangle GetShot() {
            // update some state:
            // convert HITs that are surrounded into SUNKs
            this.SunkFromSurroundedHits();

            // look for any HIT/SUNKs that could only belong to one ship
            this.SunkFromShipConstraints();

            // if we're certain that a ship must be at some point, choose that
            if (this._mustExplore.Count > 0) {
                var r = this._mustExplore[0];
                this._mustExplore.RemoveAt(0);
                return r;
            }

            // check for any outstanding HITs that need to be explored
            var hitPoint = this.GetHitPoint();
            if (hitPoint.Uid != string.Empty) {
                return hitPoint;
            }

            // if there are no outstanding hits, explore empty space
            return this.GetSpacePoint();
        }

        // when we hit something, update state
        public void ShotHit(Rectangle rect, bool sunk) {
            var coords = Calculation.CharCoordsToInt(rect.Uid);
            int x = coords[0], y = coords[1];
            this._state[x, y] = sunk ? State.Sunk : State.Hit;

            // if we know that we just sunk a ship at (x,y), look at adjacent locations to see if we
            // had to have sunk a given ship (i.e. if there's only one remaining ship that has a size
            // less than the sunk length, that ship must have sunk.)
            if (sunk) {
                var localLeft = x;
                while (localLeft > 0 && (this._state[localLeft - 1, y] == State.Hit || this._state[localLeft - 1, y] == State.Sunk)) {
                    --localLeft;
                }

                var localRight = x;
                while (localRight < GameSize - 1 && (this._state[localRight + 1, y] == State.Hit || this._state[localRight + 1, y] == State.Sunk)) {
                    ++localRight;
                }

                var localAbove = y;
                while (localAbove > 0 && (this._state[x, localAbove - 1] == State.Hit || this._state[x, localAbove - 1] == State.Sunk)) {
                    --localAbove;
                }

                var localBelow = y;
                while (localBelow < GameSize - 1 && (this._state[x, localBelow + 1] == State.Hit || this._state[x, localBelow + 1] == State.Sunk)) {
                    ++localBelow;
                }

                var sunkWidth = localRight - localLeft + 1;
                var sunkHeight = localBelow - localAbove + 1;
                if (sunkWidth > 1) {
                    var numShips = 0;
                    foreach (var w in this._remainingShips) {
                        if (w <= sunkWidth) {
                            ++numShips;
                        }
                    }

                    if (numShips == 1) {
                        this._remainingShips.Remove(sunkWidth);
                    }
                } else if (sunkHeight > 1) {
                    var numShips = 0;
                    foreach (var w in this._remainingShips) {
                        if (w <= sunkHeight) {
                            ++numShips;
                        }
                    }

                    if (numShips == 1) {
                        this._remainingShips.Remove(sunkHeight);
                    }
                }
            }
        }

        // when we miss something, update state
        public void ShotMiss(Rectangle rect) {
            var coords = Calculation.CharCoordsToInt(rect.Uid);
            int x = coords[0], y = coords[1];

            if (x < 0 || y < 0) {
                this._state[x, y] = State.Unknown;
                return;
            }

            this._state[x, y] = State.Miss;
        }

        // helper functions

        // examine each HIT/SUNK, checking how the remaining ships could fit on it.  If there is only
        // one ship that could fit on this space in one way, then remove that ship from consideration
        // and mark the associated locations as SUNKSHIP.
        private void SunkFromShipConstraints() {
            for (var y = 0; y < GameSize; ++y) {
                for (var x = 0; x < GameSize; ++x) {
                    if (this._state[x, y] == State.Hit || this._state[x, y] == State.Sunk) {
                        var localLeft = x;
                        while (localLeft > 0 && (this._state[localLeft - 1, y] == State.Unknown || this._state[localLeft - 1, y] == State.Hit || this._state[localLeft - 1, y] == State.Sunk)) {
                            --localLeft;
                        }

                        var localRight = x;
                        while (localRight < GameSize - 1 && (this._state[localRight + 1, y] == State.Unknown || this._state[localRight + 1, y] == State.Hit || this._state[localRight + 1, y] == State.Sunk)) {
                            ++localRight;
                        }

                        var locaAbove = y;
                        while (locaAbove > 0 && (this._state[x, locaAbove - 1] == State.Unknown || this._state[x, locaAbove - 1] == State.Hit || this._state[x, locaAbove - 1] == State.Sunk)) {
                            --locaAbove;
                        }

                        var localBelow = y;
                        while (localBelow < GameSize - 1 && (this._state[x, localBelow + 1] == State.Unknown || this._state[x, localBelow + 1] == State.Hit || this._state[x, localBelow + 1] == State.Sunk)) {
                            ++localBelow;
                        }

                        var sunkWidth = localRight - localLeft + 1;
                        var sunkHeight = localBelow - locaAbove + 1;

                        if (sunkWidth > 1) {
                            int numShips = 0, lastW = 0;
                            foreach (var w in this._remainingShips) {
                                if (w <= sunkWidth) {
                                    lastW = w;
                                    ++numShips;
                                }
                            }

                            if (numShips == 1 && lastW == sunkWidth) {
                                for (var i = localLeft; i <= localRight; ++i) {
                                    if (this._state[i, y] == State.Unknown) {
                                        this._mustExplore.Add(new Rectangle { Uid = $"x: {i}, y: {y}, {Field}" });
                                    }

                                    this._state[i, y] = State.Sunkship;
                                }

                                this._remainingShips.Remove(sunkWidth);
                            }
                        } else if (sunkHeight > 1) {
                            int numShips = 0, lastH = 0;
                            foreach (var w in this._remainingShips) {
                                if (w <= sunkHeight) {
                                    lastH = w;
                                    ++numShips;
                                }
                            }

                            if (numShips == 1 && lastH == sunkHeight) {
                                for (var i = locaAbove; i <= localBelow; ++i) {
                                    if (this._state[x, i] == State.Unknown) {
                                        this._mustExplore.Add(new Rectangle { Uid = $"x: {x}, y: {i}, {Field}" });
                                    }

                                    this._state[x, i] = State.Sunkship;
                                }

                                this._remainingShips.Remove(sunkHeight);
                            }
                        }
                    }
                }
            }
        }

        // check each HIT to see if it is completely surrounded by zero or more HITs followed by
        // an edge or other termination piece.  If so, mark it as SUNK.
        private void SunkFromSurroundedHits() {
            for (var y = 0; y < GameSize; ++y) {
                for (var x = 0; x < GameSize; ++x) {
                    if (this._state[x, y] == State.Hit) {
                        var localLeft = x;
                        while (localLeft > 0 && this._state[localLeft - 1, y] == State.Hit) {
                            --localLeft;
                        }
                        if (localLeft > 0 && this._state[localLeft - 1, y] == State.Unknown) {
                            continue; // not bound
                        }

                        var localRight = x;
                        while (localRight < GameSize - 1 && this._state[localRight + 1, y] == State.Hit) {
                            ++localRight;
                        }
                        if (localRight < GameSize - 1 && this._state[localRight + 1, y] == State.Unknown) {
                            continue;
                        }

                        var localAbove = y;
                        while (localAbove > 0 && this._state[x, localAbove - 1] == State.Hit) {
                            --localAbove;
                        }
                        if (localAbove > 0 && this._state[x, localAbove - 1] == State.Unknown) {
                            continue;
                        }

                        var localBelow = y;
                        while (localBelow < GameSize - 1 && this._state[x, localBelow + 1] == State.Hit) {
                            ++localBelow;
                        }
                        if (localBelow < GameSize - 1 && this._state[x, localBelow + 1] == State.Unknown) {
                            continue;
                        }

                        // if we get here, we know that this State.HIT square is bounded on all sides by zero
                        // or more State.HITs and then a non-State.UNKNOWN piece or edge.  Since it's surrounded, it's sunk.
                        this._state[x, y] = State.Sunk;
                    }
                }
            }
        }

        // for each HIT, examine all the configurations of remaining ships that could fit on that location
        private Rectangle GetHitPoint() {
            var hitSum = 0;
            for (var y = 0; y < GameSize; ++y) {
                for (var x = 0; x < GameSize; ++x) {
                    this._hits[x, y] = 0;
                }
            }

            for (var y = 0; y < GameSize; ++y) {
                for (var x = 0; x < GameSize; ++x) {
                    if (this._state[x, y] == State.Hit) {
                        foreach (var w in this._remainingShips) {
                            // horizontal
                            int localLeft = x, endl = Math.Max(x - (w - 1), 0);
                            while (localLeft > endl && (this._state[localLeft - 1, y] == State.Unknown || this._state[localLeft - 1, y] == State.Hit || this._state[localLeft - 1, y] == State.Sunk)) {
                                --localLeft;
                            }

                            int localRight = x, endr = Math.Min(x + (w - 1), GameSize - 1);
                            while (localRight < endr && (this._state[localRight + 1, y] == State.Unknown || this._state[localRight + 1, y] == State.Hit || this._state[localRight + 1, y] == State.Sunk)) {
                                ++localRight;
                            }
                            if (localRight - localLeft + 1 >= w) {
                                for (var i = localLeft; i <= localRight; ++i) {
                                    if (this._state[i, y] == State.Unknown) {
                                        ++this._hits[i, y];
                                        ++hitSum;
                                    }
                                }
                            }

                            // vertical
                            int localAbove = y, enda = Math.Max(y - (w - 1), 0);
                            while (localAbove > enda && (this._state[x, localAbove - 1] == State.Unknown || this._state[x, localAbove - 1] == State.Hit || this._state[x, localAbove - 1] == State.Sunk)) {
                                --localAbove;
                            }

                            int localBelow = y, endb = Math.Min(y + (w - 1), GameSize - 1);
                            while (localBelow < endb && (this._state[x, localBelow + 1] == State.Unknown || this._state[x, localBelow + 1] == State.Hit || this._state[x, localBelow + 1] == State.Sunk)) {
                                ++localBelow;
                            }
                            if (localBelow - localAbove + 1 >= w) {
                                for (var i = localAbove; i <= localBelow; ++i) {
                                    if (this._state[x, i] == State.Unknown) {
                                        ++this._hits[x, i];
                                        ++hitSum;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // if we haven't marked any location as possible matches for existing HITs, do something else
            if (hitSum == 0) {
                return new Rectangle();
            }

            // choose randomly among the best hit locations
            return this.GetLargest(this._hits);
        }

        // examine how many different ways the remaining ships could fit in the open space surrounding each position
        private Rectangle GetSpacePoint() {
            for (var y = 0; y < GameSize; ++y) {
                for (var x = 0; x < GameSize; ++x) {
                    this.ComputeAdjacentSpace(x, y);

                    var sumPoss = 0;
                    foreach (var len in this._remainingShips) {
                        // left/right
                        var leftPos = this._left[x, y] < len - 1 ? this._left[x, y] : len - 1;
                        var rightPos = this._right[x, y] < len - 1 ? this._right[x, y] : len - 1;

                        var possLr = leftPos + rightPos + 1 - len + 1;
                        if (possLr > 0) {
                            sumPoss += possLr;
                        }

                        // above/below
                        var abovePos = this._above[x, y] < len - 1 ? this._above[x, y] : len - 1;
                        var belowPos = this._below[x, y] < len - 1 ? this._below[x, y] : len - 1;

                        var possAb = abovePos + belowPos + 1 - len + 1;
                        if (possAb > 0) {
                            sumPoss += possAb;
                        }
                    }

                    this._space[x, y] = sumPoss;
                }
            }

            // choose randomly among the best space locations
            return this.GetLargest(this._space);
        }

        // compute the amount of open space in each direction from (x,y)
        private void ComputeAdjacentSpace(int x, int y) {
            if (this._state[x, y] != State.Unknown) {
                this._left[x, y] = this._right[x, y] = this._below[x, y] = this._above[x, y] = 0;
            } else {
                // left
                if (x == 0 || this._state[x - 1, y] != State.Unknown) {
                    this._left[x, y] = 0;
                } else {
                    this._left[x, y] = this._left[x - 1, y] + 1;
                }

                // right
                if (x == 0 || this._right[x - 1, y] == 0) {
                    var ctr = 0;
                    for (var xp = x + 1; xp < GameSize; ++xp) {
                        if (this._state[xp, y] != State.Unknown) {
                            break;
                        }

                        ++ctr;
                    }

                    this._right[x, y] = ctr;
                } else {
                    this._right[x, y] = this._right[x - 1, y] - 1;
                }

                // above
                if (y == 0 || this._state[x, y - 1] != State.Unknown) {
                    this._above[x, y] = 0;
                } else {
                    this._above[x, y] = this._above[x, y - 1] + 1;
                }

                // below
                if (y == 0 || this._below[x, y - 1] == 0) {
                    var ctr = 0;
                    for (var yp = y + 1; yp < GameSize; ++yp) {
                        if (this._state[x, yp] != State.Unknown) {
                            break;
                        }

                        ++ctr;
                    }

                    this._below[x, y] = ctr;
                } else {
                    this._below[x, y] = this._below[x, y - 1] - 1;
                }
            }
        }

        // find the largest element(s) in arr and choose one at random
        private Rectangle GetLargest(int[,] arr) {
            // find largest element in arr
            int largest = 0, count = 0;
            for (var y = 0; y < GameSize; ++y) {
                for (var x = 0; x < GameSize; ++x) {
                    if (arr[x, y] > largest) {
                        largest = arr[x, y];
                        count = 1;
                    } else if (this._hits[x, y] == largest) {
                        ++count;
                    }
                }
            }

            // choose one at random
            for (var y = 0; y < GameSize; ++y) {
                for (var x = 0; x < GameSize; ++x) {
                    if (arr[x, y] == largest) {
                        if (this._rand.NextDouble() < 1.0 / count) {
                            return new Rectangle { Uid = $"x: {x}, y: {y}, {Field}" };
                        }

                        --count;
                    }
                }
            }

            // shouldn't happen
            return new Rectangle();
        }
    }
}
