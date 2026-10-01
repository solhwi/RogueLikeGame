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
│   ├── CharacterModule/ 3D 캐릭터 외형(파츠 교체) 시스템 — solhwi/CharacterDemo에서 이식, 아래 섹션 참고
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

## 3D 캐릭터 외형 모듈 (CharacterModule)

아이템 장착 시 캐릭터의 3D 외형(바디/머리카락/눈/얼굴/머리장식/소지품)이 바뀌는 기능을 위해, `solhwi/CharacterDemo` 레포(UnityChan 기반 파츠 교체 시스템)에서 스크립트와 에셋을 그대로 이식했습니다.

- `Assets/Scripts/CharacterModule/` — 원본 그대로(전역 네임스페이스) 가져온 `CharacterEquipmentInventory`, `CharacterAssetComponent`, `CharacterAnimatorComponent`, `CharacterResourceSystem`, `CharacterAssetContainer`, `ItemTable`, UnityChan 스프링본 스크립트 등
- `Assets/Bundles/`, `Assets/Resources/UnityChan`, `Assets/Resources/SD Unity-Chan Haon Custom`, `Assets/AddressableAssetsData` — 원본 3D 모델/머티리얼/애니메이션/텍스처 및 Addressables 설정 (원본 `.meta` GUID를 그대로 보존해서 복사했기 때문에, `Assets/Bundles/Datas/*.asset`에 미리 구성되어 있던 아이템↔파츠 매핑이 그대로 유효합니다)
- `Assets/Scripts/Items/CharacterAppearanceAdapter.cs`, `AppearanceItemDefinition.cs`, `CharacterPreloader.cs` — 이번 프로젝트의 아이템 시스템(`ItemDefinition`/`ItemSkillBehavior`)과 위 모듈을 연결하는 새 브릿지 코드. `AppearanceItemDefinition`을 장착/해제하면 `CharacterEquipmentInventory`의 해당 슬롯(Body/Hair/Eye/Face/Accessory/Prop)을 갱신하고 `CharacterAssetComponent.Refresh()`를 호출합니다.

### 에디터에서 직접 해야 하는 작업 (자동화 불가)

이 세션은 Unity 에디터가 없는 환경이라 아래는 코드/에셋만 옮겨뒀을 뿐 직접 확인·설정이 필요합니다:

1. **패키지 해결**: 프로젝트를 열면 Package Manager가 `com.unity.addressables`(원본 버전 1.21.21 그대로 기입)를 Unity `6000.2.7f2`에 맞는 버전으로 올리라고 안내할 수 있습니다 — 안내대로 업데이트하세요.
2. **Addressables 초기화 확인**: `Window > Asset Management > Addressables > Groups`를 한 번 열어서, 복사해온 `Assets/AddressableAssetsData`의 그룹(Default Local Group 등)이 정상 인식되는지 확인하세요. 비어 보이면 에셋들이 Addressable로 마킹되어 있는지 다시 확인이 필요할 수 있습니다.
3. **씬에 캐릭터 배치**: `Assets/Bundles/Prefabs/Character.prefab`을 Player(또는 원하는 위치)의 자식으로 배치하세요. 이 프리팹은 그 자체로는 빈 Transform이고, `CharacterAssetComponent.Refresh()`가 호출되는 시점에 바디/머리/눈/얼굴 등 자식 파츠를 런타임에 생성합니다.
4. **프리로더 배치**: `CharacterPreloader` 컴포넌트를 씬의 매니저 오브젝트에 추가하고 `resourceSystem`(`Assets/Bundles/Datas/CharacterResourceSystem.asset`), `assetComponent`(위에서 배치한 Character 프리팹의 `CharacterAssetComponent`)를 연결하세요. Addressable 에셋은 미리 로드되어야 동기적으로 조회가 가능합니다.
5. **어댑터 연결**: Player에 `CharacterAppearanceAdapter`를 추가하고 `inventory`(`Assets/Bundles/Datas/CharacterEquipmentInventory.asset`), `assetComponent`를 연결하세요.
6. **외형 아이템 에셋 생성**: `Create > Roguelike > Items > Appearance Item`으로 아이템을 만들고 `slot`(Body/Hair/Eye/Face/Accessory/Prop)과 `itemCode`를 지정하세요. `itemCode`는 `Assets/Bundles/Datas/ItemTable.asset`에 이미 등록된 키와 일치해야 합니다(인스펙터에서 `partsItemDataDictionary`/`propItemDataDictionary` 확인).
7. 2D 탑다운 카메라(직교 투영)에서 3D 리그 캐릭터가 의도한 각도로 보이는지, Animator Controller(`Assets/Bundles/Animations/Character.controller`)가 RogueLikeGame의 이동 입력과 자연스럽게 맞물리는지는 시각적 확인이 필요합니다.

원본 프로젝트는 Unity `2023.2.17f1`에서 작업되었던 것이라, 처음 열 때 에셋 재임포트/업그레이드 과정이 다소 걸릴 수 있습니다.
