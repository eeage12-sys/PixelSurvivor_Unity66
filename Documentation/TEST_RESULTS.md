# Validation report — Infinite Survival update, 2026-11-04

## Executed in this environment
- Static validation: **201 checks, 0 failures** (STATIC_CHECKS.json).
- C# grammar parsing: **55 source files** with tree-sitter-c-sharp; no syntax errors found.
- JSON: package manifest, assembly definitions and Input Actions parse successfully.
- **139 original PNG game assets** decoded; expected dimensions and RGBA format checked.
- 5 character portraits are distinct; each character/direction has at least three distinct walking poses.
- CharacterPreview.png inspected visually; this is an **asset preview, not a Unity screenshot**.
- WASD and arrows share one Value Move action; IDs unique; shared movement clamp inspected.
- Source scans confirm no countdown/time victory call, no non-equipment choice kind, no stopped-after-final spawn flag.
- Migration source retains original PlayerPrefs key and adds per-character/stage records.
- Removed-member references and deprecated FindFirstObjectByType calls scanned.

The 201 static checks include individual image checks. They are **not 194 gameplay tests**.
Syntax parsing does not resolve C# types, Unity APIs or serialized references.

## Not executed — blocking environment limitation
- Unity 6000.6.6f1 import and C# type compilation: **NOT RUN**.
- EditMode / PlayMode Unity tests: **NOT RUN**.
- Console red-error check, missing reference/script checks in an actual scene: **NOT RUN**.
- 5/20/30+ minute gameplay, input response, on-screen UI rendering and restart: **NOT RUN**.
- Windows x64 build / launch / performance: **NOT RUN**.
This execution environment contains no Unity Editor or C# compiler. No claim of a clean Unity Console or successful runtime build is made.

## Supplied Unity regression tests (all NOT RUN)
- `EquipmentRulesTests.cs`: 12 cases
- `SurvivalRulesTests.cs`: 17 cases
- `RulesTests.cs`: 17 cases
- `SaveRulesTests.cs`: 5 cases
- `CampaignTests.cs`: 16 cases
- `GameplayTests.cs`: 17 cases
- `ExpansionTests.cs`: 5 cases

Total declared test cases: 89 (not pass counts).
Tests use a temporary profile or unique PlayerPrefs key, keeping the user's normal profile separate.
GameplayTests replaces old five-minute tests in the same file so overlay updates leave no stale references.
ExpansionTests now runs in the current campaign rather than silently skipping it.
CampaignTests covers 20-minute unlock without ending a run, delayed boss death, later waves, five characters,
chests/evolution, special kill counters, field objects/supports, reset and practice save isolation.
SaveRulesTests covers version1 migration, separate character upgrades and per-character/stage records.

## How to run
Use Setup/Apply, then Window > General > Test Runner. Run EditMode and PlayMode.
Or close Unity and run Documentation/RunUnityQA.ps1 on a fresh project copy. It rebuilds the generated scene.
Inspect TestResults/EditMode.xml, PlayMode.xml and all logs. Complete QA_CHECKLIST.md before treating
this source package as verified for distribution.

## Korean localization verification
Embedded Noto Sans CJK KR Regular covers all 11,172 modern Hangul syllables.
No missing Hangul glyphs in the current C# sources. Font license/source are bundled.
KoreanTextPreview.png was visually inspected as a font/text preview, not a Unity screenshot.
Current and existing-scene font paths, exact default data migration and Korean apply menu inspected.
The existing UI navigation test button labels were updated for the translated controls.
No Unity test execution is claimed by these checks.
