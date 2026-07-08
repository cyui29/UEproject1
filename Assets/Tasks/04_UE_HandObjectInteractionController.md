# Task 04. UE_HandObjectInteractionController

## 목적
오른손 조준, 핀치, 주먹 입력을 사용해 방 안 오브젝트를 선택하고 집기/놓기/파괴/버튼 강타를 처리한다.

## 생성할 클래스
- `UE_HandObjectInteractionController`
- 위치: `Assets/Scripts/UE_HandObjectInteractionController.cs`
- 형식: `MonoBehaviour`

## 씬 배치
- `HandPlayroom` 씬 루트에 `UE_HandObjectInteractionController` GameObject를 만든다.
- 상호작용 오브젝트에는 Collider와 식별용 태그 또는 직렬화 목록 등록 방식을 사용한다.

## 구현 순서
1. 중앙 타겟 기준 Raycast 설정을 만든다.
2. 상호작용 가능 레이어 또는 오브젝트 목록을 직렬화 필드로 만든다.
3. 현재 조준 오브젝트 상태를 저장한다.
4. 오른손 핀치 시작 시 잡기 가능 오브젝트를 잡는다.
5. 잡은 오브젝트를 Player 앞 약 1m 프리뷰 위치에 유지한다.
6. 핀치 해제 시 배치 가능 위치에 놓는다.
7. 오른손 주먹 상태에서 파괴 가능 오브젝트 또는 버튼형 오브젝트를 처리한다.
8. 오브젝트가 방 밖이나 바닥 아래로 떨어졌을 때 원위치 복귀 메서드를 만든다.
9. 오브젝트 완료 이벤트를 진행/피드백 시스템에 전달한다.

## 연결 대상
- `UE_HandTrackingGestureController`
- `UE_HandPlayerController`
- `UE_HandPlayroomProgressFeedbackController`

## 검증 기준
- 현재 조준 오브젝트를 외부에서 읽을 수 있다.
- 잡힌 오브젝트는 하나만 유지된다.
- 손 추적 손실 시 잡기 상태가 안전 해제된다.
- 오브젝트 완료 이벤트가 발생한다.
- Play Mode 검증 전에는 실제 Raycast/물리 동작 항목을 체크하지 않는다.
