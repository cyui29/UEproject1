# Task 03. UE_HandPlayerController

## 목적
왼손 입력을 사용해 `Player` 오브젝트를 단일 방 안에서 이동시키고, 필요 시 시선 방향을 조절한다.

## 생성할 클래스
- `UE_HandPlayerController`
- 위치: `Assets/Scripts/UE_HandPlayerController.cs`
- 형식: `MonoBehaviour`

## 씬 배치
- `HandPlayroom` 씬의 `Player` GameObject에 컴포넌트를 붙인다.
- `Player_Body`, `Player_Forward_Marker`, `Main Camera` 또는 카메라 Rig 참조를 Inspector에 연결한다.

## 구현 순서
1. 이동 속도, 회전 속도, 시선 상하 민감도를 직렬화 필드로 만든다.
2. 이동 가능 영역의 중심과 크기를 직렬화 필드로 만든다.
3. `UE_HandTrackingGestureController`의 왼손 이동 벡터를 참조한다.
4. 왼손 주먹 상태일 때 이동을 정지한다.
5. 왼손 X축 입력을 좌우 이동 또는 회전으로 변환한다.
6. 왼손 Y축 입력을 전진/후진 또는 시선 상하 조절로 변환한다.
7. 이동 결과가 방 밖으로 나가지 않도록 위치를 제한한다.
8. 리셋 시 시작 매트 위치로 복귀하는 메서드를 만든다.

## 연결 대상
- `UE_HandTrackingGestureController`
- `UE_HandPlayroomFlowController`
- `UE_HandPlayroomProgressFeedbackController`

## 검증 기준
- Player 참조 없이도 null 경고 후 멈춘다.
- 왼손 입력이 없으면 이동하지 않는다.
- 주먹 상태에서는 이동이 정지된다.
- 리셋 메서드가 시작 위치로 복귀시킨다.
- Play Mode 검증 전에는 실제 이동 확인 항목을 체크하지 않는다.
