# Infinite survival implementation

Latest source of truth: LATEST_REQUIREMENTS.txt (2026-11-03). Supersedes the earlier 20-minute clear design.

## Lifecycle
GameManager requires a configured CampaignController. START opens Character Select, then Stage Select.
HasCharacterSelection guards BeginRun as well as the disabled Next button. Retry retains selection.
No countdown, time victory or final-chest victory path remains. EndRun(true) is ignored for compatibility;
only EndRun(false), normally called on lethal health damage, transitions to GameOver.
Legacy UI/upgrade components remain as serialized compatibility adapters, not active stat choices.

## Time and bosses
GameTimer.Elapsed advances while Playing; Campaign Stage.Elapsed applies the optional practice multiplier.
SurvivalRules owns HH:MM:SS formatting, wave schedule, unlock-wave identity and growth calculations.
StageDirector increments boss wave every 600 simulated seconds, independently of previous living bosses.
At wave 2 (default unlockBossTime=1200), IsFinalBoss is a retained internal compatibility name meaning
UNLOCK BOSS, not a final/run-ending boss. The identity stays on the enemy if its death is delayed.
Death increments separate elite/boss counters, drops the guaranteed chest, and saves next-stage unlock.
Normal spawning never stops at a milestone. Elite frequency decreases to a data-driven minimum.
Boss HP grows from its first-wave baseline, then from the unlock-wave baseline on later waves.
BossBrain scales attack interval, projectile count, area and damage from StageData; BossProjectile uses
swept player collision. Warning rings delay their damage. All attacks freeze outside Playing.
Normal enemies respect the normal count cap; elite/boss events bypass it to preserve scheduled encounters.

## Equipment
8 weapons + 8 enhancements in the content catalog; each run has separate caps of 7 + 7.
Only Weapon/Passive are legal level-up kinds. Duplicate-free random candidates are limited to three.
If there are fewer legal candidates, offer only those. With none, consume pending choices and continue
level growth without an artificial non-equipment choice. Chests can still evolve MAX weapons.
Evolution requires weapon MAX + its specific enhancement. It only occurs in a chest, in the same slot,
without consuming the enhancement. Each reward reevaluates evolution, then owned upgrades, then gold.
Weapon icons change to EvolvedWeapon0..7 after evolution. Enhancements have no evolution state.

## Data / migration
CampaignSetup creates only missing ScriptableObject assets; fills missing new portrait/frame/icon fields.
Existing combat balance edits are retained. FormerlySerializedAs maps duration to unlockBossTime and
finalBossHP to unlockBossHP. Apply refreshes the original Move reference while preserving action UUIDs.
5 characters have unique portrait/4-direction frame sets; 80 walking frames + 5 portraits added.
All original game art is generated from project-owned pixel drawing code; no third-party sprites.

## Save
PlayerPrefs key PixelSurvivor.Campaign.v1 is intentionally unchanged. Payload version is 2.
New records list uses characterId + stageId and monotonic maximum seconds. Existing gold, upgrades,
character IDs and stage unlock count migrate without reset. Save every 15 seconds, on death and exit.
Practice skips Run gold/record/unlock changes. Permanent shop purchases still save normally.
Restart resets Run equipment, HP, EXP, timers, enemies, supports; persistent progress remains.

## UI / dependencies
Separate uGUI main, character, stage, shop, HUD and modal panels. Canvas Scaler reference 1600x900.
Original GameUI retains HP/EXP/level/kills, now with increasing survival time. Korean text with embedded Noto Sans CJK KR Regular (SIL OFL).
Input System 1.20.1, Value Move action, WASD + Arrow composites, shared ClampMagnitude movement.
Unity official package reference: https://docs.unity.com/en-us/engine/6000.6/manual/packages-list/packages-all/pack-safe/com-unity-inputsystem

## Limits
Unity compilation/runtime/build tests have NOT RUN here. Source inspection is not a substitute.
No audio, cloud leaderboard, object pooling or mid-run resume. Bosses reuse enemy sprites, and default
patterns are shared with Stage-dependent data. Endless performance/balance needs Unity playtesting.

## Korean update (2026-11-04)
KoreanText translates known default content without changing IDs, stats or user-edited custom strings.
KoreanSetup embeds a dynamic OTF font and applies it to the existing scene HUD.
CampaignUI uses the same Resources font; shop effects display percentages and cooldown reduction signs.
The Apply Korean menu delegates to existing scene migration. No gameplay or save format changes.
