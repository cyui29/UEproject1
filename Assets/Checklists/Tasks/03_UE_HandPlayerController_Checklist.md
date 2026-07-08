# Task 03 Checklist. UE_HandPlayerController

- [x] `UE_HandPlayerController` public 클래스가 생성되어 있다.
- [x] Player Transform 또는 Rigidbody 참조 슬롯이 있다.
- [x] 왼손 이동 벡터를 이동/회전으로 변환하는 로직이 있다.
- [x] 왼손 주먹 상태에서 이동이 정지된다.
- [x] 이동 가능 영역 제한값이 있다.
- [x] 리셋 시 시작 매트 위치로 복귀하는 메서드가 있다.
- [x] 참조 누락 시 null 경고 후 안전하게 멈춘다.
- [ ] Play Mode 미검증

## 5단계 적용 결과
- [x] `HandPlayroom.unity`의 `Player` GameObject에 `UE_HandPlayerController`가 배치되어 있다.
- [x] `Player_Body`, `Player_Forward_Marker`, `Main Camera`, `UE_HandTrackingGestureController`, `UE_HandPlayroomProgressFeedbackController` 참조가 연결되어 있다.
- [ ] `UE_HandPlayroomFlowController` 참조는 현재 씬에 FlowController GameObject가 없어 미연결 상태다.
- [x] Movement sensitivity tuned down for webcam hand input: slower move/rotation, larger dead zone, wider normalized input range.
- [x] Left hand movement guide overlay is added with live hand position, neutral point, and movement directions.
- [x] Left hand movement guide is placed as an editable scene UI object under UE_MainCanvas.
- [x] Left hand vertical input now controls only forward/back movement and no longer changes camera pitch.
- [x] Right hand vertical movement now controls camera pitch for up/down aiming.
- [x] Control mapping changed: open left hand moves forward/back, left fist controls look yaw/pitch.
- [x] Left fist movement suppresses player movement briefly to prevent webcam fist-detection flicker from causing drift.
