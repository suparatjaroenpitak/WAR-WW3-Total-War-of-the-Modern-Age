# WAR WW3 Changelog

Format: Added / Changed / Fixed / Security / Known issues. Reverse chronological.

## 2026-10-11 — Phase 0/1 scaffolding

### Added
- Unreal 5.8 project skeleton (.uproject, targets, Build.cs, DefaultEngine.ini).
- Core logging categories and constants.
- Deterministic turn pipeline (UTurnPipeline) with seven resolvable steps.
- Campaign types (FProvince, FCountry, EDiplState), battle types (EUnitClass, FUnitDef).
- UWAWW3GameSettings language switch (English/Thai) with persistence.
- AWARWW3Unit with health/damage/move + a server-validated move-order RPC on ABattlePlayerController.
- Campaign save-game schema v1 with atomic write (temp + rename).
- Turn-pipeline automation tests.
- Docs: ENVIRONMENT_AUDIT, PROJECT_STATUS, CHANGELOG, KNOWN_ISSUES.

### Known issues
- Compilation pending MSVC Build Tools installation (in progress).
- LoadCampaign slot name validation blocks only path traversal; other malformed names are rejected by filesystem.
- Battle controller ownership checks are TODO(Phase5).
