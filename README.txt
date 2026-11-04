[최신 업데이트: 전투 HUD + ESC 정보창]
적용은 UPDATE_README.txt의 순서를 따라주세요.
Tools > Pixel Survivor > UI 업데이트 적용 (Apply HUD Update)
아래의 이전 한국어 적용 메뉴도 동일한 업데이트를 실행합니다.

[2026-11-04 한국어판 업데이트]
이번 ZIP에는 한국어 UI와 한글 폰트가 포함되어 있습니다.
기존 프로젝트: Unity 종료 → 새 Assets 전체를 기존 Assets와 합치기/덮어쓰기
→ Unity 실행 → MainScene → Tools > Pixel Survivor > 한국어 적용 (Apply Korean)
→ Play → 시작 → 캐릭터 선택 → 다음: 스테이지 선택 → 전투 시작.
기존 Apply Infinite Survival Update 메뉴도 한국어를 함께 적용합니다.
한글 폰트 Resources/Fonts 폴더를 빠뜨리지 마세요. 별도 폰트 설치는 필요 없습니다.
아래에서 영어 버튼 이름이 나오는 경우 기존 버전 명칭 설명입니다.
상세 변경/확인 결과: Documentation/KOREAN_UPDATE.md.

Pixel Survivor — 무한 생존 업데이트 (2026-11-03)
대상: Unity 6.6 / 6000.6.6f1, Windows PC, Input System 1.20.1
최신 기준: Documentation/LATEST_REQUIREMENTS.txt

[이미 실행되던 프로젝트에 적용 — 권장]
1. Unity Editor를 종료합니다. Unity Hub는 열려 있어도 됩니다.
2. ZIP을 별도 폴더에 압축 해제합니다.
3. 압축 해제한 PixelSurvivor_Unity66 안의 Assets 폴더를 복사합니다.
4. 기존 프로젝트의 Assets / Packages / ProjectSettings가 보이는 위치에 붙여넣습니다.
   같은 파일은 덮어쓰기합니다. 기존 Assets를 먼저 삭제하지 마세요.
   Assets 안에 Assets가 생기는 구조가 아니라, 같은 Assets 폴더끼리 합쳐야 합니다.
5. Documentation, README.txt, UPDATE_README.txt, VERSION.txt도 새 버전으로 복사합니다.
   정상 실행 중인 기존 프로젝트의 Packages / ProjectSettings / .meta는 유지합니다.
6. Unity Hub에서 기존 프로젝트를 엽니다. 임포트/컴파일이 끝날 때까지 기다립니다.
7. 기존 MainScene을 엽니다.
8. 상단 Tools > Pixel Survivor > Apply Infinite Survival Update 를 클릭합니다.
9. Console에 '한국어 무한 생존 업데이트 적용 완료'가 표시되면 상단 ▶ Play를 누릅니다.
10. START > 캐릭터 카드 클릭 > NEXT: STAGE > 스테이지 > START RUN.

이번 적용 메뉴의 정확한 이름은 'Apply Infinite Survival Update'입니다.
'Apply Campaign Expansion'만 보이면 새 Assets가 해당 프로젝트에 덮어써지지 않은 것입니다.
정상 사용 중인 씬을 유지하려면 Rebuild 메뉴 대신 Apply 메뉴를 사용합니다.
Apply는 기존 씬을 유지하며 캠페인 데이터, 새 그림 참조와 입력을 연결합니다.

[새 프로젝트로 여는 경우]
1. ZIP 전체를 압축 해제합니다.
2. Hub > 추가 > 디스크에서 프로젝트 추가 > Assets/Packages/ProjectSettings가 들어 있는
   PixelSurvivor_Unity66 폴더를 선택합니다.
3. 6000.6.6f1로 열고 패키지 임포트를 기다립니다.
4. Tools > Pixel Survivor > Setup - Rebuild Game Scene 실행.
5. Scene, Prefab, ScriptableObject 데이터가 자동 생성됩니다. 저장 후 Play.
미리 생성된 Scene/Prefab/.asset과 Windows EXE는 ZIP에 없습니다.
Editor Setup이 원본 C#, PNG, Input Actions를 사용해 이 파일들을 만듭니다.

