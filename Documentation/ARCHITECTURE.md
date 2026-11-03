# 구현 구조

- 목표 엔진: Unity 6000.6.6f1. Built-In 렌더 파이프라인.
- 입력: `InputActionReference` 단일 방식. Gameplay/Move와 UI 맵 분리. Legacy Input API / PlayerInput 미사용.
- 상태: `Menu → Playing → ChoosingUpgrade → Playing → GameOver 또는 Victory`.
- `GameManager`가 참조를 소유하고 초기화하며 전역 Singleton은 사용하지 않는다.
- `GameTimer`만 전투 시간을 누적한다. 상태 검사와 `Time.timeScale=0`을 함께 사용해 UI가 열린 동안 이동·피해·드롭·시간을 막는다.
- 재시작 시 RuntimeObjects의 자식을 먼저 비활성화한 뒤 제거한다. 같은 프레임에 이전 적/투사체가 피해를 주지 못한다.
- 적 목록은 EnemySpawner가 관리한다. 매 프레임 FindObjects를 사용하지 않는다.
- 자동 공격은 화면 안 + Range 안에서 거리 제곱으로 가장 가까운 적을 찾는다.
- 투사체는 이동 선분과 적의 원형 판정을 교차 검사한다. 가장 앞선 충돌 한 번만 처리하고 사라진다. Collider2D/Rigidbody2D 없이 동작한다.
- 적 접촉과 EXP 습득은 원형 거리 판정이다. 최대 120적에 한해 O(N²) 약한 밀집 분산을 사용한다. 실기 성능은 아직 측정하지 않았다.
- 화면 밖 생성은 카메라 실제 크기·종횡비를 사용한다. 맵 가장자리에서는 적이 맵 밖에서 안으로 들어올 수 있다. 보석 드롭 위치는 맵 안으로 제한한다.
- 무기 쿨타임은 게임 재개 후 이어진다. 생성된 투사체는 발사 시점 능력치를 보존하고 이후 투사체부터 강화 수치가 반영된다.
- EXP 잉여분은 유지한다. 한 번에 여러 레벨을 올리면 PendingChoices로 강화 선택을 각각 제공한다.
- 강화 후보는 Fisher–Yates로 섞어 3개를 뽑는다. 공격 간격 하한 도달 시 무효한 공격속도 후보를 제거한다.
- SpriteRenderer와 논리 컴포넌트를 분리했다. PNG 교체로 공격/HP/이동 코드를 수정할 필요가 없다.
- Setup이 씬, 5개 프리팹, InputActionReference와 UI 참조를 생성한다. 원본 소스와 원본 PNG는 변경하지 않는다.

# 의도적으로 제한한 범위

풀링, 복수 무기, 추가 적, 영구 저장, 사운드, 수동 일시정지, 게임패드는 구현하지 않았다. 16:9 PC 화면을 기준으로 하며 초광각·모바일 UI는 검증 대상 밖이다. 카메라는 회전하지 않고 플레이어를 추적한다. Point 필터를 쓰지만 별도의 Pixel Perfect Camera 패키지는 사용하지 않는다.

# 확인한 공식 자료

- InputActionReference: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.18/api/UnityEngine.InputSystem.InputActionReference.html
- Input System UI module: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.18/api/UnityEngine.InputSystem.UI.InputSystemUIInputModule.html
- uGUI assembly source: https://github.com/Unity-Technologies/uGUI/blob/main/com.unity.ugui/Runtime/UGUI/UnityEngine.UI.asmdef
- Unity 6.6 assembly definitions: https://docs.unity.com/en-us/engine/6000.6/manual/packages-list/cus-pkg-lp/cus-pkg-development/cus-asmdef

공식 API 확인은 Unity 컴파일/실행 검증을 대체하지 않는다.
