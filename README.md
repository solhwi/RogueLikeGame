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

## 3D 캐릭터 외형 모듈 (CharacterModule) + 2D처럼 보이게 하는 렌더링 (CharacterRendering)

아이템 장착 시 캐릭터의 3D 외형(바디/머리카락/눈/얼굴/머리장식/소지품)이 바뀌는 기능을 위해 두 레포에서 가져왔습니다:

- `solhwi/CharacterDemo` — UnityChan 기반 3D 파츠 교체 시스템 (실제 3D 리그/모델)
- `solhwi/SebamoGameClient` — 그 3D 결과물을 오프스크린 카메라로 RenderTexture에 찍어서, 2D 씬의 평면 쿼드(Quad)에 입히는 "3D지만 2D처럼 보이게" 하는 렌더링 시스템

둘을 합치면: 실제로는 리그드 3D 모델이 전투/장면 바깥의 먼 좌표에서 애니메이션하고 있고, 플레이어 눈에 보이는 건 그 모습을 매 프레임 캡처해 붙인 평면 쿼드입니다. 그래서 2D 탑다운 게임 안에서도 자연스럽고, 라이팅/카메라 각도도 자유롭게 조절할 수 있습니다.

- `Assets/Scripts/CharacterModule/` — `solhwi/CharacterDemo`에서 원본 그대로(전역 네임스페이스) 가져온 `CharacterEquipmentInventory`, `CharacterAssetComponent`, `CharacterAnimatorComponent`, `CharacterResourceSystem`, `CharacterAssetContainer`, `ItemTable`, UnityChan 스프링본 스크립트 등
- `Assets/Scripts/CharacterRendering/` — `solhwi/SebamoGameClient`의 `ObjectView`/`ObjectCameraManager`/`MeshRenderer2D`를 이식·수정한 것. 원본은 Addressable 카메라 프리팹과 자체 `ResourceManager`를 썼지만, 여기서는 ① 캐릭터 프리팹을 일반 참조로 직접 꽂고(이 프로젝트의 다른 모든 프리팹과 동일한 방식, `CharacterDemo`의 `DemoScene.cs`도 원래 이렇게 했음) ② 미리보기 카메라는 코드로 바로 생성(별도 Addressable 프리팹 불필요)하도록 단순화했습니다. 여러 미리보기가 서로의 화면에 끼어들지 않도록, 그리고 메인 게임 카메라가 절대 보지 못하도록 각 미리보기 카메라를 플레이 공간에서 멀리 떨어진 좌표에 배치하는 원본의 트릭을 그대로 유지했습니다.
  - `CharacterRenderView` — `ObjectView`를 CharacterModule(`CharacterAssetComponent`/`CharacterAnimatorComponent`)에 연결하는 어댑터 (SebamoGameClient 자체 캐릭터 시스템 대신)
- `Assets/Bundles/`, `Assets/Resources/UnityChan`, `Assets/Resources/SD Unity-Chan Haon Custom`, `Assets/AddressableAssetsData` — 원본 3D 모델/머티리얼/애니메이션/텍스처 및 Addressables 설정 (원본 `.meta` GUID를 그대로 보존해서 복사했기 때문에, `Assets/Bundles/Datas/*.asset`에 미리 구성되어 있던 아이템↔파츠 매핑이 그대로 유효합니다)
- `Assets/Scripts/Items/CharacterAppearanceAdapter.cs`, `AppearanceItemDefinition.cs`, `CharacterPreloader.cs` — 이번 프로젝트의 아이템 시스템(`ItemDefinition`/`ItemSkillBehavior`)과 위 두 모듈을 연결하는 브릿지 코드. `AppearanceItemDefinition`을 장착/해제하면 `CharacterEquipmentInventory`의 해당 슬롯(Body/Hair/Eye/Face/Accessory/Prop)을 갱신하고 `CharacterRenderView.Refresh()`(→ 내부적으로 `CharacterAssetComponent.Refresh()`)를 호출합니다.