[바뀐 핵심 규칙]
- 제한시간 없음. 5분/20분 자동 승리 없음. 사망 시 결과 화면.
- 시간은 HH:MM:SS로 계속 증가합니다. 일시정지, 레벨업, 상자에서는 멈춥니다.
- 보스는 10, 20, 30, 40분… 계속 등장합니다.
- 20분 등장 보스를 잡으면 다음 Stage를 영구 해금하지만 현재 Run은 계속됩니다.
  그 보스를 30분 이후에 잡아도 해금됩니다. 맵 자동 전환은 없습니다.
- 일반 몬스터도 20분 이후 계속 생성됩니다. 시간에 따라 HP/접촉 대미지/속도,
  생성 빈도/수, 엘리트 빈도가 증가합니다.
- 보스도 회차에 따라 HP/접촉 및 패턴 대미지/공격 빈도/탄 수/범위가 증가합니다.
- 메인 메뉴 START / UPGRADE / QUIT, 별도 캐릭터 선택, 별도 Stage 선택 화면.
- 5명 서로 다른 스탯/시작 무기/고유 패시브/외형/4방향 걷기 프레임.
- 캐릭터를 선택하기 전 NEXT와 게임 시작이 차단됩니다.
- 레벨업은 공격형/강화형 장비만 선택합니다. 단순 스탯/골드/힐/지원 선택지는 없습니다.
- 공격형 7칸 + 강화형 7칸. 같은 장비 재선택 시 같은 슬롯에서 레벨 증가.
- 공격형 MAX + 지정 강화형 보유 -> EVOLUTION READY -> 상자에서 진화.
  강화형 자체는 진화하지 않고, 진화 재료로 소모되지 않습니다.
- 진화하면 같은 슬롯의 이름/아이콘/공격 성능이 바뀝니다.
- 엘리트 상자 100%: 기본 1회, 3회/5회 잭팟 기본 가중치 85/12/3.
- 보스 상자 100%: 항상 3회. 매 보상마다 진화 가능 여부를 다시 검사합니다.
- 더 강화할 장비가 없으면 상자 보상을 골드로 변환합니다.
- 파괴 가능한 보급 상자, Gold/Magnet/Bomb/Heal/Support Call 유지.
- 임시 지원군: 공격 드론, 회복 드론, 몬스터 우회용 장벽. 장비 슬롯 사용 안 함.
- 결과: 생존 시간/레벨/총 처치/엘리트 처치/보스 처치/획득 골드/최고 기록.
- 최고 생존 기록은 캐릭터 ID + Stage ID별로 저장합니다.
- 기존 저장 키를 그대로 쓰므로 이전 골드/공통 강화/고유 강화/해금 상태를 유지합니다.

[레벨업 선택지 소진 규칙]
중복 없는 장비 후보가 3개 이상이면 무작위 3개를 표시합니다.
후보가 1~2개면 그 수만큼만 표시합니다. 모든 장비가 MAX라 후보가 없으면
레벨만 올라가고 전투를 계속합니다. 상자를 통한 진화는 여전히 가능합니다.
MAX 장비를 반복 강화하거나 장비가 아닌 대체 선택지를 만들지 않습니다.

[조작]
WASD 또는 방향키: 이동. 동일 Move Action의 2개 2DVector Composite입니다.
대각선과 W+↑ 같은 동시 입력도 기본 속도를 넘지 않습니다.
공격: 자동. 마우스: 메뉴/카드/상자/상점/장비 슬롯/PAUSE.
PAUSE > RESUME: 재개. RESTART: 같은 캐릭터/Stage 새 Run.
MAIN MENU: 메뉴로 복귀. UPGRADE의 CHANGE CHARACTER로 고유 강화 대상을 바꿉니다.
QUIT은 Windows 빌드에서 종료합니다. Editor에서는 상단 Play 버튼으로 정지합니다.

