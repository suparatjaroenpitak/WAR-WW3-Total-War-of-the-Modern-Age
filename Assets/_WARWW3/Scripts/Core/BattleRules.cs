using System;
using System.Collections.Generic;

namespace WarWW3.Core
{
    public struct Point2
    {
        public float X, Y;
        public Point2(float x, float y) { X = x; Y = y; }
        public bool IsFinite { get { return !float.IsNaN(X) && !float.IsNaN(Y) && !float.IsInfinity(X) && !float.IsInfinity(Y); } }
        public static float DistanceSquared(Point2 a, Point2 b) { float x = a.X - b.X, y = a.Y - b.Y; return x * x + y * y; }
        public static Point2 MoveTowards(Point2 a, Point2 b, float distance)
        {
            float d = (float)Math.Sqrt(DistanceSquared(a, b));
            return d <= distance || d < 0.0001f ? b : new Point2(a.X + (b.X - a.X) * distance / d, a.Y + (b.Y - a.Y) * distance / d);
        }
    }

    public sealed class UnitStats
    {
        public readonly string Id;
        public readonly int Health, Armor, Damage;
        public readonly float Speed, Range, FireInterval;
        public UnitStats(string id, int health, int armor, int damage, float speed, float range, float fireInterval)
        {
            if (string.IsNullOrEmpty(id) || health <= 0 || armor < 0 || damage <= 0 ||
                !FinitePositive(speed) || !FinitePositive(range) || !FinitePositive(fireInterval)) throw new ArgumentException("Invalid unit definition.");
            Id = id; Health = health; Armor = armor; Damage = damage; Speed = speed; Range = range; FireInterval = fireInterval;
        }
        static bool FinitePositive(float value) { return value > 0 && !float.IsInfinity(value) && !float.IsNaN(value); }
        public static int CalculateDamage(int damage, int armor) { return Math.Max(1, damage - armor); }
    }

    public enum OrderKind { Hold, Move, AttackMove, Attack }
    public enum MatchOutcome { Running, PlayerVictory, PlayerDefeat, Draw }

    public sealed class UnitState
    {
        public readonly int Id, Team;
        public readonly UnitStats Stats;
        public Point2 Position { get; internal set; }
        public Point2 Destination { get; internal set; }
        public int Health { get; internal set; }
        public int TargetId { get; internal set; }
        public int LastShotTick { get; internal set; }
        public OrderKind Order { get; internal set; }
        public bool Alive { get { return Health > 0; } }
        internal float Cooldown;
        internal Queue<Point2> Path = new Queue<Point2>();
        internal UnitState(int id, int team, Point2 position, UnitStats stats)
        { Id = id; Team = team; Stats = stats; Position = Destination = position; Health = stats.Health; TargetId = -1; LastShotTick = -100; }
    }

