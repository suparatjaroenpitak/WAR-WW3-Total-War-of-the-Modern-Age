using System;
using System.Collections.Generic;

namespace WarWW3.Core
{
    public sealed class BattleGrid
    {
        public readonly int Width, Height;
        public readonly float CellSize;
        readonly bool[] blocked;
        static readonly int[] Dx = { 1, 0, -1, 0 }, Dy = { 0, 1, 0, -1 };
        public BattleGrid(int width, int height, float cellSize)
        {
            if (width <= 0 || height <= 0 || width > 512 || height > 512 || cellSize <= 0 || float.IsNaN(cellSize) || float.IsInfinity(cellSize)) throw new ArgumentException("Invalid grid.");
            Width = width; Height = height; CellSize = cellSize; blocked = new bool[width * height];
        }
        public bool Walkable(int x, int y) { return x >= 0 && y >= 0 && x < Width && y < Height && !blocked[y * Width + x]; }
        public bool IsWalkable(Point2 p) { return p.IsFinite && Walkable((int)Math.Floor(p.X / CellSize), (int)Math.Floor(p.Y / CellSize)); }
        public void Block(int x, int y, int width, int height)
        { for (int j = y; j < y + height; j++) for (int i = x; i < x + width; i++) if (i >= 0 && j >= 0 && i < Width && j < Height) blocked[j * Width + i] = true; }
        public Point2 Center(int x, int y) { return new Point2((x + 0.5f) * CellSize, (y + 0.5f) * CellSize); }
        public bool HasLineOfSight(Point2 a, Point2 b)
        {
            int steps = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(Point2.DistanceSquared(a, b)) / (CellSize * 0.25f)));
            for (int i = 0; i <= steps; i++)
            { float t = (float)i / steps; if (!IsWalkable(new Point2(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t))) return false; }
            return true;
        }
        public Point2 NearestWalkable(Point2 point)
        {
            int x = Math.Max(0, Math.Min(Width - 1, (int)Math.Floor(point.X / CellSize)));
            int y = Math.Max(0, Math.Min(Height - 1, (int)Math.Floor(point.Y / CellSize)));
            for (int radius = 0; radius < Math.Max(Width, Height); radius++)
                for (int j = y - radius; j <= y + radius; j++) for (int i = x - radius; i <= x + radius; i++)
                    if ((Math.Abs(i - x) == radius || Math.Abs(j - y) == radius) && Walkable(i, j)) return Center(i, j);
            return point;
        }
        public Queue<Point2> FindPath(Point2 start, Point2 end)
        {
            if (!IsWalkable(start) || !IsWalkable(end)) return null;
            int sx = (int)(start.X / CellSize), sy = (int)(start.Y / CellSize), ex = (int)(end.X / CellSize), ey = (int)(end.Y / CellSize);
            int source = sy * Width + sx, goal = ey * Width + ex;
            int[] parent = new int[blocked.Length], cost = new int[blocked.Length];
            for (int i = 0; i < parent.Length; i++) { parent[i] = -1; cost[i] = int.MaxValue; }
            Heap open = new Heap(); cost[source] = 0; open.Push(source, Math.Abs(ex - sx) + Math.Abs(ey - sy));
            while (open.Count > 0)
            {
                int current = open.Pop();
                if (current == goal)
                {
                    List<Point2> reversed = new List<Point2>();
                    while (current != source) { reversed.Add(Center(current % Width, current / Width)); current = parent[current]; }
                    reversed.Reverse(); Queue<Point2> path = new Queue<Point2>(reversed); path.Enqueue(end); return path;
                }
                int cx = current % Width, cy = current / Width;
                for (int n = 0; n < 4; n++)
                {
                    int x = cx + Dx[n], y = cy + Dy[n]; if (!Walkable(x, y)) continue;
                    int next = y * Width + x, g = cost[current] + 1;
                    if (g >= cost[next]) continue;
                    parent[next] = current; cost[next] = g; open.Push(next, g + Math.Abs(ex - x) + Math.Abs(ey - y));
                }
            }
            return null;
        }
        sealed class Heap
        {
            struct Entry { public int Node, Score; public Entry(int n, int s) { Node = n; Score = s; } }
            readonly List<Entry> entries = new List<Entry>();
            public int Count { get { return entries.Count; } }
            public void Push(int node, int score)
            {
                Entry e = new Entry(node, score); entries.Add(e); int i = entries.Count - 1;
                while (i > 0) { int p = (i - 1) / 2; if (entries[p].Score <= score) break; entries[i] = entries[p]; i = p; } entries[i] = e;
            }
            public int Pop()
            {
                int result = entries[0].Node; Entry last = entries[entries.Count - 1]; entries.RemoveAt(entries.Count - 1); if (entries.Count == 0) return result;
                int i = 0;
                while (i * 2 + 1 < entries.Count)
                { int child = i * 2 + 1; if (child + 1 < entries.Count && entries[child + 1].Score < entries[child].Score) child++; if (entries[child].Score >= last.Score) break; entries[i] = entries[child]; i = child; }
                entries[i] = last; return result;
            }
        }
    }
}
