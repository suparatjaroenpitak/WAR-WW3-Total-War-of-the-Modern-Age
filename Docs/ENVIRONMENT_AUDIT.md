# WAR WW3 — Environment Audit (Phase 0)

Audit date: 2026-10-11. All figures were read directly from the machine; nothing is assumed.

## Operating System

| Item | Value |
|---|---|
| OS | Windows 10.0.26200 (Windows 11) 64-bit, x86_64 |
| Shell used for audit | Git Bash (MSYS2) 3.6.10 |

## Hardware

| Item | Value | Assessment |
|---|---|---|
| CPU | Intel Core i7-10750H @ 2.60 GHz, 12 logical cores | Mid-range laptop CPU, adequate for development. |
| RAM | 15.77 GB | Matches the 16 GB target. Editor + compilation will be tight; compile jobs should be limited to avoid OOM. |
| GPU | NVIDIA GeForce RTX 3060 Laptop GPU (reported WMI AdapterRAM 4.29 GB, actual 6 GB class) | Main rendering device. Matches the 6 GB VRAM target. |
| iGPU | Intel UHD Graphics | Secondary; game settings must remain playable here or at Low preset on 3060. |
| Disk | D: 932 GB total, 637 GB free | Plenty for UE projects, DDC, and packaging. |

## Engine

| Item | Value |
|---|---|
| Unreal Engine | 5.8.3, branch `++UE5+Release-5.8`, changelist 58210709, promoted build |
| Install path | `C:\Program Files\Epic Games\UE_5.8` |
| Install state | Complete (`ToolComplete.txt` present). Editor, UnrealPak, UnrealLightmass binaries all present. |

Plugins of note already available in the engine: Automation (functional testing), Automation (`CQTest`), `StructUtils`, MassEntity, World Partition, OnlineSubsystem (see `EOSSDK-Management` in install list).

## Build Toolchain — CRITICAL GAP

| Item | Status |
|---|---|
| Visual Studio 2022 / Build Tools | **NOT FOUND** (no `vswhere.exe`, no VS directories under Program Files) |
| MSVC `cl.exe` | NOT FOUND on PATH |
| Windows 10/11 SDK | NOT FOUND (`C:\Program Files (x86)\Windows Kits` missing) |
| MSBuild | NOT FOUND on PATH |

Consequence: UnrealBuildTool cannot compile C++ game modules until Visual Studio Build Tools (C++ workload) is installed. **User authorization obtained on 2026-10-11 to install `Microsoft.VisualStudio.2022.BuildTools` with the VC++ tools + Windows SDK workloads via winget.** Install progress is tracked in `Docs/PROJECT_STATUS.md`.

## Other Tools

| Tool | Version | Notes |
|---|---|---|
| Git | 2.56.0.windows.1 | Sufficient. |
| winget | Available (`%LOCALAPPDATA%\Microsoft\WindowsApps\winget.exe`) | Will be used for the toolchain install. |
| Python | 3.12.10 (`python`), 3.14.7 (`py`) | Available for content/data script tooling. |
| dotnet SDK | Not installed | Not needed for UE scripting; DO NOT install unless a task requires it. |

## Repository

| Item | Value |
|---|---|
| Branch | `main` |
| History | 2 commits on 2026-10-11 (fresh repository), 1 human contributor |
| Tracked files | `.gitattributes` only |
| Existing game code | None — project starts from zero. |

## Compiler-Requirement Considerations

UE 5.8.3 on Windows requires an MSVC v14.3x (VS 2022) toolset and a Windows SDK (10.0.17763 or newer; the UE launcher-used default is SDK 10.0.19041+). The VS Build Tools install must include:

- `Microsoft.VisualStudio.Workload.VCTools` workload with `--includeRecommended` (brings in MSVC v143, Windows 11 SDK, and C++ CMake support).

Estimated footprint: ~6–8 GB. The 637 GB free on D: (and system C:) is not a concern, but UBT may default to installing Build Tools on `C:\` unless a custom path is set; the default path is acceptable.

## Risks and Decisions Made

1. **Compilation is blocked by the missing MSVC toolchain** until the Build Tools install completes. Milestones that require compiling C++ are marked pending in `Docs/PROJECT_STATUS.md` until this resolves.
2. **RAM headroom is 16 GB.** Limit compilation to linked jobs (`-NoLogLocalCache` optional), keep the editor closed (or run `-nullrhi` game targets) while building, and avoid running the Unreal Editor during full rebuilds.
3. **The user selected a 3D low-poly terrain campaign map** (spring-performance concert interaction choice on 2026-10-11). Phase 2's campaign map will be a low-poly terrain world built cheaply (instanced meshes, limited PCG, scalable settings), not a flat 2D map.
4. Target spec is confirmed adequate: development will stay within the 16 GB RAM and ~6 GB VRAM envelope, with Low/Medium/High scalability presets from the first commit.

## Files and Authorization Log

| Time (2026-10-11) | Event |
|---|---|
| — | Repository inspected: only `.gitattributes` present. |
| — | System/OS, GPU, CPU, RAM, disk, UE install, Build Tools absence, Python, Git, winget: all verified. |
| — | User approved installing VS Build Tools via winget (ask_questions → "Yes, install via winget"). |
| — | User chose 3D low-poly terrain campaign map for Phase 2. |
