# Task 04 Checklist. UE_HandObjectInteractionController

- [x] `UE_HandObjectInteractionController` public 클래스가 생성되어 있다.
- [x] 중앙 타겟 기준 Raycast 또는 동등한 조준 감지 로직이 있다.
- [x] 상호작용 가능 오브젝트 목록 또는 레이어 설정이 있다.
- [x] 핀치로 오브젝트를 집고 해제할 수 있는 상태 구조가 있다.
- [x] 잡힌 오브젝트는 하나만 유지된다.
- [x] 주먹 파괴/버튼 강타 처리 지점이 있다.
- [x] 오브젝트 안전 복귀 메서드가 있다.
- [x] 완료 이벤트를 외부로 전달할 수 있다.
- [ ] Play Mode 미검증

## 구현 기록
- `Assets/Scripts/UE_HandObjectInteractionController.cs`를 추가했다.
- `HandPlayroom` 씬의 `Systems` 아래에 `UE_HandObjectInteractionController` GameObject를 추가했다.
- `UE_HandTrackingGestureController`, `UE_HandPlayerController`, `UE_HandPlayroomProgressFeedbackController`, `Main Camera`, `Player_Hand_Interaction_Target` 참조를 연결했다.
- `EnergyCube_A/B`, `Socket_A/B`, `ArchiveDial`, `CoreCrack_A/B/C`, `FinalPalmPanel`을 상호작용 목록으로 연결했다.
- Play Mode Raycast/물리 확인은 아직 수행하지 않았다.
- [x] Interactable objects receive a light green outline guide at runtime.
- [x] Right hand interaction guide overlay is added as an editable scene UI object under UE_MainCanvas.
- [x] Right hand movement now moves the held object while pinched/grabbed instead of controlling camera look.
- [x] Grabbable object outline turns red when the object is aimed and can be grabbed by right hand pinch.
- [x] Right hand guide displays the active gesture state, including PINCH.
- [x] Pinch grab now retries while pinching so aimed grabbable objects lift immediately, with pinch hold reduced to 0.03 seconds.
- [x] Held objects stay in front of the player camera while pinched, even when the player moves or looks around.
- [x] Held objects are clamped near the center of the camera view and ignore right-hand object movement briefly after grab to prevent dropping out of view.
- [x] Aimed grabbable objects show a red outline with `GRAB`, and aimed breakable objects show a red outline with `BREAK` on an action prompt outside ObjectivePanel.
- [x] Releasing right-hand pinch immediately drops the held object, while matching sockets still complete placement.
- [x] Grabbable objects get runtime Rigidbody physics and a runtime floor collider so released objects fall onto the room floor instead of passing through it.
