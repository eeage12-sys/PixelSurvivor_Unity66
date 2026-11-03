# 테스트 결과 — 2026-11-02

## 결론

**프로젝트 소스/자동 Setup/PNG/테스트를 작성했고 정적 검사 19개가 통과했다. Unity Editor 컴파일·실행·Windows 빌드는 미실시다.**

이 제작 환경에 Unity 또는 Unity 라이선스가 설치되어 있지 않으므로, 아래 항목을 실제 통과했다고 표시하지 않는다. `6000.6.6f1에서 Console 오류 0건`이라는 결과도 확보하지 못했다. 실제 Unity 실행 화면은 제공하지 않는다. SpritePreview.png는 PNG 소스만 배치한 미리보기다.

## 여기서 수행한 검증

| 항목 | 결과 | 범위 |
|---|---|---|
| C# 문법 분석 | PASS | C# 23개 파일을 tree-sitter C# 문법으로 파싱, 구문 오류 0 |
| JSON 문서 | PASS | manifest와 4개 asmdef 구문 정상 |
| 어셈블리 참조 | PASS | 프로젝트 Runtime / Unity.InputSystem / UnityEngine.UI 이름 검사 |
| 입력 방식 | PASS | Legacy Input / PlayerInput 혼용 없음 |
| Editor 격리 | PASS | 런타임 소스에 UnityEditor 참조 없음 |
| WASD 매핑 | PASS | 4개 키와 2D Vector 합성 바인딩 |
| UI 입력 | PASS | 포인터/클릭/스크롤/방향/제출/취소 액션 구성 |
| Input ID | PASS | 맵/액션/바인딩 ID 중복 없음 |
| 원본 PNG | PASS | 7개 PNG 디코딩, RGBA, 지정 해상도 검사 |
| Setup 소스 경로 | PASS | 참조하는 PNG 파일 모두 존재 |
| 프리팹 생성 코드 | PASS | 5개 프리팹 생성·연결 코드 존재, 실제 생성은 Unity에서 수행 |
| 문서/설정 | PASS | 안내서와 프로젝트 설정 존재, New Input System 설정 |

세부 기계 검사 기록: `STATIC_CHECKS.json`. PNG별 검사를 개별 집계하여 총 19개다.

수동 코드 검토에서는 입력 참조 영구 저장, 재시작 직전 오브젝트 비활성화, 적 사망 중복 처리 차단, 선분 기반 고속 투사체 충돌, EXP 잉여분/연속 선택, 공격 간격 하한을 확인했다. 이 검토는 실제 런타임 테스트가 아니다.

## 제공했으나 실행하지 못한 자동 테스트

| 종류 | 케이스 수 | 상태 |
|---|---:|---|
| Unity EditMode | 17 | NOT RUN |
| Unity PlayMode | 10 | NOT RUN |
| Unity Scene/Prefab Validate 메뉴 | 1 검사 루틴 | NOT RUN |
| 정상 5분 플레이 | — | NOT RUN |
| 1600×900 / 1920×1080 화면 QA | — | NOT RUN |
| Windows x64 빌드 | — | NOT RUN |

PlayMode 테스트는 실제 씬과 컴포넌트를 사용한다. 입력 테스트에는 가상 Keyboard 장치를 사용하며, 승리 테스트는 타이머를 300초로 진행시키는 방식이다. 이는 장시간 실플레이 또는 난이도 평가와 다르다.

## 사용자 PC에서 재현

1. README의 Setup을 먼저 실행한다.
2. `Tools > Pixel Survivor > Validate Current Scene`을 실행한다.
3. `Window > General > Test Runner`의 EditMode / PlayMode에서 각각 Run All.
4. `QA_CHECKLIST.md`를 따라 실제 5분 전투와 반복 재시작을 확인한다.
5. 선택: Unity를 닫고 새 복사본에서 `Documentation/RunUnityQA.ps1`을 실행하면 Setup 및 두 테스트 모드의 XML/로그를 TestResults 폴더에 기록한다. 이 스크립트는 생성 씬·프리팹을 재작성한다.

실기 테스트가 실패하면 Console의 첫 번째 오류와 테스트 이름을 기준으로 수정해야 한다. Python 구문 검사는 Unity 어셈블리 타입 검사, API 호환성, 씬 로딩이나 프레임별 동작을 검증하지 않는다.
