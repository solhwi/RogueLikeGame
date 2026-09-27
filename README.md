# RogueLikeGame

Unity 2D 로그라이크(뱀서라이크) 프로젝트입니다. 웨이브 단위로 몰려오는 적을 자동 타겟팅 아이템 스킬로 처치하고, 경험치를 모아 레벨업하며 새 아이템을 얻어 스킬 슬롯을 채워나가는 방식의 게임을 목표로 합니다. 아이템이 곧 스킬이며(장착 시 자동 발동), 별도의 액티브/패시브 구분 없이 하나의 `ItemDefinition`/`ItemSkillBehavior` 구조로 통일되어 있습니다.

## 개발 환경

- Unity `6000.2.7f2`
- Universal Render Pipeline (URP)
- Input System
- 2D Tilemap / Sprite

## 프로젝트 구조

```
Assets/
├── Art/          스프라이트, 타일, 머티리얼
├── Data/         ScriptableObject 데이터 (챕터, 웨이브, 적, 스킬)
├── Prefabs/      플레이어, 적, 투사체 등 프리팹
├── Scenes/       게임 씬
├── Scripts/
│   ├── CameraSystem/  카메라 추적
│   ├── Combat/        투사체, 데미지, 스탯 타입
│   ├── Core/          게임 매니저, 싱글톤
│   ├── Editor/        에디터 전용 툴
│   ├── Enemies/       적 AI, 정의 데이터
│   ├── Items/         아이템(=스킬) 정의/행동, 아이템 인벤토리·장착 슬롯, 장비/장비 융합
│   ├── Level/         챕터/웨이브 스포너, 무한 타일맵
│   ├── Managers/      오디오, 경험치, 킬 카운트, 메타 진행, 런 관리
│   ├── Player/        플레이어 컨트롤, 경험치 젬, 루트 마그넷
│   ├── UI/            HUD, 레벨업 선택, 이동/일시정지 UI
│   └── Utility/       오브젝트 풀 등 공용 유틸
└── Settings/     URP 렌더 파이프라인 설정
```

## 실행 방법

1. Unity Hub에서 `6000.2.7f2` 이상 버전으로 프로젝트를 엽니다.
2. `Assets/Scenes/RogueLikeGameScene.unity` 씬을 열고 재생합니다.