### 에디터에서 직접 해야 하는 작업 (자동화 불가)

이 세션은 Unity 에디터가 없는 환경이라 아래는 코드/에셋만 옮겨뒀을 뿐 직접 확인·설정이 필요합니다. 기존 플레이어의 평면 스프라이트(`SpriteRenderer`)를 건드리면 지금까지 확인된 2D 게임플레이가 깨질 수 있어서, **씬에 자동으로 배치하지 않고 아래 수동 설정으로 남겨뒀습니다**:

1. **패키지 해결**: 프로젝트를 열면 Package Manager가 `com.unity.addressables`(원본 버전 1.21.21 그대로 기입)를 Unity `6000.2.7f2`에 맞는 버전으로 올리라고 안내할 수 있습니다 — 안내대로 업데이트하세요.
2. **Addressables 초기화 확인**: `Window > Asset Management > Addressables > Groups`를 한 번 열어서, 복사해온 `Assets/AddressableAssetsData`의 그룹(Default Local Group 등)이 정상 인식되는지 확인하세요.
3. **쿼드용 머티리얼 생성**: `Assets/Art`에 새 머티리얼을 만들고 셰이더를 `Universal Render Pipeline/2D/Sprite-Unlit-Default`로 지정하세요 (`RoguelikeSceneBuilder.GetSpriteMaterial()`이 플레이어 스프라이트에 쓰는 것과 동일한 URP 호환 셰이더 — 원본의 Built-in RP `Standard` 셰이더를 그대로 쓰면 URP에서 마젠타로 깨집니다).
4. **매니저 배치**: 빈 오브젝트에 `ObjectCameraManager`를 추가해 씬에 하나 둡니다.
5. **캐릭터 뷰 오브젝트 구성**: 새 GameObject(예: Player의 자식, 이름 `CharacterView`)에 `MeshFilter`(내장 Quad: `Assets > 3D Object > Quad`로 임시 생성 후 메시만 복사하거나 `GameObject/3D Object/Quad`로 만든 오브젝트에 컴포넌트를 옮겨 붙이는 식), `MeshRenderer`(머티리얼 = 3번에서 만든 것), `MeshRenderer2D`, `CharacterRenderView`를 추가합니다. `CharacterRenderView`의 `Origin Prefab`에 `Assets/Bundles/Prefabs/Character.prefab`을 연결하세요.
6. **프리로더 배치**: `CharacterPreloader` 컴포넌트를 씬의 매니저 오브젝트에 추가하고 `resourceSystem`(`Assets/Bundles/Datas/CharacterResourceSystem.asset`), `renderView`(5번에서 만든 `CharacterRenderView`)를 연결하세요.
7. **어댑터 연결**: Player에 `CharacterAppearanceAdapter`를 추가하고 `inventory`(`Assets/Bundles/Datas/CharacterEquipmentInventory.asset`), `renderView`를 연결하세요.
8. **외형 아이템 에셋 생성**: `Create > Roguelike > Items > Appearance Item`으로 아이템을 만들고 `slot`(Body/Hair/Eye/Face/Accessory/Prop)과 `itemCode`를 지정하세요. `itemCode`는 `Assets/Bundles/Datas/ItemTable.asset`에 이미 등록된 키와 일치해야 합니다.
9. 실제로 쿼드에 3D 캐릭터가 올바른 각도/크기로 찍히는지, 메인 2D 카메라가 미리보기 카메라의 먼 좌표(기본 `(10000, 10000, 0)`대)를 보지 않는지, Animator Controller(`Assets/Bundles/Animations/Character.controller`)가 이동 입력과 자연스럽게 맞물리는지는 시각적 확인이 필요합니다.

원본 프로젝트들은 Unity `2023.2.17f1`에서 작업되었던 것이라, 처음 열 때 에셋 재임포트/업그레이드 과정이 다소 걸릴 수 있습니다.
