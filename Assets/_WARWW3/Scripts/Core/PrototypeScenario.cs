namespace WarWW3.Core
{
    public static class PrototypeScenario
    {
        public static BattleGrid CreateGrid()
        {
            BattleGrid grid = new BattleGrid(64, 48, 2);
            grid.Block(27, 17, 6, 6); grid.Block(36, 28, 5, 7); grid.Block(18, 32, 4, 5);
            return grid;
        }
        public static BattleSimulation Create(UnitStats tank, UnitStats scout)
        {
            BattleSimulation sim = new BattleSimulation(CreateGrid());
            for (int i = 0; i < 8; i++) sim.Spawn(0, new Point2(15 + (i % 2) * 5, 29 + (i / 2) * 8), i % 2 == 0 ? tank : scout);
            for (int i = 0; i < 6; i++) sim.Spawn(1, new Point2(107 + (i % 2) * 5, 33 + (i / 2) * 10), i % 2 == 0 ? tank : scout);
            return sim;
        }
        public static UnitStats DefaultTank() { return new UnitStats("bulwark", 180, 3, 23, 5.5f, 20, 1.1f); }
        public static UnitStats DefaultScout() { return new UnitStats("kestrel", 105, 1, 12, 8.5f, 17, 0.65f); }
    }
}
