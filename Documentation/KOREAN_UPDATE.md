# 한국어판 업데이트 — 2026-11-04

## 적용
Unity를 종료한 뒤 새 Assets 폴더 전체를 기존 프로젝트의 Assets와 합쳐 덮어씁니다.
기존 Assets / .meta / Scenes / Prefabs / Data를 먼저 삭제하지 마세요.
Unity에서 MainScene을 열고 **Tools > Pixel Survivor > 한국어 적용 (Apply Korean)**을 실행합니다.
기존 Apply Infinite Survival Update 메뉴도 동일한 한국어 적용을 포함합니다.
새 프로젝트는 Setup - Rebuild Game Scene으로 시작합니다.

## 변경 범위
- 시작/캐릭터/스테이지/상점/장비 카드/HUD/상자/결과/일시정지 화면을 한국어로 변경.
- 공격형 8종, 진화 무기 8종, 강화형 8종의 기본 이름과 설명 번역.
- 캐릭터 5명, 스테이지 3개, 공통/캐릭터별 영구 강화 이름 번역.
- 진화 조건, 시너지, 보스/해금/아이템/지원군 알림 번역.
- 상점 효과를 백분율 또는 실제 수치로 표시. 쿨다운 강화는 감소 부호로 표시.
- 기존 영어 기본 데이터는 적용 시 번역. 저장 ID, 수치, 프리팹 참조는 바꾸지 않음.
- 직접 수정한 사용자 정의 이름/설명은 일치하는 기본 문구가 아니면 유지.
- Resources/Fonts/NotoSansCJKkr-Regular.otf 포함. SIL OFL 라이선스와 출처 동봉.
- 동적 폰트와 Include Font Data 설정을 적용하여 PC의 설치 폰트에 의존하지 않음.
- 긴 문구에 제한된 자동 글자 크기 조절 적용; 장비 설명 영역 조정.

## 검사 결과
C# 구문/JSON/원본 PNG 검사 및 한글 폰트 관련 정적 검사 통과.
현대 한글 완성형 11,172자를 모두 포함하며, 소스에서 사용하는 한글의 누락 글리프 없음.
KoreanTextPreview.png는 포함 폰트로 렌더링한 문구 검토용 이미지이며 Unity 실행 캡처가 아님.
문구를 해당 폭으로 줄바꿈한 샘플에서 글자 표시를 시각적으로 확인함.
Unity Editor에서 실제 컴파일, uGUI 레이아웃, Windows 빌드 검증은 미실행.
따라서 실제 게임의 줄바꿈/화면 배치 최종 확인은 Unity에서 필요함.

## 개발 파일
Scripts/UI/KoreanText.cs: 기본 데이터 번역, 한글 폰트 로드, 능력치 표시.
Scripts/UI/CampaignUI.cs: 한글 화면 구성.
Editor/KoreanSetup.cs: 한국어 적용 메뉴, 폰트 임포트 및 기존 HUD 갱신.
Documentation/KOREAN_TRANSLATIONS.json: 번역 대응표.

Unity 상단 메뉴 전체나 Inspector의 C# 필드명까지 번역하는 기능은 아님.
플레이어가 보는 게임 화면을 한국어로 제공하며, 개발용 진단/객체명 일부는 영어 유지.

공식 폰트 임포트 API 참고:
https://docs.unity3d.com/6000.0/Documentation/ScriptReference/TrueTypeFontImporter.html