    public sealed class BattleSimulation
    {
        public const float StepSeconds = 0.1f;
        readonly List<UnitState> units = new List<UnitState>();
        readonly List<int> pendingDamage = new List<int>();
        readonly System.Collections.ObjectModel.ReadOnlyCollection<UnitState> readOnlyUnits;
        public IList<UnitState> Units { get { return readOnlyUnits; } }
        public readonly BattleGrid Grid;
        public int Tick { get; private set; }
        public MatchOutcome Outcome { get; private set; }
        public event Action<int, int> Shot;
        public BattleSimulation(BattleGrid grid) { if (grid == null) throw new ArgumentNullException("grid"); Grid = grid; readOnlyUnits = units.AsReadOnly(); }
        public UnitState Spawn(int team, Point2 position, UnitStats stats)
        {
            if (Tick != 0 || team < 0 || team > 1 || stats == null || !Grid.IsWalkable(position)) throw new ArgumentException("Invalid spawn.");
            UnitState unit = new UnitState(units.Count, team, position, stats); units.Add(unit); pendingDamage.Add(0); return unit;
        }
        public UnitState Find(int id) { return id >= 0 && id < units.Count ? units[id] : null; }
        public bool Command(int team, int id, OrderKind kind, Point2 destination, int targetId = -1)
        {
            UnitState unit = Find(id);
            if (Outcome != MatchOutcome.Running || unit == null || !unit.Alive || unit.Team != team || !destination.IsFinite || !Enum.IsDefined(typeof(OrderKind), kind)) return false;
            if (kind == OrderKind.Hold) { unit.Path.Clear(); unit.Order = kind; unit.TargetId = -1; return true; }
            if (kind == OrderKind.Attack)
            {
                UnitState target = Find(targetId);
                if (target == null || !target.Alive || target.Team == team) return false;
                destination = target.Position;
            }
            Queue<Point2> path = Grid.FindPath(unit.Position, destination);
            if (path == null) return false;
            unit.Path = path; unit.Destination = destination; unit.Order = kind; unit.TargetId = targetId; return true;
        }
        public void Step(bool enemyAi)
        {
            if (Outcome != MatchOutcome.Running) return;
            if (enemyAi && Tick % 10 == 0) UpdateEnemyAi();
            for (int i = 0; i < units.Count; i++) pendingDamage[i] = 0;
            foreach (UnitState unit in units)
            {
                if (!unit.Alive) continue;
                unit.Cooldown = Math.Max(0, unit.Cooldown - StepSeconds);
                UnitState target = Find(unit.TargetId);
                if (target == null || !target.Alive || target.Team == unit.Team) target = NearestEnemy(unit, unit.Stats.Range);
                bool inRange = target != null && Point2.DistanceSquared(unit.Position, target.Position) <= unit.Stats.Range * unit.Stats.Range && Grid.HasLineOfSight(unit.Position, target.Position);
                if (unit.Order == OrderKind.Attack && target != null && !inRange && Tick % 10 == 0)
                { Queue<Point2> chase = Grid.FindPath(unit.Position, target.Position); if (chase != null) unit.Path = chase; }
                if (unit.Order != OrderKind.Hold && !(inRange && (unit.Order == OrderKind.Attack || unit.Order == OrderKind.AttackMove))) Move(unit);
                // Recheck after moving so a moving attacker cannot fire from beyond its range.
                if (target != null && target.Alive && unit.Cooldown <= 0 && Point2.DistanceSquared(unit.Position, target.Position) <= unit.Stats.Range * unit.Stats.Range && Grid.HasLineOfSight(unit.Position, target.Position))
                {
                    pendingDamage[target.Id] += UnitStats.CalculateDamage(unit.Stats.Damage, target.Stats.Armor);
                    unit.Cooldown = unit.Stats.FireInterval; unit.LastShotTick = Tick;
                    if (Shot != null) Shot(unit.Id, target.Id);
                }
            }
            // Apply shots together: a unit alive at the start of this tick can return fire.
            for (int i = 0; i < units.Count; i++) units[i].Health = Math.Max(0, units[i].Health - pendingDamage[i]);
            Tick++;
            int player = CountAlive(0), enemy = CountAlive(1);
            if (player == 0 && enemy == 0) Outcome = MatchOutcome.Draw;
            else if (player == 0) Outcome = MatchOutcome.PlayerDefeat;
            else if (enemy == 0) Outcome = MatchOutcome.PlayerVictory;
        }
        void Move(UnitState unit)
        {
            float remaining = unit.Stats.Speed * StepSeconds;
            while (remaining > 0 && unit.Path.Count > 0)
            {
                Point2 next = unit.Path.Peek(); float d = (float)Math.Sqrt(Point2.DistanceSquared(unit.Position, next));
                unit.Position = Point2.MoveTowards(unit.Position, next, remaining); remaining -= d;
                if (d <= unit.Stats.Speed * StepSeconds && Point2.DistanceSquared(unit.Position, next) < 0.00001f) unit.Path.Dequeue();
            }
            if (unit.Path.Count == 0 && unit.Order == OrderKind.Move) unit.Order = OrderKind.Hold;
        }
        UnitState NearestEnemy(UnitState source, float range)
        {
            UnitState best = null; float distance = range * range;
            foreach (UnitState other in units)
            {
                if (!other.Alive || other.Team == source.Team) continue;
                float d = Point2.DistanceSquared(source.Position, other.Position);
                if (d <= distance && Grid.HasLineOfSight(source.Position, other.Position)) { distance = d; best = other; }
            }
            return best;
        }
        void UpdateEnemyAi()
        {
            foreach (UnitState unit in units)
            {
                if (unit.Team != 1 || !unit.Alive) continue;
                UnitState target = null; float best = float.MaxValue;
                foreach (UnitState other in units)
                    if (other.Team == 0 && other.Alive && Point2.DistanceSquared(unit.Position, other.Position) < best)
                    { best = Point2.DistanceSquared(unit.Position, other.Position); target = other; }
                if (target != null) Command(1, unit.Id, OrderKind.Attack, target.Position, target.Id);
            }
        }
        public int CountAlive(int team) { int count = 0; foreach (UnitState u in units) if (u.Team == team && u.Alive) count++; return count; }
        public ulong StateHash()
        {
            ulong h = 14695981039346656037UL;
            h = Hash(h, BitConverter.GetBytes(Tick));
            foreach (UnitState unit in units)
            {
                h = Hash(h, BitConverter.GetBytes(unit.Id)); h = Hash(h, BitConverter.GetBytes(unit.Team));
                h = Hash(h, BitConverter.GetBytes(unit.Position.X)); h = Hash(h, BitConverter.GetBytes(unit.Position.Y));
                h = Hash(h, BitConverter.GetBytes(unit.Health)); h = Hash(h, BitConverter.GetBytes((int)unit.Order));
                h = Hash(h, BitConverter.GetBytes(unit.TargetId)); h = Hash(h, BitConverter.GetBytes(unit.Cooldown));
                h = Hash(h, BitConverter.GetBytes(unit.Destination.X)); h = Hash(h, BitConverter.GetBytes(unit.Destination.Y));
                foreach (Point2 point in unit.Path) { h = Hash(h, BitConverter.GetBytes(point.X)); h = Hash(h, BitConverter.GetBytes(point.Y)); }
            }
            return h;
        }
        static ulong Hash(ulong h, byte[] bytes) { unchecked { foreach (byte b in bytes) h = (h ^ b) * 1099511628211UL; } return h; }
    }
}
