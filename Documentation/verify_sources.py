"""Static checks only; NOT a Unity compile/play test.
Optional developer dependencies: Pillow, fonttools, tree-sitter, tree-sitter-c-sharp.
"""
from pathlib import Path
import json, re, hashlib, sys
from PIL import Image
from tree_sitter import Language, Parser
import tree_sitter_c_sharp
root=Path(__file__).resolve().parents[1]
assets=root/'Assets/PixelSurvivor'
checks=[]
def check(name,condition,detail):
    checks.append({'check':name,'status':'PASS' if condition else 'FAIL','detail':detail})
parser=Parser(Language(tree_sitter_c_sharp.language()))
files=list(assets.rglob('*.cs'))
errors=[str(p.relative_to(root)) for p in files if parser.parse(p.read_bytes()).root_node.has_error]
check('C# grammar',not errors,{'files':len(files),'errors':errors,'limitation':'Not Unity compilation or type checking'})
json_files=list(root.glob('Packages/*.json'))+list(assets.rglob('*.asmdef'))+[assets/'Input/Survivor.inputactions']
for f in json_files: json.loads(f.read_text())
check('Package, assembly and input JSON',True,len(json_files))
manifest=json.loads((root/'Packages/manifest.json').read_text())['dependencies']
check('Compatibility fixes retained',manifest.get('com.unity.inputsystem')=='1.20.1' and 'com.unity.modules.vr' not in manifest,manifest.get('com.unity.inputsystem'))
expected={'Player':(32,32),'Enemy':(32,32),'Projectile':(8,8),'ExperienceGem':(12,12),'Ground':(64,64),'Tuft':(16,16),'White':(1,1),'EnemyMoth':(32,32),'EnemyBeetle':(32,32)}
for direction in ('Down','Up','Left','Right'):
    for i in range(4): expected[f'Player_{direction}_{i}']=(32,32)
for name in ['AttackRing','TreasureChest','SupplyCrate','FieldItem','SupportDrone']+[f'Weapon{i}' for i in range(8)]+[f'Passive{i}' for i in range(8)]: expected[name]=(32,32)
for k in range(5):
    expected[f'Portrait{k}']=(32,32)
    for direction in ('Down','Up','Left','Right'):
        for frame in range(4): expected[f'Character{k}_{direction}_{frame}']=(32,32)
for k in range(8): expected[f'EvolvedWeapon{k}']=(32,32)
for name,size in expected.items():
    with Image.open(assets/'Art'/f'{name}.png') as im:
        im.load(); check('PNG '+name,im.size==size and im.mode=='RGBA',{'size':im.size,'mode':im.mode})
for direction in ('Down','Up','Left','Right'):
    frames=[Image.open(assets/'Art'/f'Player_{direction}_{i}.png').tobytes() for i in range(4)]
    check('Distinct stepping poses '+direction,len(set(frames))>=3,'neutral, left stride, right stride')
rt='\n'.join(p.read_text() for p in (assets/'Scripts').rglob('*.cs'))
check('Runtime Editor isolation','using UnityEditor' not in rt,'Runtime has no UnityEditor imports')
check('No old input polling',not re.search(r'\bInput\.(GetAxis|GetKey|GetButton)',rt),'InputActionReference retained')
actions=json.loads((assets/'Input/Survivor.inputactions').read_text())
paths=[b['path'] for m in actions['maps'] for b in m['bindings']]
check('WASD retained',all('<Keyboard>/'+k in paths for k in 'wasd'),'W A S D bindings')
ids=[item['id'] for m in actions['maps'] for item in [m]+m['actions']+m['bindings']]
check('Input identifiers unique',len(ids)==len(set(ids)),len(ids))
move=next(m for m in actions['maps'] if m['name']=='Gameplay')
move_paths=[b['path'] for b in move['bindings'] if b.get('action')=='Move']
check('Single Move action with WASD + arrows',len(move['actions'])==1 and all('<Keyboard>/'+k in move_paths for k in ['w','a','s','d','upArrow','downArrow','leftArrow','rightArrow']),move_paths)
check('Value input chooses strongest composite',move['actions'][0]['type']=='Value','Value, not PassThrough; movement also clamps magnitude')
check('Movement clamps magnitude','Vector2.ClampMagnitude' in (assets/'Scripts/Core/GameRules.cs').read_text(),'Shared movement path for both layouts')
for k in range(5):
    for direction in ('Down','Up','Left','Right'):
        frames=[Image.open(assets/'Art'/f'Character{k}_{direction}_{i}.png').tobytes() for i in range(4)]
        check(f'Character{k} {direction} walking',len(set(frames))>=3,'Three distinct stepping poses minimum')
