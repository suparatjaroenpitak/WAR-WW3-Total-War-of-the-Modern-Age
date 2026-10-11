using System;
using System.Collections.Generic;
using WarWW3.Core;

namespace WarWW3.Tests
{
    public static class CoreChecks
    {
        public static IDictionary<string, Action> Cases()
        {
            return new Dictionary<string, Action> {
                {"movement_reaches_destination", Movement}, {"path_avoids_buildings", PathAvoidsBuildings},
                {"unreachable_path_rejected", Unreachable}, {"invalid_commands_do_not_mutate", InvalidCommands},
                {"friendly_fire_rejected", FriendlyFire}, {"armor_damage_floor", Damage},
                {"line_of_sight_blocks_damage", LineOfSight}, {"simultaneous_fire_draw", Simultaneous},
                {"victory_latches_and_freezes", Victory}, {"defeat_detected", Defeat},
                {"ai_completes_skirmish", Ai}, {"replay_hash_is_repeatable", Determinism},
                {"invalid_unit_stats_rejected", InvalidStats}, {"hold_cancels_movement", Hold},
                {"grid_rejects_nonfinite_positions", InvalidGrid}, {"attack_tracks_moving_target", Chase}
            };
        }
        static void Assert(bool value, string message) { if (!value) throw new Exception(message); }
        static UnitStats Stats(int health = 100, int damage = 20, float range = 8, float speed = 5) { return new UnitStats("test", health, 0, damage, speed, range, 0.2f); }
        static BattleSimulation Pair(BattleGrid grid, UnitStats a, UnitStats b, Point2 p, Point2 q)
        { var sim = new BattleSimulation(grid); sim.Spawn(0, p, a); sim.Spawn(1, q, b); return sim; }
        static void Run(BattleSimulation sim, int ticks, bool ai = false) { for (int i = 0; i < ticks; i++) sim.Step(ai); }
        static void Movement()
        {
            var sim = Pair(new BattleGrid(32, 32, 2), Stats(range: 1), Stats(range: 1), new Point2(3, 3), new Point2(61, 61));
            Assert(sim.Command(0, 0, OrderKind.Move, new Point2(21, 17)), "Move rejected"); Run(sim, 100);
            Assert(Point2.DistanceSquared(sim.Find(0).Position, new Point2(21, 17)) < 0.001f, "Destination not reached");
        }
        static void PathAvoidsBuildings()
        {
            var grid = new BattleGrid(20, 20, 2); grid.Block(5, 0, 3, 12);
            var sim = Pair(grid, Stats(range: 1), Stats(range: 1), new Point2(3, 3), new Point2(39, 39));
            Assert(sim.Command(0, 0, OrderKind.Move, new Point2(25, 3)), "Detour rejected");
            for (int i = 0; i < 200; i++) { sim.Step(false); Assert(grid.IsWalkable(sim.Find(0).Position), "Crossed blocked building"); }
            Assert(Point2.DistanceSquared(sim.Find(0).Position, new Point2(25, 3)) < 0.001f, "Detour failed");
        }
        static void Unreachable()
        {
            var grid = new BattleGrid(10, 10, 1); grid.Block(5, 0, 1, 10);
            Assert(grid.FindPath(new Point2(1, 1), new Point2(8, 8)) == null, "Accepted disconnected destination");
        }
        static void InvalidCommands()
        {
            var sim = Pair(new BattleGrid(20, 20, 2), Stats(), Stats(), new Point2(3, 3), new Point2(35, 35)); ulong before = sim.StateHash();
            Assert(!sim.Command(1, 0, OrderKind.Move, new Point2(5, 5)), "Enemy ownership accepted");
            Assert(!sim.Command(0, 999, OrderKind.Move, new Point2(5, 5)), "Unknown unit accepted");
            Assert(!sim.Command(0, 0, OrderKind.Move, new Point2(float.NaN, 5)), "NaN accepted");
            Assert(!sim.Command(0, 0, OrderKind.Move, new Point2(-1, 5)), "Out of map accepted");
            Assert(!sim.Command(0, 0, (OrderKind)99, new Point2(5, 5)), "Unknown order accepted");
            Assert(sim.StateHash() == before, "Invalid command mutated simulation");
        }
        static void FriendlyFire()
        {
            var sim = Pair(new BattleGrid(10, 10, 2), Stats(), Stats(), new Point2(3, 3), new Point2(17, 17));
            Assert(!sim.Command(0, 0, OrderKind.Attack, new Point2(3, 3), 0), "Friendly attack accepted");
        }
        static void Damage() { Assert(UnitStats.CalculateDamage(23, 3) == 20 && UnitStats.CalculateDamage(2, 100) == 1, "Armor rule incorrect"); }
        static void LineOfSight()
        {
            var grid = new BattleGrid(10, 10, 2); grid.Block(4, 0, 1, 10);
            var sim = Pair(grid, Stats(range: 20), Stats(range: 20), new Point2(5, 5), new Point2(13, 5)); Run(sim, 20);
            Assert(sim.Find(0).Health == 100 && sim.Find(1).Health == 100, "Shot through obstacle");
        }
        static void Simultaneous()
        {
            var sim = Pair(new BattleGrid(10, 10, 2), Stats(20, 20), Stats(20, 20), new Point2(3, 3), new Point2(5, 3)); sim.Step(false);
            Assert(sim.Outcome == MatchOutcome.Draw, "Simultaneous destruction did not draw");
        }
        static void Victory()
        {
            var sim = Pair(new BattleGrid(10, 10, 2), Stats(100, 100), Stats(10, 1), new Point2(3, 3), new Point2(5, 3)); sim.Step(false);
            Assert(sim.Outcome == MatchOutcome.PlayerVictory, "Victory missing"); ulong hash = sim.StateHash(); Run(sim, 20);
            Assert(hash == sim.StateHash() && !sim.Command(0, 0, OrderKind.Move, new Point2(7, 7)), "Ended match changed");
        }
        static void Defeat()
        {
            var sim = Pair(new BattleGrid(10, 10, 2), Stats(10, 1), Stats(100, 100), new Point2(3, 3), new Point2(5, 3)); sim.Step(false);
            Assert(sim.Outcome == MatchOutcome.PlayerDefeat, "Defeat missing");
        }
        static void Ai()
        {
            var sim = PrototypeScenario.Create(PrototypeScenario.DefaultTank(), PrototypeScenario.DefaultScout()); Run(sim, 6000, true);
            Assert(sim.Outcome != MatchOutcome.Running, "AI never completed match");
        }
        static void Determinism()
        {
            var a = PrototypeScenario.Create(PrototypeScenario.DefaultTank(), PrototypeScenario.DefaultScout());
            var b = PrototypeScenario.Create(PrototypeScenario.DefaultTank(), PrototypeScenario.DefaultScout());
            for (int i = 0; i < 800; i++)
            {
                if (i % 100 == 0) foreach (var u in a.Units) if (u.Team == 0 && u.Alive) { a.Command(0, u.Id, OrderKind.AttackMove, new Point2(75, 45)); b.Command(0, u.Id, OrderKind.AttackMove, new Point2(75, 45)); }
                a.Step(true); b.Step(true); Assert(a.StateHash() == b.StateHash(), "Replay diverged at tick " + i);
            }
        }
        static void InvalidStats()
        {
            bool rejected = false; try { Stats(speed: float.PositiveInfinity); } catch (ArgumentException) { rejected = true; } Assert(rejected, "Infinite speed accepted");
        }
        static void Hold()
        {
            var sim = Pair(new BattleGrid(20, 20, 2), Stats(range: 1), Stats(range: 1), new Point2(3, 3), new Point2(35, 35));
            sim.Command(0, 0, OrderKind.Move, new Point2(21, 21)); sim.Step(false); var p = sim.Find(0).Position; sim.Command(0, 0, OrderKind.Hold, p); Run(sim, 20);
            Assert(Point2.DistanceSquared(p, sim.Find(0).Position) == 0, "Hold did not halt");
        }
        static void InvalidGrid()
        {
            var grid = new BattleGrid(10, 10, 2); Assert(!grid.IsWalkable(new Point2(float.PositiveInfinity, 0)) && !grid.IsWalkable(new Point2(-0.1f, 1)), "Invalid coordinate accepted");
        }
        static void Chase()
        {
            var sim = Pair(new BattleGrid(50, 50, 2), Stats(range: 3, speed: 9), Stats(range: 1, speed: 3), new Point2(3, 3), new Point2(40, 3));
            sim.Command(0, 0, OrderKind.Attack, sim.Find(1).Position, 1); sim.Command(1, 1, OrderKind.Move, new Point2(80, 3)); Run(sim, 200);
            Assert(sim.Find(1).Health < 100, "Attack failed to track moving target");
        }
    }
}
