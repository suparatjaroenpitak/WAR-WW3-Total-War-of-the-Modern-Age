# WAR WW3 — Known Issues

Updated: 2026-10-11

| ID | Severity | Description | Mitigation / plan |
|---|---|---|---|
| KI-001 | Blocking | MSVC Build Tools install was still running at time of last update; no C++ compile has executed yet. | Re-compile and update status as soon as installer finishes. |
| KI-002 | Medium | `ABattlePlayerController::ServerIssueMoveOrder` does not yet verify unit ownership (Phase 5 TODO). In single-player this is harmless; in multiplayer it would let a client move enemy units. | Implement PlayerState team IDs and ownership enforcement in Phase 5 before any multiplayer playtest. |
| KI-003 | Low | `UMainMenuWidget` language matching is substring-based ("ไทย"/"Thai"). Fragile if display names change. | Store language codes as combo items in Phase 2 UI pass. |
| KI-004 | Low | `UTurnPipeline::FStateView` is a plain C++ struct, not USTRUCT — fine for tests but not exposed to Blueprints. | Wrap into USTRUCT if Blueprint designers need to tweak turn economics in Phase 2. |
| KI-005 | Low | `UWAWW3GameSettings::SetLanguage` calls `SetToDefaults()` on GameUserSettings, overwriting any user resolution choices. | Replace with targeted language-only persistence (PlayerConfig/Game instance Config) in Phase 2. |
| KI-006 | Low | Thai font rendering has not been verified — no font asset yet. | Add Noto Thai chain font + audit all screens both languages in Phase 2 UI pass (acceptance: no tofu glyphs). |
| KI-007 | Info | Multiplayer, matchmaking, NAT traversal are NOT implemented. | Only plan-level work exists; see PROJECT_STATUS before claiming otherwise. |