[캐릭터]
Ari 밸런스: HP100, 이동5, 대미지10, 간격0.8, Energy Bolt, EXP+10%.
Vela 속도: HP70, 이동6.4, 대미지9, 간격0.65, Wind Dagger, 회피8%.
Bram 파워: HP150, 이동3.8, 대미지15, 간격1.05, Light Whip, 넉백+40%/방어2.
Mira 메이지: HP75, 이동4.7, 대미지10, 간격0.85, Orbit Orb, 범위+20%/탄+1.
Nox 리퍼: HP65, 이동5.5, 대미지12, 간격0.8, Crescent Scythe, 잃은 HP에 비례 대미지 증가.
카드의 ATTACKS/SEC는 기본 공격 간격의 역수이며, 각 무기 고유 쿨다운도 적용됩니다.
5명은 처음부터 해금되어 있습니다. 최초 새 저장에서는 Stage 1만 열립니다.
이전 버전에서 이미 해금한 Stage는 다시 잠그지 않습니다.

[Inspector 데이터 — Setup/Apply 실행 후]
Assets/PixelSurvivor/Data/
Campaign.asset: 장비/캐릭터/Stage 목록, 드롭 확률, 지원군, 상자 가중치.
Character_0~4.asset: HP/이동/대미지/간격/시작 무기/패시브/전용 강화/그림/걷기 배열.
Weapon_0~7.asset: 최대 레벨/공격 패턴/레벨당 성장/진화 파트너/진화 성능/아이콘.
Passive_0~7.asset: 최대 레벨/효과/아이콘. 진화 필드 없음.
Stage_0~2.asset: 보스 간격600초, 해금 보스 시점1200초, 맵/스폰/스케일링.
Stage의 Boss Health Growth Per Wave 등으로 보스 성장을 수정할 수 있습니다.
기본 보스 1회 HP1200, 해금 보스 HP4500. 이후 기본 회차당 HP 성장 계수0.75.
Boss Attack Interval 5초, Minimum 1.2초, 탄수 기본4 + 회차당2, 최대24.
엘리트 간격 기본90초 / 최저20초, 분당 빈도 성장0.05.
일반 몬스터 동시 수는 EnemySpawner의 Max Enemy Count 기본120으로 제한합니다.
엘리트/보스 이벤트는 일반 수 제한과 별도로 생성합니다.
Enemy/EnemyMoth/EnemyBeetle.prefab: HP/이동/접촉 대미지 수정.
GameTimer에는 Round Duration이 없습니다. 시간은 더 이상 게임 종료 조건이 아닙니다.
Apply 재실행은 기존 캐릭터/장비 수치를 초기화하지 않고 없는 새 그림 필드를 채웁니다.
기존 Stage duration/finalBossHP 데이터는 unlockBossTime/unlockBossHP로 이전합니다.

[폴더 구조]
Assets/PixelSurvivor/Scripts/Core: 게임 상태, 증가 타이머, 규칙.
Scripts/Player: 이동, HP, 스탯, 프레임 애니메이션.
Scripts/Enemy: 몬스터, 스포너, 보스 경고 공격/투사체.
Scripts/Combat: 무기별 자동 공격, 투사체, 범위 공격.
Scripts/Progression: EXP/레벨. 기존 UpgradeSystem은 옛 파일 참조 호환용이며 전투에서 호출 안 함.
Scripts/Equipment: 7+7 장비/진화/상자 보상.
Scripts/Campaign: Run, Stage, 저장, 필드 드롭, 지원군.
Scripts/Data: ScriptableObject 클래스.
Scripts/UI: 선택/메뉴/장비/결과와 기존 HUD 호환 컴포넌트.
Art: 원본 PNG 139개. Character0~4_[방향]_[0~3], Portrait0~4, EvolvedWeapon0~7 포함.
Input/Survivor.inputactions: Move 하나 + UI Actions.
Editor: 씬/프리팹/데이터 생성과 기존 씬 업데이트, 빌드, QA 명령.
Tests: Unity EditMode / PlayMode 테스트 소스.
Documentation: 최신 요청서, 설계, 변경 파일, QA/테스트 결과, 이미지 생성기.

