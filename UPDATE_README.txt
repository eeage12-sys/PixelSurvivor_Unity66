UI 재배치 업데이트 — 2026-11-04

적용 순서 (기존 프로젝트)
1. Unity 종료.
2. ZIP을 기존 프로젝트와 다른 새 폴더에 압축 해제.
   예: 다운로드/새업데이트/PixelSurvivor_Unity66
3. 새 폴더 안의 Assets만 복사.
4. 기존 다운로드/PixelSurvivor_Unity66 폴더에 붙여넣고 같은 파일 덮어쓰기.
   Assets, Packages, ProjectSettings가 함께 보이는 위치에 붙여넣습니다.
   Assets 안에 Assets를 넣지 마세요. 기존 Assets를 삭제하지 마세요.
5. Unity에서 기존 MainScene 열기. 임포트/컴파일이 끝날 때까지 기다리기.
6. Tools > Pixel Survivor > UI 업데이트 적용 (Apply HUD Update) 클릭.
7. 씬 저장 후 Play. 메뉴에서 시작 > 캐릭터 > 스테이지 > 전투 시작.

새 프로젝트는 Tools > Pixel Survivor > Setup - Rebuild Game Scene으로 생성합니다.
기존 프로젝트에는 Rebuild 대신 UI 업데이트 적용을 사용합니다.
이전 한국어 적용 / Apply Infinite Survival Update 메뉴도 같은 업데이트를 적용합니다.

변경점
- 상단 전체 너비의 경험치 바 + 내부 LV 숫자.
- 시간은 상단 가운데 한 개, 이름 없이 숫자만 표시.
- 체력바는 캐릭터 발밑. 기존 좌측 체력바/하단 EXP/큰 장비 HUD는 숨김.
- 왼쪽 위 공격형 7칸, 그 아래 강화형 7칸: 작은 아이콘만 표시.
- 오른쪽 위 처치/획득 골드: 해골/동전 아이콘과 숫자.
- ESC: 캐릭터 정보, 장비 이름과 레벨 칸, 옵션, 조합법, 계속하기.
- 밝은 칸은 현재 장비 레벨, 어두운 칸은 남은 레벨. 진화 장비도 최대 칸 표시.
- ESC를 다시 누르면 계속하기. 옵션/조합법에서 ESC는 정보창으로 돌아감.
- 레벨업/상자/결과 창은 ESC로 건너뛰지 않음.
- 옵션: 사물 피격 효과, 발밑 체력바, Windows 실행 파일 전체 화면 전환.
- 사물 피격 시 반짝임/크기 흔들림/불꽃, 파괴 시 파편.
- 실행 전 빈 메뉴의 제목/안내/버튼 글자 복구. 실행 전은 미리보기 안내 화면.

검증
정적 검사 214개 통과. Unity 컴파일/Play/Windows 빌드는 이 환경에서 실행하지 못했습니다.
HudUpdateTests 7개를 추가했으며 Unity Test Runner에서 실행할 수 있습니다.
상세 수동 체크는 Documentation/HUD_UPDATE.md를 참고하세요.
