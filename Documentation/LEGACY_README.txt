[SUPERSEDED — 이전 버전 이력. 현재 기준은 LATEST_REQUIREMENTS.txt / README.txt입니다.]

PIXEL SURVIVOR : LUMEN FIELD
Unity 6.6 / 6000.6.6f1용 2D 탑다운 로그라이트 MVP

[몬스터/모션 확장판]
이미 실행 중인 프로젝트에 적용하려면 UPDATE_README.txt를 먼저 보십시오.
총 적 3종과 플레이어 4방향 걷기 모션이 추가되었습니다.

[2026-11-02 패키지 오류 수정]
Input System을 1.20.1로 갱신했습니다. 기존 1.18.0의 PlayerInputEditor가
Unity 6.6에서 EntityId 관련 CS0619 오류를 내는 문제를 피하기 위한 변경입니다.
Unity 6.5부터 제거된 com.unity.modules.vr 의존성을 삭제했습니다.
이미 이전 ZIP을 적용했다면 Unity를 종료하고 Packages/manifest.json에서
"com.unity.modules.vr": "1.0.0", 한 줄을 삭제한 뒤 저장하고 다시 여십시오.
전체 프로젝트를 새로 만들거나 기존 작업물을 삭제할 필요는 없습니다.

[먼저 알아둘 점]
이 ZIP은 C# 전체, PNG 원본, 입력 에셋, 패키지/프로젝트 설정,
씬·프리팹 생성용 Editor Setup, 자동 테스트를 포함한 프로젝트입니다.
씬과 프리팹은 아래 Setup 메뉴를 실행할 때 생성됩니다.
Windows 실행 파일(EXE)은 포함하지 않습니다.

제작 환경에 Unity Editor가 없어 6000.6.6f1 컴파일과 Play 실행은 하지 못했습니다.
C# 문법 파싱, 파일 구성, 입력 바인딩, 코드 참조 구조는 검사했습니다.
실제 Unity Console 오류 0건 또는 5분 실플레이 통과를 보장한 결과물이 아닙니다.
상세 결과: Documentation/TEST_RESULTS.md

============================================================
1. 가장 쉬운 실행 방법 — 이 프로젝트를 그대로 열기
============================================================
1) ZIP을 모두 압축 해제합니다.
   예: C:\UnityProjects\PixelSurvivor_Unity66
2) Unity Hub > Projects > Add > Add project from disk
3) Assets / Packages / ProjectSettings가 들어 있는
   PixelSurvivor_Unity66 폴더를 선택합니다. Assets만 선택하면 안 됩니다.
4) Unity 6000.6.6f1로 엽니다.
5) 최초 패키지 다운로드와 스크립트 컴파일이 끝날 때까지 기다립니다.
   최초 실행에는 인터넷과 Unity 라이선스가 필요합니다.
6) 상단 메뉴:
   Tools > Pixel Survivor > Setup - Rebuild MVP Scene
7) Assets/PixelSurvivor/Scenes/MainScene.unity가 생성되고 열립니다.
8) Game 탭 해상도를 1600x900 또는 1920x1080으로 설정합니다.
9) 상단 Play ▶ 버튼을 누른 뒤 게임의 START 버튼을 클릭합니다.
10) WASD로 이동합니다. 공격은 자동입니다.

입력 시스템을 전환하라는 창이 나오면 New Input System 사용을 허용하고 재시작합니다.
현재 설정은 Input System Package (New)입니다. Both / Old를 사용하지 않습니다.

Setup은 처음 한 번만 필요합니다. 다음부터 MainScene을 열고 Play하면 됩니다.
Setup 재실행은 이 프로젝트의 생성된 씬/프리팹을 재작성합니다.
Inspector에서 바꾼 수치는 Setup 재실행 전에 별도로 기록하십시오.
C# 원본과 PNG 원본은 Setup이 덮어쓰지 않습니다.

============================================================
2. 새 Unity 프로젝트에 적용하는 방법
============================================================
1) Unity 6000.6.6f1의 2D Built-In Render Pipeline 새 프로젝트를 만듭니다.
2) Unity를 종료합니다.
3) ZIP의 Assets/PixelSurvivor 폴더를 새 프로젝트의 Assets 안에 복사합니다.
4) 새 프로젝트 Packages/manifest.json에 다음 의존성이 있는지 확인합니다.
   "com.unity.inputsystem": "1.20.1"
   "com.unity.ugui": "2.0.0"
   "com.unity.test-framework": "1.4.5"
   JSON 쉼표에 주의하십시오. 새 빈 프로젝트라면 제공한 manifest.json을 사용해도 됩니다.
   기존 작업 프로젝트의 전체 Packages / ProjectSettings를 덮어쓰지는 마십시오.
