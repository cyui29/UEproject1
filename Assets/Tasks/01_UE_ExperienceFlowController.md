# Task 01. UE_ExperienceFlowController

## 대상 클래스
- `UE_ExperienceFlowController` (`MonoBehaviour`)

## UI 우선 실행 순서
- 1단계
- 0단계에서 `SampleScene`에 `UE_AppRoot`, `UE_MainCanvas`, `TitlePanel`, `MusicSelectionPanel`, `PlayHudPanel`을 먼저 만든 뒤 실행한다.

## 목표
- 프로토타입 세션의 시작, 음악 선택 진입, 플레이 전환, 종료, 재시작 흐름을 한 클래스에서 안정적으로 관리한다.

## 선행 조건
- 없음

## 구현 내용
- 타이틀, 음악 선택, 대기, 플레이, 종료 상태를 표현할 최소 상태 enum과 현재 상태 보관 로직을 만든다.
- 선택한 음악 인덱스, 현재 세션 진행 여부, 재시작 요청 여부 등 프로토타입 세션 데이터를 내부 필드로 관리한다.
- 다른 시스템이 접근할 수 있도록 상태 전환 메서드와 읽기 전용 상태 접근자를 제공한다.
- 종료 또는 재시작 시 세션 데이터를 초기화하고 다음 진입 상태를 일관되게 복구한다.

## 작업 순서
- `Assets/Scripts`에 흐름 제어 클래스를 생성한다.
- 프로토타입에 필요한 최소 상태 정의와 상태 전환 API를 만든다.
- 세션 데이터 캐시 필드를 추가하고 상태 전환 시점에 초기화 규칙을 연결한다.
- 디버그 로그 또는 이벤트 훅으로 후속 시스템 연결 지점을 남긴다.
- `SampleScene`의 `UE_AppRoot` 아래에 `UE_ExperienceFlowController` GameObject를 만들고 컴포넌트를 붙인다.
- Inspector에서 초기 상태와 디버그 옵션 기본값을 확인한다.
- 씬을 저장한다.

## 완료 기준
- 한 세션 안에서 시작, 선택, 플레이, 종료 흐름이 끊기지 않는다.
- 재시작 또는 종료 후 이전 세션 선택 정보가 의도치 않게 남지 않는다.
- `SampleScene` Hierarchy에서 `UE_ExperienceFlowController` GameObject를 확인할 수 있다.
