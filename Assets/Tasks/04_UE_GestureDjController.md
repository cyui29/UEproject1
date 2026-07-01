# Task 04. UE_GestureDjController

## 대상 클래스
- `UE_GestureDjController` (`MonoBehaviour`)

## UI 우선 실행 순서
- 5단계
- 웹캠 입력과 기본 HUD가 준비된 뒤 제스처 결과를 오디오와 UI에 연결한다.

## 목표
- 손 데이터를 해석해 제스처를 판별하고 DJ 파라미터를 안전 범위 안에서 적용한다.

## 선행 조건
- `UE_MusicSelectionController`
- `UE_WebcamTrackingController`

## 구현 내용
- 오른손 상하, 왼손 상하, 좌우 이동, 양손 거리, 손목 회전, 주먹 쥐기/펴기 입력을 판별할 규칙을 내부에 정의한다.
- 연속형 제스처와 이벤트형 제스처를 분리해 처리하고, 우선순위 충돌 시 단일 결과를 선택한다.
- 필터, 레이어 양, 크로스페이더, FX, 원샷 트리거를 오디오 재생 시스템에 반영하는 매핑 로직을 구현한다.
- 추적 손실 또는 불안정 입력 시 파라미터를 급격히 튀지 않게 완만히 복귀시킨다.
- 현재 인식 중인 제스처와 핵심 파라미터 값을 피드백 시스템이 읽을 수 있게 노출한다.

## 작업 순서
- 손 데이터 입력 연결과 제스처 판별 기준값을 먼저 정의한다.
- 파라미터 매핑과 안전 범위 제한을 구현한다.
- 이벤트형 제스처 트리거와 연속형 제스처 갱신 루프를 분리한다.
- 피드백 시스템으로 넘길 현재 상태 접근자를 정리한다.
- `SampleScene`의 `UE_AppRoot` 아래에 `UE_GestureDjController` GameObject를 만들고 컴포넌트를 붙인다.
- Inspector에서 `UE_MusicSelectionController`, `UE_WebcamTrackingController` 참조를 연결한다.
- 이미 배치된 `UE_PlayFeedbackController`에 `UE_GestureDjController` 참조를 다시 연결한다.
- 씬을 저장한다.

## 완료 기준
- 같은 입력에서 같은 제스처 결과가 반복 가능하게 나온다.
- 잘못된 입력이나 추적 흔들림이 있어도 사운드 파라미터가 과격하게 깨지지 않는다.
- `UE_PlayFeedbackController`가 제스처 컨트롤러 상태를 읽을 수 있다.
