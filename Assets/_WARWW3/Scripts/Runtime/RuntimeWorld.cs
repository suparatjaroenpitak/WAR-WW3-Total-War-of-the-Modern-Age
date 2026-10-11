using UnityEngine;
using UnityEngine.Rendering;
using WarWW3.Core;

namespace WarWW3
{
    public static class VisualPalette
    {
        static Material[] materials;
        public static Material Get(int index)
        {
            if (materials == null)
            {
                Color[] colors = { new Color(0.15f, 0.65f, 0.72f), new Color(0.85f, 0.29f, 0.19f), new Color(0.085f, 0.11f, 0.13f), new Color(0.24f, 0.29f, 0.25f), new Color(0.4f, 0.43f, 0.39f), new Color(0.14f, 0.18f, 0.2f), new Color(0.12f, 0.28f, 0.22f), new Color(0.95f, 0.83f, 0.4f) };
                materials = new Material[colors.Length];
                for (int i = 0; i < colors.Length; i++)
                {
                    var template = Resources.Load<Material>(i == 7 ? "Materials/Unlit" : "Materials/Lit");
                    materials[i] = template != null ? new Material(template) : new Material(Shader.Find(i == 7 ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit"));
                    materials[i].color = colors[i]; materials[i].enableInstancing = true;
                }
            }
            return materials[index];
        }
        public static void Dispose() { if (materials != null) foreach (var mat in materials) Object.Destroy(mat); materials = null; }
    }
    public static class RuntimeWorld
    {
        public static GameObject Cube(string name, Vector3 position, Vector3 scale, int color, Transform parent = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name; go.transform.SetParent(parent, false); go.transform.localPosition = position; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = VisualPalette.Get(color); Object.Destroy(go.GetComponent<Collider>()); return go;
        }
        public static void Create()
        {
            var root = new GameObject("Fictional region — Redvale proving ground").transform;
            Cube("Ground", new Vector3(64, -0.4f, 48), new Vector3(128, 0.8f, 96), 3, root);
            Cube("East west highway", new Vector3(64, 0.015f, 48), new Vector3(128, 0.04f, 6), 5, root);
            Cube("Service road", new Vector3(66, 0.02f, 48), new Vector3(5, 0.04f, 96), 5, root);
            for (int x = 4; x < 128; x += 8) Cube("Lane marker", new Vector3(x, 0.05f, 48), new Vector3(3, 0.04f, 0.18f), 7, root);
            Building(root, 27, 17, 6, 6, 7); Building(root, 36, 28, 5, 7, 9); Building(root, 18, 32, 4, 5, 5);
            var random = new System.Random(2219);
            for (int i = 0; i < 50; i++)
            {
                float x = (float)random.NextDouble() * 124 + 2, z = i % 2 == 0 ? (float)random.NextDouble() * 12 + 2 : 82 + (float)random.NextDouble() * 12;
                Cube("Tree trunk", new Vector3(x, 1, z), new Vector3(0.4f, 2, 0.4f), 2, root);
                var crown = GameObject.CreatePrimitive(PrimitiveType.Sphere); crown.name = "Tree crown"; crown.transform.SetParent(root); crown.transform.position = new Vector3(x, 3, z); crown.transform.localScale = new Vector3(2, 3.5f, 2); crown.GetComponent<Renderer>().sharedMaterial = VisualPalette.Get(6); Object.Destroy(crown.GetComponent<Collider>());
            }
            var sun = new GameObject("Sun", typeof(Light)).GetComponent<Light>(); sun.type = LightType.Directional; sun.intensity = 1.5f; sun.transform.rotation = Quaternion.Euler(48, -32, 0); sun.shadows = LightShadows.Hard;
            RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(0.47f, 0.55f, 0.6f);
            RenderSettings.fog = true; RenderSettings.fogColor = new Color(0.16f, 0.22f, 0.25f); RenderSettings.fogMode = FogMode.Linear; RenderSettings.fogStartDistance = 100; RenderSettings.fogEndDistance = 240;
        }
        static void Building(Transform parent, int x, int y, int width, int height, int elevation)
        {
            Cube("Depot obstacle", new Vector3(x * 2 + width, elevation * 0.5f, y * 2 + height), new Vector3(width * 2, elevation, height * 2), 4, parent);
            Cube("Depot roof", new Vector3(x * 2 + width, elevation + 0.2f, y * 2 + height), new Vector3(width * 2, 0.4f, height * 2), 2, parent);
        }
        public static GameObject Vehicle(UnitDefinition definition, int team)
        {
            var root = new GameObject("Unit");
            if (definition != null && definition.visualPrefab != null) Object.Instantiate(definition.visualPrefab, root.transform);
            else
            {
                Cube("Hull", new Vector3(0, 0.65f, 0), new Vector3(2, 0.8f, 3), team, root.transform);
                Cube("Turret", new Vector3(0, 1.3f, 0), new Vector3(1.4f, 0.6f, 1.4f), team, root.transform);
                Cube("Barrel", new Vector3(0, 1.45f, 1.5f), new Vector3(0.2f, 0.2f, 2), 2, root.transform);
                for (int side = -1; side <= 1; side += 2) Cube("Track", new Vector3(side * 1.05f, 0.4f, 0), new Vector3(0.4f, 0.7f, 3.2f), 2, root.transform);
            }
            foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>())
                renderer.sharedMaterial = VisualPalette.Get(renderer.gameObject.name.ToLowerInvariant().Contains("track") || renderer.gameObject.name.ToLowerInvariant().Contains("barrel") || renderer.gameObject.name.ToLowerInvariant().Contains("wheel") ? 2 : team);
            return root;
        }
        public static Vector3 World(Point2 p) { return new Vector3(p.X, 0, p.Y); }
    }
}
