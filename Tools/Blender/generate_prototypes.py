"""Original WAR WW3 prototypes. Blender 5.2+, no add-ons or external textures."""
import argparse
import json
import math
from pathlib import Path
import sys
import bpy
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "Assets/_WARWW3/Models"
PREVIEW = ROOT / "Docs/Art"
parser = argparse.ArgumentParser()
parser.add_argument("--force", action="store_true", help="Explicitly replace previously generated files.")
args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else [])
OUT.mkdir(parents=True, exist_ok=True)
PREVIEW.mkdir(parents=True, exist_ok=True)

def material(name, color):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*color, 1)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (*color, 1)
    bsdf.inputs["Roughness"].default_value = 0.8
    return mat

def cube(name, location, dimensions, mat):
    bpy.ops.mesh.primitive_cube_add(size=1, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.dimensions = dimensions
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(mat)
    return obj

def vehicle(name, scout, lod):
    body = material(name + "_Body", (0.12, 0.5, 0.54))
    dark = material(name + "_Tracks", (0.045, 0.06, 0.075))
    parts = []
    parts.append(cube("Hull", (0, 0, 0.75), (1.9, 3.1 if scout else 3.5, 0.7), body))
    parts.append(cube("Turret", (0, -0.15, 1.35), (1.25, 1.4, 0.55), body))
    parts.append(cube("Barrel", (0, 1.3, 1.4), (0.15 if scout else 0.22, 2.2, 0.18), dark))
    for side in (-1, 1):
        if scout:
            for y in (-1.05, 0, 1.05):
                bpy.ops.mesh.primitive_cylinder_add(vertices=8 if lod else 12, radius=0.4, depth=0.3, location=(side * 1, y, 0.4), rotation=(0, math.pi / 2, 0))
                obj = bpy.context.object; obj.name = "Wheel"; obj.data.materials.append(dark); parts.append(obj)
        else:
            parts.append(cube("Track", (side * 1.02, 0, 0.4), (0.45, 3.7, 0.75), dark))
            if not lod:
                for y in (-1.2, -0.4, 0.4, 1.2): parts.append(cube("TrackGuard", (side * 1.04, y, 0.85), (0.5, 0.7, 0.16), body))
    if not lod:
        parts.append(cube("Hatch", (0.28, -0.4, 1.68), (0.55, 0.55, 0.12), dark))
        parts.append(cube("Antenna", (-0.45, -0.7, 2), (0.04, 0.04, 1.1), dark))
        parts.append(cube("EngineDeck", (0, -1.25, 1.16), (1.4, 0.55, 0.12), dark))
    return parts

def export(name, objects):
    path = OUT / (name + ".fbx")
    if path.exists() and not args.force: raise FileExistsError(f"Refusing to overwrite {path}; use --force for regeneration.")
    bpy.ops.object.select_all(action="DESELECT")
    for obj in objects:
        obj.select_set(True)
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.mode_set(mode="EDIT")
        bpy.ops.mesh.select_all(action="SELECT")
        bpy.ops.uv.smart_project()
        bpy.ops.object.mode_set(mode="OBJECT")
    bpy.ops.export_scene.fbx(filepath=str(path), use_selection=True, object_types={"MESH"}, apply_unit_scale=True, bake_space_transform=True, axis_forward="-Z", axis_up="Y", add_leaf_bones=False, bake_anim=False)
    triangles = sum(len(p.vertices) - 2 for obj in objects for p in obj.data.polygons)
    for obj in objects: bpy.data.objects.remove(obj, do_unlink=True)
    return {"file": path.name, "triangles": triangles, "scale": "meters", "uv": "smart projected", "pivot": "ground / origin", "license": "original project asset"}

bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
bpy.context.scene.unit_settings.system = "METRIC"
manifest = []
for name, scout in (("Bulwark", False), ("Kestrel", True)):
    for lod in (0, 1): manifest.append(export(f"{name}_LOD{lod}", vehicle(name, scout, lod)))
gray = material("Depot concrete", (0.4, 0.43, 0.39))
manifest.append(export("Depot", [cube("Depot", (0, 0, 3), (10, 12, 6), gray), cube("Roof", (0, 0, 6.2), (10.4, 12.4, 0.4), gray)]))
green = material("Vegetation", (0.08, 0.23, 0.16))
trunk = cube("Trunk", (0, 0, 1), (0.4, 0.4, 2), gray)
bpy.ops.mesh.primitive_cone_add(vertices=7, radius1=1.4, radius2=0, depth=3.8, location=(0, 0, 3.2))
crown = bpy.context.object; crown.name = "Crown"; crown.data.materials.append(green)
manifest.append(export("Tree", [trunk, crown]))
(OUT / "manifest.json").write_text(json.dumps({"generator": "Tools/Blender/generate_prototypes.py", "assets": manifest}, indent=2) + "\n", encoding="utf-8")

# An actual CPU-rendered preview of the generated design, separate from Unity verification.
for offset, name, scout in ((-2.6, "Bulwark", False), (2.6, "Kestrel", True)):
    for obj in vehicle(name, scout, 0): obj.location.x += offset
floor = material("Preview ground", (0.12, 0.16, 0.18))
cube("Ground", (0, 0, -0.1), (200, 200, 0.2), floor)
bpy.ops.object.light_add(type="AREA", location=(1, -2, 9))
bpy.context.object.data.energy = 1800; bpy.context.object.data.shape = "DISK"; bpy.context.object.data.size = 8
bpy.ops.object.camera_add(location=(9, 11, 9))
camera = bpy.context.object; camera.rotation_euler = (Vector((0, 0, 0.7)) - camera.location).to_track_quat("-Z", "Y").to_euler(); camera.data.type = "ORTHO"; camera.data.ortho_scale = 13
scene = bpy.context.scene; scene.camera = camera; scene.render.engine = "CYCLES"; scene.cycles.device = "CPU"; scene.cycles.samples = 24
scene.render.resolution_x = 1200; scene.render.resolution_y = 720; scene.render.resolution_percentage = 100
scene.world.color = (0.22, 0.22, 0.22); scene.render.image_settings.file_format = "PNG"; scene.render.filepath = str(PREVIEW / "prototype-vehicles.png")
bpy.ops.wm.save_as_mainfile(filepath=str(OUT / "PrototypeVehicles.blend"))
bpy.ops.render.render(write_still=True)
print("WAR WW3: generated", len(manifest), "FBX assets; original vehicle preview rendered.")
