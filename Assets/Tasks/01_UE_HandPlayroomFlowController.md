# Task 01. UE_HandPlayroomFlowController

## 목적
`Hand Play Room`의 전체 체험 상태를 관리하고, 손 인식 대기부터 플레이, 클리어, 리셋까지의 흐름을 제어한다.

## 생성할 클래스
- `UE_HandPlayroomFlowController`
- 위치: `Assets/Scripts/UE_HandPlayroomFlowController.cs`
- 형식: `MonoBehaviour`

## 씬 배치
- `Assets/Scenes/HandPlayroom.unity` 루트에 `UE_HandPlayroomFlowController` GameObject를 만든다.
- `Player`, 주요 오브젝트 루트, HUD 루트가 생기면 Inspector 참조로 연결한다.

## 구현 순서
1. 체험 상태 enum을 만든다.
   - `Boot`
   - `WaitingForHands`
   - `Playing`
   - `Cleared`
   - `Resetting`
2. 초기 상태를 `WaitingForHands`로 설정한다.
3. `EnterWaitingForHands`, `StartPlay`, `CompleteExperience`, `RequestReset` 메서드를 만든다.
4. 상태 변경 이벤트를 제공해 다른 시스템이 흐름 변화를 받을 수 있게 한다.
5. 손 인식 완료 이벤트를 받으면 `Playing`으로 전환할 수 있는 public 메서드를 만든다.
6. 클리어 이벤트를 받으면 `Cleared`로 전환한다.
7. 리셋 요청 시 진행/피드백/오브젝트/플레이어 시스템에 초기화 요청을 보낼 연결 지점을 만든다.

## 연결 대상
- `UE_HandTrackingGestureController`
- `UE_HandPlayerController`
- `UE_HandObjectInteractionController`
- `UE_HandPlayroomProgressFeedbackController`
- `UE_HandPlayroomOperatorController`

## 검증 기준
- 손 인식 대기, 플레이, 클리어, 리셋 상태가 코드상 명확히 구분된다.
- 상태 전환 메서드가 Inspector 버튼 또는 다른 시스템에서 호출 가능한 형태다.
- 리셋 시 다른 시스템에 초기화 요청을 전달할 수 있다.
- Play Mode 검증 전에는 실제 전환 확인 항목을 체크하지 않는다.