[검증 상태]
이 환경에는 Unity Editor / C# 컴파일러가 없습니다.
C# 문법 트리, JSON, PNG 크기/RGBA/걷기 프레임, 입력 구조, 제거된 멤버 참조,
금지된 시간 종료 경로 등 정적 검사 PASS. 정적 검사는 Unity 컴파일 검사가 아닙니다.
Unity 6000.6.6f1 실제 컴파일, Play, Windows 빌드는 미실행입니다.
Console 빨간 오류 없음 / Missing Reference 없음 / 장시간 플레이 성공을 보증하지 않습니다.
상세: Documentation/TEST_RESULTS.md, STATIC_CHECKS.json, QA_CHECKLIST.md.
Unity에서 Window > General > Test Runner의 EditMode와 PlayMode를 실행할 수 있습니다.
새 복사본에서 Documentation/RunUnityQA.ps1을 사용하면 Setup과 테스트를 일괄 실행합니다.

[빠른 확인]
메뉴 FAST TEST를 켜면 시간표만 20배로 빨라집니다. 보스 실제30초/60초/90초…
이 모드의 Run 골드/Stage 해금/최고 기록은 영구 저장하지 않습니다.
기존 영구 강화는 적용되며, 상점에서 직접 구매한 강화는 테스트 모드와 관계없이 저장됩니다.
Tools > Pixel Survivor > QA Fast Test:
Grant 100 EXP / Max starter and give evolution partner / Drop all field items /
Spawn elite / Advance to next boss / Defeat current boss / Summon barrier / End test run.
실제 저장 검증은 FAST TEST OFF에서 또는 자동 테스트의 별도 임시 저장으로 진행합니다.

[오류 대응]
메뉴가 안 보임: Console 첫 빨간 오류를 확인하고 Assets 덮어쓰기 위치를 확인합니다.
Incomplete Campaign data: MainScene에서 Apply Infinite Survival Update 실행.
Input actions missing: 패키지/파일 임포트 완료 후 재실행, Survivor.inputactions가 있는지 확인.
Input System 옛 EntityId 오류: 새 프로젝트 기준 manifest는 1.20.1입니다.
기존 프로젝트는 Window > Package Manager에서 Input System 버전을 확인합니다.
Library/PackageCache 파일을 직접 고치지 마세요.
옛 'Apply Campaign Expansion'만 보임: 현재 열린 프로젝트와 복사한 폴더가 같은지 확인.
파란/분홍 사각형 또는 Missing Sprite: Art PNG 전체와 Apply 실행 여부 확인.
문제가 남으면 Console 첫 빨간 오류 전문과 파일명/줄 번호를 확인해야 합니다.

[미구현 / 미검증 한계]
실제 Unity 실행 및 Windows 빌드 검증, 장시간 성능 프로파일링/밸런스 검증은 미완료.
사운드, 온라인 랭킹, 클라우드 저장, Run 중간 저장/재개, 오브젝트 풀링은 미구현.
게임 UI와 기본 캐릭터/장비/스테이지 데이터는 한국어입니다. Console 개발 진단과 코드 식별자는 일부 영어입니다.
보스는 Stage별 기본 몬스터 외형 확대/색상 변경을 재사용하고 공통 성장 패턴을 사용합니다.
캐릭터 Idle은 마지막 방향 정지 프레임이며 별도 호흡 애니메이션은 없습니다.
무한 생존은 종료 제한이 없다는 뜻이며 무제한 오브젝트 수를 보장한다는 뜻은 아닙니다.

기존 CAMPAIGN_REQUIREMENTS.txt/LEGACY_README.txt는 이전 버전 이력입니다.
충돌 시 LATEST_REQUIREMENTS.txt와 이 README의 무한 생존 구현을 기준으로 합니다.
