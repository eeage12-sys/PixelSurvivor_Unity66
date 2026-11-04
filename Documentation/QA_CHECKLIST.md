# Unity 6000.6.6f1 acceptance checklist — NOT RUN here

Use a backup/fresh project copy. Apply Infinite Survival Update before testing an existing MainScene.
Static checks are separately reported in STATIC_CHECKS.json; unchecked items below are not passed.

## Import
- [ ] Packages resolve; Console has no red compiler errors.
- [ ] Apply completes, Validate Current Scene completes, no missing script/reference.
- [ ] Restart Unity; menu loads with all five character images.

## Menu / characters / input
- [ ] START opens Character Select; Next disabled until choosing a card.
- [ ] Five cards show role, HP, damage, speed, attacks/sec, starting weapon and unique passive.
- [ ] Selected highlight and Stage Select work; fresh profile Stage2/3 locked.
- [ ] Each character applies distinct HP/speed/damage/starter/passive and walking sprites.
- [ ] W A S D and all four arrows move correctly; both diagonals have unit speed.
- [ ] W+Up and A+Left do not stack movement speed; boundaries stop movement/animation.
- [ ] Menu/card buttons work with mouse; in-run arrow movement does not select hidden UI.

## Survival / enemy / boss
- [ ] Survive past 05:00, 20:00 and 30:00; no automatic result screen.
- [ ] Pause, level-up and chest stop time, movement and enemy attacks.
- [ ] Normal spawn/HP/damage/speed/count scale; normal enemies keep spawning after 20:00.
- [ ] Elite appearance/color/size distinct; elite frequency scales; chest guaranteed.
- [ ] Bosses spawn at 10/20/30/40 minutes even if earlier bosses remain alive.
- [ ] Boss HP/damage/attack frequency/projectiles/pattern area scale on later waves.
- [ ] 20-minute boss death unlocks next stage and current run keeps playing.
- [ ] Killing that boss after 30:00 still unlocks; killing other waves doesn't substitute.
- [ ] Boss chest drops on ground, gives exactly 3 rewards, Continue returns to combat.

## Equipment / progression
- [ ] EXP pickup levels player; random choices are weapon/enhancement only.
- [ ] Choosing a new item consumes correct slot; reselecting levels the same slot.
- [ ] 7+7 caps hold; icons/levels are displayed; no eighth item in either category.
- [ ] Under-MAX weapon cannot evolve; MAX without required enhancement cannot evolve.
- [ ] MAX + partner shows READY but waits for a chest; partner icon/status and synergy appear.
- [ ] Chest evolution changes name/icon/attack, same slot; enhancement retained and never evolves.
- [ ] Elite chest boundaries yield 1/3/5; boss always3; fully maxed chest converts remaining rolls to gold.
- [ ] Last 1-2 legal equipment choices work; all MAX continues levels without stat/gold/heal choices.

## Field / supports
- [ ] Destructibles can be targeted and broken, dropping all 5 item types over repeated rolls.
- [ ] Magnet attracts EXP/gold/items across map; healing respects maximum HP.
- [ ] Bomb instantly kills normals but only damages elites/bosses.
- [ ] Attack/heal/barrier supports work, expire, do not consume equipment slots.
- [ ] Monsters route around barrier; player and player projectiles pass through.

## Results / persistence / restart
- [ ] Death shows survival/level/kill/elite/boss/gold/best, Restart and Main Menu.
- [ ] Restart resets equipment/evolution/HP/EXP/level/supports/time and keeps persistent progression.
- [ ] Common upgrade affects all characters; character upgrades affect only that ID.
- [ ] Gold, purchases, stage/character unlocks and per-character/stage best survive Unity restart.
- [ ] Prior version1 profile migrates without losing gold or upgrades.
- [ ] Practice doesn't save Run gold/unlocks/records; normal mode does.

## Final
- [ ] Run all EditMode / PlayMode tests and inspect XML/logs.
- [ ] Test 1600x900 and 1920x1080 UI without clipping.
- [ ] Windows x64 build launches; QUIT exits; keyboard input works outside Editor.
- [ ] Long run performance/profile and balance acceptable on target Windows PC.

## 한국어판 추가 확인 — Unity 미실행
- [ ] 한국어 적용 메뉴 실행 후 기존 영어 이름/설명이 한국어로 변경됨.
- [ ] 시작/캐릭터/스테이지/상점/레벨업/상자/결과/HUD에 네모 글자 없음.
- [ ] 1600x900 / 1920x1080에서 긴 카드 설명과 진화 조건이 잘리지 않음.
- [ ] 한국어 폰트가 설치되지 않은 Windows PC에서도 빌드 문구 표시됨.
- [ ] 적용 전 골드/영구 강화/최고 기록/해금과 Inspector 수정 수치가 유지됨.

## HUD / ESC update
Complete all 12 manual scenarios in HUD_UPDATE.md. All remain NOT RUN in this environment.
