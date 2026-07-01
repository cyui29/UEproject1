# Task 03. UE_WebcamTrackingController

## 대상 클래스
- `UE_WebcamTrackingController` (`MonoBehaviour`)

## UI 우선 실행 순서
- 4단계
- 타이틀, 음악 선택 UI, 기본 HUD 틀이 씬에 배치된 뒤 손 추적 입력을 연결한다.

## 목표
- 웹캠 시작과 프레임 공급, `MediaPipe Hands` 연동, 손 데이터 안정화까지 한 클래스에서 처리한다.

## 선행 조건
- `UE_ExperienceFlowController`

## 구현 내용
- 웹캠 장치 선택, 시작, 정지, 실패 상태 노출 로직을 구현한다.
- 최신 프레임을 보관하고 외부 추적 모듈 또는 브리지 호출 지점에 전달할 접근자를 만든다.
- 양손 랜드마크 결과를 내부 직렬화 구조로 정리하고 좌우 손 상태를 분리 보관한다.
- 좌표 정규화, 좌우 반전 보정, 민감도, 스무딩, 신뢰도 하락 시 마지막 안정값 유지 규칙을 포함한다.
- 장치 미존재나 추적 실패 시 플레이 흐름에 미인식 상태를 전달할 수 있게 한다.

## 작업 순서
- 웹캠 라이프사이클과 상태값부터 구현한다.
- 프레임 접근 API와 추적 결과 입력 지점을 만든다.
- MediaPipe Hands 연결 전에는 Mock 손 입력으로 좌우 손 상태 갱신을 먼저 확인한다.
- MediaPipe Hands 연결 시 21개 랜드마크를 내부 추적 결과 입력 메서드로 전달한다.
- 손 데이터 보관 구조와 보정 규칙을 연결한다.
- MediaPipe 원본 랜드마크 타입은 외부 시스템에 직접 노출하지 않는다.
- 실패 상태와 미인식 상태를 흐름 시스템에 전달하는 훅을 추가한다.
- `SampleScene`의 `UE_AppRoot` 아래에 `UE_WebcamTrackingController` GameObject를 만들고 컴포넌트를 붙인다.
- Inspector에서 `UE_ExperienceFlowController`와 웹캠/보정 기본값을 연결한다.
- 씬을 저장한다.

## 완료 기준
- 웹캠 시작과 정지가 가능하다.
- MediaPipe Hands가 없어도 Mock 입력으로 후속 제스처 시스템 연결을 준비할 수 있다.
- 최신 프레임과 안정화된 손 상태를 후속 제스처 시스템에 전달할 수 있다.
- 장치 또는 추적 실패 시 앱이 멈추지 않고 상태가 노출된다.
- `SampleScene` Hierarchy에서 `UE_WebcamTrackingController` GameObject를 확인할 수 있다.