check('Five visually distinct character portraits',len({Image.open(assets/'Art'/f'Portrait{k}.png').tobytes() for k in range(5)})==5,'Pixel bytes differ across all five portraits')
for k in range(8):check(f'Evolution changes icon {k}',(assets/'Art'/f'Weapon{k}.png').read_bytes()!=(assets/'Art'/f'EvolvedWeapon{k}.png').read_bytes(),'Distinct PNG')
game=(assets/'Scripts/Core/GameManager.cs').read_text();timer=(assets/'Scripts/Core/GameTimer.cs').read_text()
check('No countdown or time victory calls','Remaining' not in timer and 'Duration' not in timer and 'EndRun(true)' not in game,'Source inspection, not runtime test')
choice=(assets/'Scripts/Equipment/EquipmentManager.cs').read_text()
check('Level-up choices are equipment only','enum EquipmentChoiceKind { Weapon, Passive }' in choice,'No Gold/Heal/Support choice kinds')
campaign=(assets/'Scripts/Campaign/CampaignController.cs').read_text();stage=(assets/'Scripts/Campaign/StageDirector.cs').read_text()
check('Chest cannot end the run','EndRun(true)' not in campaign,'Unlock boss death only changes saved stage availability')
check('Spawns continue after milestone','finalSpawned' not in stage and 'DueBosses' in stage,'No final boss stop flag')
ui=(assets/'Scripts/UI/CampaignUI.cs').read_text()
check('Menu flow and selection gate',all(x in ui for x in ['캐릭터 선택','스테이지 선택','characterNext.interactable=c.HasCharacterSelection']) and '!Campaign.HasCharacterSelection' in game,'Separate panels and runtime start guard')
save=(assets/'Scripts/Campaign/SaveManager.cs').read_text()
check('Records and old save key retained',all(x in save for x in ['PixelSurvivor.Campaign.v1','SurvivalRecord','characterId','stageId','version=2']),'Migration extends existing profile instead of resetting it')
check('Latest setup entry point exists','Apply Infinite Survival Update' in (assets/'Editor/CampaignSetup.cs').read_text(),'Existing scene migration tool')
check('Deprecated search API removed','FindFirstObjectByType' not in rt,'FindAnyObjectByType in Unity 6.6')
check('No stale removed member references',not re.search(r'(Timer\.(Duration|Remaining|SetDuration)|StageData\.duration|EquipmentChoiceKind\.(Gold|Heal|Support))','\n'.join(p.read_text() for p in files)),'Cross-file removed member scan')