5) Unity를 다시 열고 패키지 다운로드를 기다립니다.
6) Edit > Project Settings > Player > Other Settings > Configuration
   > Active Input Handling = Input System Package (New)
7) 요청에 따라 Unity를 재시작합니다.
8) Tools > Pixel Survivor > Setup - Rebuild MVP Scene
9) Play > START.

이 배포본은 Built-In 렌더 파이프라인 기준입니다.
URP 프로젝트에 섞기보다 제공한 프로젝트 또는 새 2D Built-In 프로젝트를 사용하십시오.
Python, 외부 폰트, Asset Store 에셋, Cinemachine, TMP Essentials는 필요하지 않습니다.

============================================================
3. 조작과 게임 규칙
============================================================
W / A / S / D : 이동. 대각선 속도 보정 적용.
마우스 클릭 : START, 강화 카드, RESTART 선택.
방향키 + Enter : 메뉴 버튼/강화 카드 선택. 강화는 좌우 방향키 사용.
공격 버튼 : 없음. 화면 안의 가장 가까운 적을 에너지 볼트로 자동 공격.
플레이 종료 : Unity Play 버튼 해제. Windows 빌드는 창 닫기 또는 Alt+F4.

플레이어: HP 100 / 이동 5 / 피해 10 / 공격 간격 0.8초 / 투사체 속도 8.
슬라임: HP 20 / 이동 2 / 접촉 피해 10.
나방: HP 10 / 이동 3.4 / 접촉 피해 8, 30초부터 등장 가능.
딱정벌레: HP 60 / 이동 1.25 / 접촉 피해 18, 90초부터 등장 가능.
모든 적은 사망 시 EXP 1 보석을 1개 드롭합니다.
피격 직후 0.65초 무적: 여러 적이 겹쳐도 한 프레임에 즉사하지 않습니다.
EXP 1개 = 1 EXP. 가까운 보석은 약하게 끌어당겨 회수하기 쉽게 했습니다.
필요 EXP = 5 + (현재 레벨 - 1) × 3.
강화 선택 중에는 적, 공격, 이동, 타이머가 모두 정지합니다.
여러 레벨을 한 번에 얻으면 선택을 차례로 처리하고 남는 EXP를 유지합니다.
공격 간격 최소 0.12초. 하한 도달 시 공격속도 강화는 후보에서 제외됩니다.
각 선택 화면의 강화 3개는 서로 중복되지 않습니다.
퍼센트 강화는 현재 능력치에 곱해서 누적됩니다.
최대 HP 강화는 최대 HP +20, 현재 HP +20입니다.

HP가 0이 되면 GAME OVER.
실제 전투 진행 시간 300초를 채우면 SURVIVED!.
레벨업 화면에서 머문 시간은 300초에 포함되지 않습니다.
결과 화면은 생존 시간, 도달 레벨, 처치 수를 보여 줍니다.
RESTART는 능력치, HP, EXP, 레벨, 타이머, 적, 보석, 투사체를 초기화합니다.
게임 UI는 영문입니다. 한글 폰트 의존성 없이 즉시 열리도록 구성했습니다.
설명서는 한국어로 제공됩니다.

============================================================
4. Inspector에서 수정할 곳
============================================================
Play를 끈 상태에서 수정하십시오. Play 중 변경한 값은 종료 시 되돌아갑니다.

Hierarchy > Player > PlayerStats
  Max HP           100
  Move Speed       5
  Damage           10
  Attack Interval  0.8
  Projectile Speed 8

Hierarchy > Player > PlayerHealth
  Invulnerability Seconds 0.65
Hierarchy > Player > AutoAttack
  Range 24 (범위 내이면서 화면 안에 있는 적만 겨냥)

Project > Assets/PixelSurvivor/Prefabs/Enemy
  HP              20
  Move Speed      2
  Contact Damage  10
  Radius          0.38

Hierarchy > EnemySpawner
  Spawn Intervals [1.5, 1.2, 0.9, 0.7, 0.5]
  각 원소는 0~59초 / 60~119초 / 120~179초 / 180~239초 / 240초 이후
  Spawn Distance  12 (플레이어와의 최소 생성 거리)
  Screen Padding  1.5 (실제 카메라 화면 외부 여유 거리)
  Max Enemy Count 120

Hierarchy > GameManager > GameTimer
  Round Duration  300
  승리 확인을 빠르게 하려면 10으로 바꾼 후 Play. 제출/정상 플레이 전 300으로 복구.

Hierarchy > Map > MapBounds
  Half Size (32, 20): 실제 전투 영역 64 × 40
  맵 크기 변경 시 Ground의 SpriteRenderer Size와 경계 표시 위치도 함께 수정.
