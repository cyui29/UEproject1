# Task 05. UE_HandPlayroomProgressFeedbackController

## 목적
방 진행 상태, 별 램프 클리어 조건, 손 인식 HUD, 이동 원, 중앙 타겟, 오브젝트 근접 자막, 성공/실패 피드백을 통합 관리한다.

## 생성할 클래스
- `UE_HandPlayroomProgressFeedbackController`
- 위치: `Assets/Scripts/UE_HandPlayroomProgressFeedbackController.cs`
- 형식: `MonoBehaviour`

## 씬 배치
- `HandPlayroom` 씬 루트에 `UE_HandPlayroomProgressFeedbackController` GameObject를 만든다.
- Canvas가 없다면 `UE_MainCanvas`를 만들고 HUD 텍스트, 중앙 타겟, 이동 원, 진행 게이지 자리를 배치한다.

## 구현 순서
1. 완료 항목 enum 또는 문자열 ID 구조를 만든다.
   - 큐브 집기
   - 큐브 배치
   - 쿠키 블록 파괴
   - 버튼 강타
   - 별 램프 인증
2. 완료 항목을 기록하는 메서드를 만든다.
3. 3개 이상 완료 시 별 램프 활성화 상태를 true로 바꾼다.
4. 별 램프 인증 완료 시 클리어 이벤트를 발생시킨다.
5. 손 인식 상태, 왼손 이동 원, 중앙 타겟, 진행 게이지 HUD 참조를 직렬화 필드로 만든다.
6. 현재 근접 오브젝트에 맞는 자막 문구를 표시한다.
7. 성공/실패 피드백 문구와 짧은 표시 시간을 관리한다.
8. 리셋 시 완료 항목과 HUD를 초기화한다.

## 연결 대상
- `UE_HandPlayroomFlowController`
- `UE_HandTrackingGestureController`
- `UE_HandObjectInteractionController`
- `UE_HandPlayerController`

## 검증 기준
- 완료 항목 기록과 진행률 계산이 분리되어 있다.
- 별 램프 활성화 조건이 명확하다.
- 근접 자막 문구를 오브젝트별로 설정할 수 있다.
- UI 참조가 비어 있어도 null 경고 후 안전하게 동작한다.
- Play Mode 검증 전에는 실제 화면 표시 항목을 체크하지 않는다.
