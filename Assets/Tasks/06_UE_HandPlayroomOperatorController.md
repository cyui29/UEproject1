# Task 06. UE_HandPlayroomOperatorController

## 목적
운영자용 Old Input 키를 통해 방 리셋, 디버그 표시, 진행 상태 강제 변경을 처리한다.

## 생성할 클래스
- `UE_HandPlayroomOperatorController`
- 위치: `Assets/Scripts/UE_HandPlayroomOperatorController.cs`
- 형식: `MonoBehaviour`

## 씬 배치
- `HandPlayroom` 씬 루트에 `UE_HandPlayroomOperatorController` GameObject를 만든다.
- 앞선 5개 시스템 참조를 Inspector로 연결한다.

## 구현 순서
1. 운영자 입력 키를 직렬화 필드로 만든다.
   - 리셋: `R`
   - 힌트 토글: `H`
   - 디버그 토글: `D`
   - 클리어 조건 강제 충족: `C`
2. `Update`에서 Unity Old Input 방식으로 키 입력을 확인한다.
3. 리셋 키 입력 시 Flow 시스템에 리셋을 요청한다.
4. 힌트 토글 시 Progress/Feedback 시스템에 힌트 표시 상태를 전달한다.
5. 디버그 토글 시 추적/제스처 상태 표시를 켜고 끌 수 있는 지점을 만든다.
6. 클리어 조건 강제 충족 시 Progress/Feedback 시스템의 완료 조건을 충족시킨다.
7. 운영자 입력이 플레이어 손동작 입력과 충돌하지 않도록 독립 처리한다.

## 연결 대상
- `UE_HandPlayroomFlowController`
- `UE_HandTrackingGestureController`
- `UE_HandPlayerController`
- `UE_HandObjectInteractionController`
- `UE_HandPlayroomProgressFeedbackController`

## 검증 기준
- New Input System을 사용하지 않는다.
- 리셋, 힌트, 디버그, 강제 클리어 입력이 분리되어 있다.
- 참조 누락 시 Console 경고를 남기고 안전하게 무시한다.
- 운영자 입력은 플레이어 손 제스처 상태를 직접 덮어쓰지 않는다.
- Play Mode 검증 전에는 실제 키 입력 동작 항목을 체크하지 않는다.