Hierarchy > Main Camera > Camera
  Orthographic Size 9

Project > Prefabs/ExperienceGem
  Experience 1 / Pickup Radius 0.65 / Magnet Radius 1.65 / Magnet Speed 9
Project > Prefabs/Projectile
  Hit Radius 0.13 / Lifetime 4

값을 장기적으로 기본값으로 바꾸려면 해당 C#의 SerializeField 초기값도 변경하십시오.
Setup은 프리팹을 C# 기본값으로 다시 생성합니다.

============================================================
5. 폴더와 스크립트 역할
============================================================
Assets/PixelSurvivor/Scripts/Core/
  GameManager.cs  상태 전환, 초기화, 강화 선택, 승패
  GameTimer.cs    전투 시간과 라운드 길이
  GameRules.cs    EXP 공식, 생성 단계, 이동 보정, 투사체 충돌 수학
Scripts/Player/
  PlayerMovement.cs  InputActionReference 기반 WASD 이동, 맵 경계
  PlayerHealth.cs    HP, 접촉 피해 무적 시간, 피격 점멸
  PlayerStats.cs     시작 능력치와 강화 적용
  PlayerSpriteAnimator.cs  4방향 걷기, 실제 이동량 기반 프레임 교체
Scripts/Combat/
  AutoAttack.cs     대상 검색과 발사 주기
  Projectile.cs     이동, 연속 충돌 판정, 피해와 제거
Scripts/Enemy/
  Enemy.cs          추적, 약한 밀집 분산, HP, 사망, 경험치 드롭
  EnemySpawner.cs   화면 바깥 생성, 시간별 주기, 적 수 제한
Scripts/Progression/
  ExperienceGem.cs  근접 끌어당김과 습득
  PlayerLevel.cs    레벨, EXP, 연속 레벨업 처리
  UpgradeSystem.cs  5종 강화 정의와 중복 없는 무작위 후보
Scripts/World/
  MapBounds.cs      플레이어 경계, 드롭 위치 제한
  CameraFollow.cs   고정 각도의 Orthographic 추적, 맵 끝 제한
Scripts/UI/
  GameUI.cs         메뉴, HUD, HP/EXP 바
  LevelUpUI.cs      강화 화면과 카드 연결
  UpgradeButton.cs  개별 강화 카드
  ResultUI.cs       승리/패배, 통계, 재시작

Editor/
  SurvivorSetup.cs  씬·7개 프리팹·입력 참조 자동 생성, 구성 검사
  SurvivorBuild.cs  Windows x64 빌드 메뉴
Input/
  Survivor.inputactions   Gameplay/Move와 UI 입력
  *Reference.asset        Setup이 생성하는 영구 InputActionReference
Art/
  Player.png 32x32 / Enemy.png 32x32
  Player_Down/Up/Left/Right_0~3.png 각 32x32
  EnemyMoth.png / EnemyBeetle.png 각 32x32
  Projectile.png 8x8 / ExperienceGem.png 12x12
  Ground.png 64x64 / Tuft.png 16x16 / White.png 1x1
Prefabs/  (Setup 이후)
  Player / Enemy / EnemyMoth / EnemyBeetle / Projectile / ExperienceGem / UpgradeButton
Scenes/MainScene.unity  (Setup 이후)
Tests/EditMode/RulesTests.cs
Tests/PlayMode/GameplayTests.cs
Documentation/
  TEST_RESULTS.md / QA_CHECKLIST.md / ARCHITECTURE.md
  SpritePreview.png : PNG 원본 미리보기. 실행 화면이 아닙니다.
  generate_pixel_assets.py : 도트 PNG 재생성용 선택 도구(Python+Pillow 필요).

씬 Hierarchy:
GameManager / Player / EnemySpawner / Map / Main Camera / RuntimeObjects /
EventSystem / Canvas
Canvas 하위: MainMenuPanel / GameplayPanel / LevelUpPanel / ResultPanel

============================================================
6. 자동 검사와 수동 플레이 확인
============================================================
1) Setup 후 Tools > Pixel Survivor > Validate Current Scene 실행.
   필수 게임 참조, 입력 참조, UI, 프리팹, Sprite, Missing Script를 검사합니다.
   성공 시 Console에 검사 통과 로그가 남습니다.
2) Window > General > Test Runner를 엽니다.
3) EditMode > Run All.
4) PlayMode > Run All.
   먼저 Setup이 MainScene과 Build Settings를 생성해 두어야 합니다.
   테스트 중에는 키보드/마우스를 조작하지 마십시오.
5) Documentation/QA_CHECKLIST.md 순서대로 실제 플레이를 확인합니다.

