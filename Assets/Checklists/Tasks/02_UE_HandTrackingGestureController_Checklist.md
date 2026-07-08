# Task 02 Checklist. UE_HandTrackingGestureController

- [x] `UE_HandTrackingGestureController` public 클래스가 생성되어 있다.
- [x] 왼손/오른손 상태가 분리되어 있다.
- [x] MediaPipe Hands 결과를 받을 입력 지점이 있다.
- [x] Mock 손 입력 또는 임시 추적 결과 입력 지점이 있다.
- [x] 손 중심 좌표 스무딩 규칙이 있다.
- [x] 핀치, 주먹, 손바닥 유지 판정 임계값이 있다.
- [x] MediaPipe 원본 랜드마크를 외부 시스템에 직접 노출하지 않는다.
- [x] 손 미인식 상태를 외부에 알릴 수 있다.
- [ ] Play Mode 미검증

## 4단계 의존성 제외
- 실제 MediaPipe 샘플/웹캠 연동은 이후 Unity Editor 및 장비 검증 단계에서 확인한다.
- `HandPlayroom.unity` GameObject 배치와 Inspector 참조 연결은 씬 편집 의존 작업으로 이번 실행에서 제외했다.

## 4.5단계 MediaPipe 연결
- [x] `UE_MediaPipeHandTrackingAdapter` public 클래스가 생성되어 있다.
- [x] `HandLandmarkerResult`의 handedness와 normalized landmarks를 게임용 손 상태로 변환한다.
- [x] MediaPipe 결과가 `UE_HandTrackingGestureController.SubmitMediaPipeHandState(...)`로 전달된다.
- [x] `HandLandmarkerRunner`의 IMAGE, VIDEO, LIVE_STREAM 결과 경로에서 어댑터 호출 지점이 있다.
- [x] 원본 MediaPipe landmark 리스트를 Player, Interaction, HUD 시스템에 직접 넘기지 않는다.
- [x] 좌우 손 반전 보정 옵션이 있다.
- [x] `HandPlayroom.unity`에 `UE_HandTrackingGestureController`와 `UE_MediaPipeHandTrackingAdapter` GameObject가 배치되어 있다.
- [x] `UE_MediaPipeHandTrackingAdapter`의 `handTrackingController` 참조가 연결되어 있다.
- [x] `UE_HandTrackingGestureController`의 `progressFeedbackController` 참조가 연결되어 있다.
- [ ] 실제 웹캠/MediaPipe Play Mode 검증

- [x] Game View Annotation Layer visibility is corrected by `UE_AnnotationLayerGameViewFitter`.
- [x] Game View hand landmarks are drawn directly by `UE_HandLandmarkGameViewOverlay`.
- [x] Pinch detection distance increased from 0.06 to 0.09 for easier webcam pinching.
- [x] Pinch detection now uses thumb-index distance normalized by hand scale to avoid false grabs when the hand moves farther from the camera.
