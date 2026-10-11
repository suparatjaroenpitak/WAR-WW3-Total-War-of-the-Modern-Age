using UnityEngine;
using WarWW3.Core;

namespace WarWW3
{
    public sealed class UnitView : MonoBehaviour
    {
        public UnitState State { get; private set; }
        GameObject ring;
        Transform health;
        public void Initialize(UnitState state)
        {
            State = state; transform.position = RuntimeWorld.World(state.Position);
            ring = new GameObject("Selection ring", typeof(LineRenderer)); ring.transform.SetParent(transform, false);
            var line = ring.GetComponent<LineRenderer>(); line.useWorldSpace = false; line.sharedMaterial = VisualPalette.Get(7); line.widthMultiplier = 0.12f; line.loop = true; line.positionCount = 32;
            for (int i = 0; i < 32; i++) { float angle = i * Mathf.PI * 2 / 32; line.SetPosition(i, new Vector3(Mathf.Cos(angle) * 2, 0.15f, Mathf.Sin(angle) * 2)); }
            ring.SetActive(false);
            RuntimeWorld.Cube("Health background", new Vector3(0, 2.7f, 0), new Vector3(2.5f, 0.18f, 0.22f), 2, transform);
            health = RuntimeWorld.Cube("Health", new Vector3(0, 2.85f, 0), new Vector3(2.4f, 0.14f, 0.2f), state.Team, transform).transform;
            transform.rotation = Quaternion.Euler(0, state.Team == 0 ? 90 : -90, 0);
        }
        public void Select(bool selected) { ring.SetActive(selected && State.Alive); }
        void Update()
        {
            if (!State.Alive) { gameObject.SetActive(false); return; }
            var target = RuntimeWorld.World(State.Position), delta = target - transform.position;
            if (delta.sqrMagnitude > 0.005f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(delta), Time.deltaTime * 8);
            transform.position = Vector3.MoveTowards(transform.position, target, State.Stats.Speed * Time.deltaTime * 1.8f);
            float ratio = (float)State.Health / State.Stats.Health;
            health.localScale = new Vector3(2.4f * ratio, 0.14f, 0.2f); health.localPosition = new Vector3((ratio - 1) * 1.2f, 2.85f, 0);
        }
    }
}
