# WAR WW3 — progress

Scope: Phase 1 and the first offline skirmish from Phase 2. No multiplayer, world campaign, or release claim.

## Execution plan

1. Inspect repository and tools; preserve existing files.
2. Create a Unity 6 / URP project, independent C# simulation, and repeatable validation scripts.
3. Implement menu, Thai/English switching, settings, RTS camera, selection, formation orders, combat, AI, and match outcome.
4. Generate original Blender prototype vehicles and scenery, bundle a licensed Thai font, and prepare Unity asset generation.
5. Compile and test everything supported by installed tools, resolve failures, and record limits.
6. Write Thai and English setup and development documentation, then commit the milestone.

## Environment inspection

- Repository: clean `main`, no tracked project files at start; Git identity is configured.
- Windows PowerShell 5.1, Python 3.12, .NET Framework compiler, Blender 5.2.2 available.
- Unity Editor / Hub and .NET SDK not found in PATH, standard install locations, or installed-app registry. Unity import, play tests, screenshots, player builds, and GPU/FPS profiling require an Editor installation.
- Do not mark either phase complete until the Unity runtime and performance checks are actually run.

## Status

Implementation in progress. Verification results will be recorded after execution.
