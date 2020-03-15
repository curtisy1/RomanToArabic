namespace BattleShip.Models {
    using System.Collections.Generic;

    internal class ShipModel {
        public List<Ship> ShipList { get; } = new List<Ship> {
            new Battleship(),
            new Cruiser(),
            new Cruiser(),
            new Destroyer(),
            new Destroyer(),
            new Destroyer(),
            new Submarine(),
            new Submarine(),
            new Submarine(),
            new Submarine()
        };
    }
}
