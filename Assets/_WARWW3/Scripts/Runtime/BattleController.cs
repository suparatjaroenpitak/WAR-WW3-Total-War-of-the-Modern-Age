using System.Collections.Generic;
using UnityEngine;
using WarWW3.Core;

namespace WarWW3
{
    public sealed class BattleController : MonoBehaviour
    {
        public BattleSimulation Simulation { get; private set; }
        public int SelectedCount { get { int n = 0; foreach (int id in selection) if (Simulation.Find(id).Alive) n++; return n; } }
        public bool AttackMoveArmed { get; private set; }
        public bool Paused { get; set; }
        readonly Dictionary<int, UnitView> views = new Dictionary<int, UnitView>();
        readonly HashSet<int> selection = new HashSet<int>();
        readonly Dictionary<int, List<int>> groups = new Dictionary<int, List<int>>();
        Camera viewCamera;
        GameUI ui;
        float accumulator;
        Vector2 dragStart;
        bool dragging;
        readonly LineRenderer[] beams = new LineRenderer[24];
        readonly float[] beamExpiry = new float[24];
        int nextBeam;
        public void Initialize(Camera camera, GameUI gameUi, GameLocalizer locale)
        {
            viewCamera = camera; ui = gameUi;
            var tank = Resources.Load<UnitDefinition>("Units/Bulwark"), scout = Resources.Load<UnitDefinition>("Units/Kestrel");
            Simulation = PrototypeScenario.Create(tank != null ? tank.ToStats() : PrototypeScenario.DefaultTank(), scout != null ? scout.ToStats() : PrototypeScenario.DefaultScout());
            foreach (var unit in Simulation.Units)
            {
                var vehicle = RuntimeWorld.Vehicle(unit.Stats.Id == "bulwark" ? tank : scout, unit.Team);
                var view = vehicle.AddComponent<UnitView>(); view.Initialize(unit); views.Add(unit.Id, view);
            }
            for (int i = 0; i < beams.Length; i++)
            {
                var line = new GameObject("Pooled tracer", typeof(LineRenderer)).GetComponent<LineRenderer>();
                line.sharedMaterial = VisualPalette.Get(7); line.widthMultiplier = 0.1f; line.positionCount = 2; line.gameObject.SetActive(false); beams[i] = line;
            }
            Simulation.Shot += OnShot;
        }
        void Update()
        {
            for (int i = 0; i < beams.Length; i++) if (beams[i].gameObject.activeSelf && Time.time >= beamExpiry[i]) beams[i].gameObject.SetActive(false);
            if (Input.GetKeyDown(KeyCode.Escape)) { dragging = false; ui.SetSelectionBox(null); if (AttackMoveArmed) AttackMoveArmed = false; else ui.TogglePause(); }
            if (Paused || Simulation.Outcome != MatchOutcome.Running) return;
            accumulator += Time.deltaTime;
            // Limit work after stalls. Retain the remainder, rather than changing simulation dt.
            int steps = 0;
            while (accumulator >= BattleSimulation.StepSeconds && steps++ < 5)
            { Simulation.Step(true); accumulator -= BattleSimulation.StepSeconds; }
            if (Simulation.Outcome != MatchOutcome.Running) { ui.ShowOutcome(Simulation.Outcome); return; }
            if (ui.BlocksWorld) return;
            HandleInput();
        }
        void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.X)) AttackMoveArmed = true;
            if (Input.GetKeyDown(KeyCode.H)) foreach (int id in selection) Simulation.Command(0, id, OrderKind.Hold, Simulation.Find(id).Position);
            if (Input.GetKeyDown(KeyCode.Tab)) { selection.Clear(); foreach (var unit in Simulation.Units) if (unit.Team == 0 && unit.Alive) selection.Add(unit.Id); RefreshSelection(); }
            for (int number = 1; number <= 5; number++)
            {
                if (!Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha0 + number))) continue;
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) groups[number] = new List<int>(selection);
                else if (groups.ContainsKey(number)) { selection.Clear(); foreach (int id in groups[number]) if (Simulation.Find(id).Alive) selection.Add(id); RefreshSelection(); }
            }
            if (Input.GetMouseButtonDown(0) && !GameUI.PointerOverUI())
            {
                if (AttackMoveArmed) { IssueGroundOrder(OrderKind.AttackMove); AttackMoveArmed = false; return; }
                dragStart = Input.mousePosition; dragging = true;
            }
            if (dragging && Input.GetMouseButton(0)) ui.SetSelectionBox(ScreenRect(dragStart, Input.mousePosition));
            if (dragging && Input.GetMouseButtonUp(0))
            {
                dragging = false; ui.SetSelectionBox(null);
                if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)) selection.Clear();
                Vector2 end = Input.mousePosition;
                if (Vector2.Distance(end, dragStart) < 9)
                { int picked = PickUnit(0); if (picked >= 0) { if (!selection.Add(picked)) selection.Remove(picked); } }
                else
                {
                    Rect rect = ScreenRect(dragStart, end);
                    foreach (var pair in views)
                    { Vector3 p = viewCamera.WorldToScreenPoint(pair.Value.transform.position); if (pair.Value.State.Team == 0 && pair.Value.State.Alive && p.z > 0 && rect.Contains(p)) selection.Add(pair.Key); }
                }
                RefreshSelection();
            }
            if (Input.GetMouseButtonDown(1) && !GameUI.PointerOverUI())
            {
                dragging = false; ui.SetSelectionBox(null); int target = PickUnit(1);
                if (target >= 0) foreach (int id in selection) Simulation.Command(0, id, OrderKind.Attack, Simulation.Find(target).Position, target);
                else IssueGroundOrder(AttackMoveArmed ? OrderKind.AttackMove : OrderKind.Move);
                AttackMoveArmed = false;
            }
        }
        int PickUnit(int team)
        {
            int best = -1; float distance = 27 * 27;
            foreach (var pair in views)
            {
                if (pair.Value.State.Team != team || !pair.Value.State.Alive) continue;
                Vector3 screen = viewCamera.WorldToScreenPoint(pair.Value.transform.position + Vector3.up);
                float d = Vector2.SqrMagnitude((Vector2)screen - (Vector2)Input.mousePosition);
                if (screen.z > 0 && d < distance) { distance = d; best = pair.Key; }
            }
            return best;
        }
        void IssueGroundOrder(OrderKind order)
        {
            var ray = viewCamera.ScreenPointToRay(Input.mousePosition); float enter;
            if (!new Plane(Vector3.up, Vector3.zero).Raycast(ray, out enter)) return;
            Vector3 hit = ray.GetPoint(enter);
            if (hit.x < 0 || hit.x >= 128 || hit.z < 0 || hit.z >= 96) return;
            var ids = new List<int>(selection); ids.Sort(); int columns = Mathf.CeilToInt(Mathf.Sqrt(ids.Count));
            for (int i = 0; i < ids.Count; i++)
            {
                var destination = new Point2(hit.x + (i % columns - (columns - 1) * 0.5f) * 4, hit.z + (i / columns) * 4);
                Simulation.Command(0, ids[i], order, Simulation.Grid.NearestWalkable(destination));
            }
        }
        void RefreshSelection() { foreach (var pair in views) pair.Value.Select(selection.Contains(pair.Key)); }
        static Rect ScreenRect(Vector2 a, Vector2 b) { return Rect.MinMaxRect(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y)); }
        void OnShot(int source, int target)
        {
            int slot = nextBeam++ % beams.Length; var line = beams[slot];
            line.SetPosition(0, RuntimeWorld.World(Simulation.Find(source).Position) + Vector3.up * 1.5f);
            line.SetPosition(1, RuntimeWorld.World(Simulation.Find(target).Position) + Vector3.up);
            line.gameObject.SetActive(true); beamExpiry[slot] = Time.time + 0.08f;
        }
        void OnDestroy() { if (Simulation != null) Simulation.Shot -= OnShot; }
    }
}
