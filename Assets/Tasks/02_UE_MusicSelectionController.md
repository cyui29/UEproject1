# Task 02. UE_MusicSelectionController

## 대상 클래스
- `UE_MusicSelectionController` (`MonoBehaviour`)

## UI 우선 실행 순서
- 2단계
- `MusicSelectionPanel`에 음악 카드 UI 자리 2~3개를 먼저 만든 뒤 컨트롤러를 연결한다.

## 목표
- 음악 카드 UI, 선택 결과 반영, 트랙 메타데이터 보관, 기본 오디오 재생 요청을 한 클래스에서 처리한다.

## 선행 조건
- `UE_ExperienceFlowController`

## 구현 내용
- 2~3개의 음악 카드 정보를 인스펙터에서 설정 가능한 직렬화 구조로 정의한다.
- 카드 선택 입력을 받아 현재 선택 상태를 갱신하고 `UE_ExperienceFlowController`에 반영한다.
- 선택된 트랙의 `AudioClip`, 제목, 루프 옵션 등 메타데이터를 내부에서 관리한다.
- 플레이 시작 요청 시 기본 비트 또는 선택 트랙을 로드하고 재생할 `AudioSource` 연결 구조를 만든다.
- 필수 클립 누락 시 대체 가능한 기본 트랙 또는 재생 차단 상태를 명확히 처리한다.

## 작업 순서
- 음악 카드 데이터와 `AudioSource` 참조 슬롯을 설계한다.
- 카드 선택 및 강조 표시 갱신 메서드를 구현한다.
- 선택 결과를 흐름 제어 클래스에 전달하는 연결 코드를 추가한다.
- 트랙 로드, 재생, 정지, 초기화 API를 구현한다.
- `SampleScene`의 `UE_AppRoot` 아래에 `UE_MusicSelectionController` GameObject를 만들고 컴포넌트를 붙인다.
- Inspector에서 `UE_ExperienceFlowController`, `AudioSource`, 음악 카드 데이터, 선택 표시 오브젝트를 연결한다.
- 씬을 저장한다.

## 완료 기준
- 한 번에 하나의 음악만 선택된다.
- 선택한 음악 정보가 플레이 시작 시점까지 유지된다.
- 오디오 클립 누락 상황에서도 안전한 기본 처리 경로가 있다.
- `MusicSelectionPanel`에 음악 카드 UI가 보이고 컨트롤러에서 참조할 수 있다.
