# Task 06. UE_OperatorControlController

## 대상 클래스
- `UE_OperatorControlController` (`MonoBehaviour`)

## UI 우선 실행 순서
- 6단계
- 앞선 5개 시스템 GameObject가 `SampleScene`에 모두 배치된 뒤 마지막으로 연결한다.

## 목표
- Old Input 기반 운영자 입력, 디버그 제어, 전역 튜닝값 참조, 공통 시스템 연결을 마무리하는 최종 제어 클래스를 만든다.

## 선행 조건
- `UE_ExperienceFlowController`
- `UE_MusicSelectionController`
- `UE_WebcamTrackingController`
- `UE_GestureDjController`
- `UE_PlayFeedbackController`

## 구현 내용
- Old Input 기준 재시작, 강제 종료, 디버그 토글, 음악 선택 보조 입력을 처리한다.
- 전역 시간, 민감도, UI/오디오 기본값 등 자주 조정되는 튜닝값을 인스펙터 직렬화 필드로 제공한다.
- 앞선 5개 시스템 참조를 연결하고 null 검사, 초기 연결 검증, 경고 로그를 담당한다.
- 운영자 명령이 플레이어 손 제스처 입력과 충돌하지 않도록 실행 조건을 분리한다.
- 프로토타입 운영 시 빠르게 상태를 재설정할 수 있는 관리 API를 제공한다.

## 작업 순서
- 운영자 입력 키맵과 디버그 토글부터 정의한다.
- 주요 시스템 참조 슬롯과 연결 검증 로직을 만든다.
- 튜닝값 직렬화 필드와 적용 지점을 연결한다.
- 재시작, 강제 종료, 디버그 입력을 흐름 제어 시스템과 연결한다.
- `SampleScene`의 `UE_AppRoot` 아래에 `UE_OperatorControlController` GameObject를 만들고 컴포넌트를 붙인다.
- Inspector에서 `UE_ExperienceFlowController`, `UE_MusicSelectionController`, `UE_WebcamTrackingController`, `UE_GestureDjController`, `UE_PlayFeedbackController` 참조를 모두 연결한다.
- 씬을 저장한다.

## 완료 기준
- 운영자 입력으로 재시작과 강제 종료를 제어할 수 있다.
- 주요 시스템 참조 누락 시 바로 확인 가능한 경고가 나온다.
- 플레이어 입력과 운영자 입력이 같은 경로에서 섞여 오동작하지 않는다.
- `SampleScene` Hierarchy에서 6개 `UE_` 시스템 GameObject가 모두 연결된 상태를 확인할 수 있다.
