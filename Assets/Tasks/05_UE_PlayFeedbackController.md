# Task 05. UE_PlayFeedbackController

## 대상 클래스
- `UE_PlayFeedbackController` (`MonoBehaviour`)

## UI 우선 실행 순서
- 3단계
- 웹캠과 제스처가 아직 없어도 `PlayHudPanel`에 기본 안내 UI가 보이도록 먼저 만든다.

## 목표
- HUD, 상태 텍스트, 제스처 가이드, 게이지, 반응형 비주얼을 한 클래스에서 갱신한다.

## 선행 조건
- `UE_ExperienceFlowController`
- `UE_GestureDjController`는 후속 단계에서 연결해도 된다.

## 구현 내용
- 손 인식 상태, 활성 제스처, 필터/볼륨/크로스페이더/FX 값을 표시할 UI 참조 슬롯을 만든다.
- 텍스트, 이미지, 슬라이더, 파티클, 머티리얼 등 최소 프로토타입 뷰 요소를 하나의 갱신 루프에서 다룬다.
- 음악 반응형 연출에 필요한 오디오 레벨 또는 파라미터 입력값을 받아 글로우, 파형, 입자 강도로 변환한다.
- 성능 저하 시 보조 연출을 순차적으로 줄일 수 있는 품질 단계 또는 토글을 제공한다.
- 손 미인식, 대기, 플레이, 종료 상태에 따라 안내 메시지를 다르게 출력한다.
- `UE_GestureDjController`가 아직 연결되지 않은 경우 기본 안내 UI만 표시한다.

## 작업 순서
- HUD와 비주얼에 필요한 인스펙터 참조 슬롯을 정리한다.
- 상태 텍스트와 게이지 갱신 로직을 먼저 구현한다.
- 제스처 컨트롤러 참조가 비어 있어도 기본 UI가 표시되도록 처리한다.
- 반응형 비주얼 갱신 로직을 추가하고 파라미터 연결을 맞춘다.
- 상태별 안내 문구와 성능 축소 옵션을 마무리한다.
- `SampleScene`의 `UE_AppRoot` 아래에 `UE_PlayFeedbackController` GameObject를 만들고 컴포넌트를 붙인다.
- Inspector에서 `UE_ExperienceFlowController`와 `PlayHudPanel`의 텍스트, 게이지, 비주얼 참조를 연결한다.
- `UE_GestureDjController` 참조는 후속 5단계에서 다시 연결한다.
- 씬을 저장한다.

## 완료 기준
- 현재 인식 상태와 주요 오디오 파라미터를 화면에서 확인할 수 있다.
- 사운드 변화와 시각 반응이 같은 흐름 안에서 갱신된다.
- 손 미인식과 종료 상태의 안내가 구분되어 표시된다.
- 제스처 컨트롤러가 없어도 `PlayHudPanel`에 기본 안내 UI가 표시된다.
