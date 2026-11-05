# Pixel Survivor 개발 지침

## 프로젝트와 기준 문서

- 사용자와 한국어로 소통한다. 최신 사용자 요청을 우선하고, 요청 범위에 필요한 변경을 완료한다.
- 이 프로젝트는 2D 탑다운 픽셀 아트 무한 생존 로그라이트 RPG다. 자동 공격, 장비 조합과 진화, 영구 성장, 스테이지 해금이 핵심이다.
- 게임 규칙의 기준은 `Documentation/LATEST_REQUIREMENTS.txt`다. 이전 기획과 충돌하면 이 문서를 우선한다.
- UI 작업은 `UPDATE_README.txt`, `Documentation/HUD_UPDATE.md`, `Documentation/KOREAN_UPDATE.md`도 읽는다.
- 구조 파악에는 `Documentation/ARCHITECTURE.md`, 검증에는 `Documentation/QA_CHECKLIST.md`를 참고한다. 과거 검증 기록을 현재 실행 결과로 보고하지 않는다.

## 환경과 주요 경로

- Unity 버전은 `ProjectSettings/ProjectVersion.txt`를 확인한다. 작성 시점 기준 `6000.6.6f1`이다.
- Windows PC 우선, Unity Input System 사용. 패키지 버전은 `Packages/manifest.json`을 기준으로 한다.
- 런타임 코드: `Assets/PixelSurvivor/Scripts/` (`PixelSurvivor` 네임스페이스).
- `Core`: 게임 상태와 생존 규칙. `Campaign`: 스테이지, 필드 이벤트, 저장과 영구 성장.
- `Combat`, `Enemy`, `Player`: 전투와 캐릭터. `Equipment`, `Progression`: 장비, 진화와 레벨업.
- `Data`: 데이터 정의. `UI`: 한국어 UI, HUD와 메뉴.
- Editor 도구: `Assets/PixelSurvivor/Editor/`.
- 메인 씬: `Assets/PixelSurvivor/Scenes/MainScene.unity`.
- 테스트: `Assets/PixelSurvivor/Tests/EditMode/`, `Assets/PixelSurvivor/Tests/PlayMode/`.

## Unity 연결과 작업 방식

- Unity를 제어할 때 설치된 `unity-cli` 및 프로젝트의 `.agents/skills/unity-pipeline/SKILL.md`와 작업에 필요한 참조 문서를 읽는다.
- 셸을 사용할 수 있으면 Unity CLI를 사용한다. 먼저 프로젝트 루트에서 `unity status --json`으로 상태를 확인한다.
- Editor 제어 명령은 `--project-path`에 현재 작업 중인 프로젝트의 절대 경로를 전달한다. 포트와 PID는 고정하지 않고 상태 조회 결과를 사용한다.
- 명령 이름과 인자는 연결된 Editor의 명령 목록이나 도움말로 확인한다. `unity command --query <검색어>`로 탐색할 수 있다.
- 스킬을 통한 `unity command` 호출에는 해당 스킬 지침에 따라 `--caller plugin --skill <스킬명>`을 붙인다.
- 샌드박스 접근 거부는 Editor 미실행과 구분한다. 권한 문제는 도구의 승인 절차로 처리하며 다른 프로젝트의 Editor에 연결하지 않는다.
- C#은 파일에서 편집하고, 씬·프리팹·직렬화된 에셋은 가능한 한 연결된 Editor API와 기존 Editor 도구로 수정한다.
- 기존 씬에는 필요한 경우 `Tools/Pixel Survivor/UI 업데이트 적용 (Apply HUD Update)`를 사용한다. 전체 재생성이 요청되지 않았다면 `Setup - Rebuild Game Scene`을 실행하지 않는다.

## 변경 원칙

- 기존 사용자 변경을 먼저 확인하고 보존한다. 요청과 무관한 대규모 포맷 변경, 패키지 업그레이드, 에셋 재생성을 피한다.
- `.meta` 파일과 GUID를 보존한다. 에셋 이동이나 이름 변경은 참조가 유지되도록 처리한다.
- `Library/`, `Temp/`, `Logs/`, `obj/` 같은 생성 폴더를 소스처럼 수정하지 않는다.
- 기존 코드 스타일과 어셈블리 구분을 따른다. 런타임 어셈블리에 `UnityEditor` 의존성을 넣지 않는다.
- 사용자 요청이 없으면 무한 생존 규칙, 공격형 7칸·강화형 7칸, 한국어 UI와 WASD/방향키 지원을 유지한다.
- 저장 호환성을 보존한다. `SaveManager`의 기본 PlayerPrefs 키는 `PixelSurvivor.Campaign.v1`이다. 테스트는 별도 키 또는 메모리 저장을 사용하고 사용자 진행도를 초기화하지 않는다.

## 검증과 완료 보고

- C# 변경 후 Unity 재컴파일 완료를 기다리고 컴파일 오류와 Console 오류를 확인한다.
- 규칙·저장·장비 변경에는 관련 EditMode 테스트, 실행 중 동작·UI 변경에는 관련 PlayMode 테스트 또는 실제 Play 검증을 수행한다.
- Play 검증에서는 실제 시간과 프레임이 진행되는지 확인한다. UI 변경은 화면도 확인한다. Play 진입만으로 정상 작동을 단정하지 않는다.
- 변경과 관련된 검증을 우선한다. 문서만 변경했을 때는 Unity 실행이나 전체 테스트가 필요하지 않다.
- Windows 빌드가 요청되면 기존 `Tools/Pixel Survivor/Build Windows x64` 또는 `PixelSurvivor.Editor.SurvivorBuild.Windows`를 활용한다. 출력은 `Builds/PixelSurvivor/PixelSurvivor.exe`이며 배포 시 전체 출력 폴더가 필요하다.
- 완료 보고에는 변경 사항, 실제 수행한 검증, 남은 오류나 미검증 범위를 간결하게 적는다.
