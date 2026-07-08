# Task 02. UE_HandTrackingGestureController

## 목적
웹캠과 MediaPipe Hands 결과를 받아 왼손/오른손 상태를 안정화하고, 이동/핀치/주먹/손바닥 인증 같은 핵심 제스처를 판별한다.

## 생성할 클래스
- `UE_HandTrackingGestureController`
- 위치: `Assets/Scripts/UE_HandTrackingGestureController.cs`
- 형식: `MonoBehaviour`

## 씬 배치
- `HandPlayroom` 씬 루트에 `UE_HandTrackingGestureController` GameObject를 만든다.
- MediaPipe 샘플 연동 전에는 Mock 입력으로 테스트할 수 있게 한다.

## 구현 순서
1. 왼손/오른손 상태를 담는 내부 구조를 만든다.
   - 인식 여부
   - 손 중심 좌표
   - 손바닥/주먹/핀치/손바닥 유지 상태
   - 신뢰도
2. MediaPipe Hands 결과를 받을 public 입력 메서드를 만든다.
3. `Assets/MediaPipeUnity/Samples/Scenes/Hand Landmark Detection` 샘플과 연결할 어댑터 지점을 남긴다.
4. MediaPipe 실제 연동 전 사용할 Mock 손 상태 입력 메서드를 만든다.
5. 손 중심 좌표에 스무딩을 적용한다.
6. 왼손 이동 벡터를 계산한다.
7. 오른손 핀치, 주먹, 손바닥 유지 상태를 판별한다.
8. 손 추적 손실 시 마지막 안정값을 짧게 유지한 뒤 미인식 상태로 전환한다.

## 연결 대상
- `UE_HandPlayroomFlowController`
- `UE_HandPlayerController`
- `UE_HandObjectInteractionController`
- `UE_HandPlayroomProgressFeedbackController`

## 검증 기준
- 왼손과 오른손 상태가 분리되어 있다.
- MediaPipe 원본 랜드마크를 외부 시스템에 직접 노출하지 않는다.
- Mock 입력으로 손 인식/미인식 상태를 바꿀 수 있다.
- 핀치와 주먹에는 최소 유지 시간 또는 임계값이 있다.
- Play Mode 검증 전에는 실제 웹캠/MediaPipe 동작 항목을 체크하지 않는다.