setup=(assets/'Editor/SurvivorSetup.cs').read_text()
static_sprites=re.findall(r'\bSprite\("([^"]+)"\)',setup)
check('Setup static sprite paths',all((assets/'Art'/f'{x}.png').exists() for x in static_sprites),static_sprites)
check('Player Input Handling',re.search(r'activeInputHandler:\s*1\b',(root/'ProjectSettings/ProjectSettings.asset').read_text()) is not None,'New Input System only')
check('Campaign assets exist',all((assets/'Art'/f'{name}.png').exists() for name in expected),len(expected))
from fontTools.ttLib import TTFont
font_path=assets/'Resources/Fonts/NotoSansCJKkr-Regular.otf'
font=TTFont(font_path);cmap=font.getBestCmap()
hangul={ord(ch) for p in files for ch in p.read_text() if '\uac00'<=ch<='\ud7a3'}
check('Korean font covers source Hangul',all(cp in cmap for cp in hangul),{'unique_syllables':len(hangul),'missing':[hex(cp) for cp in sorted(hangul) if cp not in cmap]})
check('Korean font covers all 11172 modern Hangul syllables',all(cp in cmap for cp in range(0xac00,0xd7a4)),'Full Korean font, not current-text-only subset')
check('Font redistribution notices included',(assets/'Resources/Fonts/FONT-LICENSE.txt').is_file() and (assets/'Resources/Fonts/FONT-SOURCE.txt').is_file(),'SIL Open Font License bundled')
check('Font shipped as embedded dynamic asset',all(x in (assets/'Editor/KoreanSetup.cs').read_text() for x in ['FontTextureCase.Dynamic','includeFontData=true']),'Project supplies font; no OS-installed font dependency')
check('Korean font applied to both UI paths','font=KoreanText.Font' in ui and 'label.font=KoreanText.Font' in (assets/'Scripts/UI/GameUI.cs').read_text(),'Runtime campaign and original HUD use Korean font')
check('Default data localized on migration and runtime','KoreanText.LocalizeData(result)' in (assets/'Editor/CampaignSetup.cs').read_text() and 'KoreanText.LocalizeData(data)' in campaign,'Exact known-default translation retains IDs, stats, custom text')
check('Korean application menu included','한국어 적용 (Apply Korean)' in (assets/'Editor/KoreanSetup.cs').read_text(),'Calls existing full scene migration')
hud=(assets/'Scripts/UI/CompactHUD.cs').read_text()
pause=(assets/'Scripts/UI/CampaignPauseUI.cs').read_text()
legacy=(assets/'Scripts/UI/GameUI.cs').read_text()
prop=(assets/'Scripts/Campaign/DestructibleProp.cs').read_text()
check('Campaign suppresses old HUD','gameplayPanel.SetActive(game.Campaign==null)' in legacy,'Only CompactHUD is active in campaign gameplay')
check('Single gameplay timer source','Single timer' in hud and 'status.text=' not in ui,'No duplicate stage/status clock; pause/result summaries are separate')
check('Top experience and concise LV',all(x in hud for x in ['Top experience','level.text="LV"','expFill.fillAmount']),'Edge-anchored experience with level inside')
check('Health follows world position after camera','WorldToScreenPoint' in hud and 'DefaultExecutionOrder(200)' in hud and 'ScreenPointToLocalPointInRectangle' in hud,'LateUpdate after default camera follow')
check('Compact icon rows','34+i*48,-54' in hud and '34+i*48,-102' in hud,'7 attack + 7 passive icons at upper left')
check('Numeric counters use icons','Kill icon' in hud and 'Gold icon' in hud,'Generated original pixel glyphs, separate numeric values')
check('ESC uses existing InputActionReference','InputActionReference cancelAction' in pause and 'cancelAction=input.cancel' in pause and 'WasPressedThisFrame' in pause,'Shared UI Cancel action; no legacy Input polling')
check('ESC only toggles playing or paused','else if(c.Game.State==RunState.Paused)' in pause,'ChoosingUpgrade, Treasure and GameOver are not dismissed')
check('Pause contains stats options recipes and levels',all(x in pause for x in ['캐릭터 정보','ShowOptions','ShowRecipes','LevelPips','ResumePause']),'Source check only; supplied PlayMode regressions not run')
check('Pips reflect current and maximum','i<maximum' in pause and 'i<current?Mint' in pause,'Actual equipment data maxLevel, including evolved weapon full pips')
check('Prop hit and break feedback wired','PropHitEffect.Spawn(c,transform.position,hp<=0)' in prop and 'amount<=0' in prop,'Positive hits emit sparks; optional flash/scale reaction')
check('Saved editor preview repair','PrepareEditorPreview(font)' in (assets/'Editor/KoreanSetup.cs').read_text() and 'case "Title":label.text=' in legacy,'Restores previously cleared labels by object name')
check('HUD update menu','UI 업데이트 적용 (Apply HUD Update)' in (assets/'Editor/KoreanSetup.cs').read_text(),'Non-destructive scene overlay migration')
report={'scope':'Static source/assets inspection only. Unity unavailable; compile/play/build NOT RUN.', 'checks':checks,'failures':sum(c['status']=='FAIL' for c in checks)}
(root/'Documentation/STATIC_CHECKS.json').write_text(json.dumps(report,indent=2,ensure_ascii=False)+'\n')
print(json.dumps({'checks':len(checks),'failures':report['failures'],'cs_files':len(files),'png_assets':len(expected)}))
sys.exit(1 if report['failures'] else 0)
