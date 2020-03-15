namespace BattleShip.Models {
    using System.Collections.Generic;
    using System.Windows.Shapes;

    internal class Ship {
        public int Length { get; protected set; }

        public List<char> XCoord { get; } = new List<char>();

        public List<char> YCoord { get; } = new List<char>();

        public string UidAddition { get; set; }

        public int HitCount { get; set; }

        public string Name { get; protected set; }

        public List<Rectangle> ShipRectangle { get; } = new List<Rectangle>();
    }

    internal class Battleship : Ship {
        public Battleship() {
            this.Length = 5;
            this.Name = "BattleShip";
        }
    }

    internal class Cruiser : Ship {
        public Cruiser() {
            this.Length = 4;
            this.Name = "Cruiser";
        }
    }

    internal class Destroyer : Ship {
        public Destroyer() {
            this.Length = 3;
            this.Name = "Destroyer";
        }
    }

    internal class Submarine : Ship {
        public Submarine() {
            this.Length = 2;
            this.Name = "Submarine";
        }
    }
}
