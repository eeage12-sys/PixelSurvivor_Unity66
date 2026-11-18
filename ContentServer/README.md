# Pixel Survivor 콘텐츠 서버

이 서버는 제출 시 HTTPS 콘텐츠 제공과 실패·복구 시연에 사용합니다. 기본 게임은 서버 없이 실행됩니다.

## 준비 및 실행

1. .NET 10 SDK를 설치합니다.
2. 개발 PC에서 `dotnet dev-certs https --trust`로 로컬 개발 인증서를 준비합니다. 인증서와 개인키는 제출물에 넣지 않습니다.
3. `ContentServer` 폴더에서 `dotnet run`을 실행합니다.
4. 브라우저에서 `https://localhost:7188/health`와 `https://localhost:7188/content/game-config.json`을 확인합니다.

## 실패·복구 재현

서버를 끈 상태에서 게임의 **콘텐츠 확인**을 누르면 기본 설정으로 계속 플레이하고 실패 안내가 표시됩니다. 서버를 다시 켜고 **다시 시도**를 누르면 JSON을 검증한 뒤 소리 크기를 적용합니다. 설정 파일을 잘못된 JSON 또는 `soundVolume` 범위 밖 값으로 바꾼 경우에도 기본 설정이 유지됩니다. 실제 실행 결과는 별도 확인 기록에 적습니다.

서버의 `bin`과 `obj`, 개발 인증서와 개인키는 제출에서 제외합니다.