자동 테스트는 런타임 동작 확인용이며 이 배포 환경에서 실행하지 못했습니다.
테스트 결과는 사용자의 Unity Test Runner가 보여 주는 결과를 기준으로 판단하십시오.

============================================================
7. Windows 실행 파일 만들기
============================================================
1) Unity Hub에서 이 버전의 Windows Build Support를 설치합니다.
2) Setup과 테스트 후 Play를 끕니다.
3) Tools > Pixel Survivor > Build Windows x64
4) 생성 위치: Builds/PixelSurvivor/PixelSurvivor.exe
5) 다른 PC로 보낼 때 PixelSurvivor.exe만 보내지 말고
   Builds/PixelSurvivor 폴더 전체를 압축합니다.

============================================================
8. 문제가 생기면
============================================================
Tools 메뉴가 없다 / 빨간 컴파일 오류:
  Window > General > Console에서 첫 번째 빨간 오류를 확인합니다.
  패키지 설치가 끝났는지 먼저 확인하십시오.
  InputSystem namespace 오류: Packages/manifest.json의 inputsystem 1.20.1 확인.
  UI namespace / assembly 오류: ugui 2.0.0 확인.
  asmdef의 UI 참조는 UnityEngine.UI입니다(패키지 이름 Unity.ugui가 아님).
  테스트에서 NUnit 오류: test-framework 1.4.5 확인.

WASD가 작동하지 않는다:
  Play 후 START를 누르고 Game 탭을 클릭했는지 확인.
  Active Input Handling = Input System Package (New), 재시작 완료 여부 확인.
  Player > PlayerMovement > Move Action 연결 확인. Setup으로 자동 복구 가능.

버튼이 작동하지 않는다:
  EventSystem의 InputSystemUIInputModule 확인.
  Canvas의 GraphicRaycaster 확인. Setup으로 복구 가능.

Missing Reference / Missing Script:
  Play 종료 후 Setup 재실행 > Validate Current Scene.
  Scripts 파일 이름을 바꾸거나 삭제했다면 ZIP 원본과 대조.
  생성 에셋을 옮겼다면 Scene과 Prefabs를 원래 폴더에 되돌리고 Setup 재실행.

게임이 멈춘 것처럼 보인다:
  시작·강화·결과 화면에서는 Time Scale 0이 정상입니다.
  START / 강화 카드 / RESTART를 선택하십시오.

캐릭터가 분홍색이다:
  제공 프로젝트는 Built-In 기준입니다.
  URP 기존 프로젝트 대신 이 ZIP 프로젝트 자체를 Unity Hub로 여십시오.

보석이나 투사체에 Collider가 없다:
  의도된 구조입니다. 원/선분 수학 충돌과 거리 판정을 사용합니다.
  Rigidbody2D, Physics2D 또는 레이어 설정이 필요하지 않습니다.
  이동/발사/피해는 실제 런타임 C#이 수행합니다.

오류를 전달할 때:
  Console의 첫 번째 빨간 오류 전문 + 파일명/줄 번호 + Unity 버전을 함께 알려 주십시오.
  한 오류가 뒤의 여러 오류를 만들 수 있으므로 첫 번째부터 해결합니다.

============================================================
9. 현재 범위와 이후 확장
============================================================
이번 확장판: 1캐릭터(4방향 모션), 3적, 1무기, 5강화, 1맵, 5분 생존 루프.
종류별 HP/이동속도는 고정이며 생성 빈도와 시간별 적 구성으로 난이도를 높입니다.
Object Pooling 대신 Instantiate/Destroy를 사용합니다. 적 수는 120으로 제한합니다.
사운드, 영구 저장, 영구 강화, 보스, 새 무기는 이번 버전에 넣지 않았습니다.
프레임 성능과 난이도/재미는 실제 PC에서 추가 조정이 필요합니다.

확장 순서:
1) 실플레이 수치 조정 및 Object Pooling
2) AutoAttack/Projectile을 기반으로 무기 정의 ScriptableObject 분리
3) Enemy 프리팹 변형으로 새 적, Elite 추가
4) 보스 패턴과 경고 UI
5) 캐릭터 선택과 영구 성장 저장
6) MapBounds를 유지하면서 Tilemap 확장
7) 사운드, 추가 도트 애니메이션

모든 PNG는 이 프로젝트를 위해 코드로 만든 독립적인 단순 도트입니다.
상용 게임에서 가져온 그래픽은 없습니다.
PNG 교체는 같은 파일명/크기를 유지하는 것이 가장 쉽습니다.
Unity에서 Filter Mode=Point, Pixels Per Unit=32로 사용합니다.
플레이어 모션 외형은 Player_방향_번호.png 16개를 교체하십시오. 게임 로직 수정은 필요 없습니다.
