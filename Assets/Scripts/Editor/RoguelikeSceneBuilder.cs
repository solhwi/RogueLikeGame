using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using RogueLike.Core;
using RogueLike.Player;
using RogueLike.Combat;
using RogueLike.Items;
using RogueLike.Level;
using RogueLike.Enemies;
using RogueLike.Managers;
using RogueLike.CameraSystem;
using RogueLike.UI;

namespace RogueLike.EditorTools
{
    /// <summary>
    /// Builds a minimal playable slice of the roguelike conversion: a
    /// starting item, two level-up item choices, one enemy type, one wave,
    /// one chapter, wired into a scene so the core loop (move -> auto-attack
    /// -> gain XP -> level up -> pick an item) can be tried immediately.
    /// </summary>
    public static class RoguelikeSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/RogueLikeGameScene.unity";
        private const string ArtFolder = "Assets/Art";
        private const string PrefabFolder = "Assets/Prefabs";
        private const string DataFolder = "Assets/Data";

        [MenuItem("Tools/Roguelike/Build Test Scene")]
        public static void Build()
        {
            try
            {
                BuildInternal();
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                else
                {
                    throw;
                }
            }
        }

        private static void BuildInternal()
        {
            int enemyLayer = EnsureLayer("Enemy");
            int lootLayer = EnsureLayer("Loot");

            var playerSprite = CreateAndSaveSquareSprite("RoguelikePlayerSprite", new Color(0.20f, 0.55f, 0.95f));
            var enemySprite = CreateAndSaveSquareSprite("RoguelikeEnemySprite", new Color(0.55f, 0.85f, 0.25f));
            var projectileSprite = CreateAndSaveSquareSprite("RoguelikeProjectileSprite", new Color(0.95f, 0.85f, 0.20f));
            var gemSprite = CreateAndSaveSquareSprite("RoguelikeGemSprite", new Color(0.35f, 0.90f, 0.95f));

            var projectilePrefab = CreateProjectilePrefab(projectileSprite);
            var enemyPrefab = CreateEnemyPrefab(enemySprite, enemyLayer);
            var gemPrefab = CreateGemPrefab(gemSprite, lootLayer);

            var basicShotIcon = CreateAndSaveSquareSprite("ItemIcon_BasicShot", new Color(0.95f, 0.65f, 0.20f));
            var moveSpeedIcon = CreateAndSaveSquareSprite("ItemIcon_MoveSpeedUp", new Color(0.45f, 0.90f, 0.45f));
            var magnetIcon = CreateAndSaveSquareSprite("ItemIcon_MagnetUp", new Color(0.75f, 0.45f, 0.95f));
            var throwingKnifeIcon = CreateAndSaveSquareSprite("ItemIcon_ThrowingKnife", new Color(0.85f, 0.85f, 0.90f));

            // Everything the player can end up with is an item: some start
            // owned, some are offered on level-up — see RunManager's
            // availableItemPool below.
            var basicShotItem = CreateProjectileItem(projectilePrefab, basicShotIcon, "basic_shot", "Basic Shot", "가장 가까운 적에게 투사체를 자동으로 발사한다.");
            var moveSpeedItem = CreateStatBonusItem(moveSpeedIcon, "move_speed_up", "Swift Boots", StatType.MoveSpeedMultiplier, 0.15f, "이동 속도가 증가한다.");
            var magnetItem = CreateStatBonusItem(magnetIcon, "magnet_up", "Loot Magnet", StatType.PickupRange, 1.5f, "아이템 획득 범위가 증가한다.");
            var throwingKnifeItem = CreateProjectileItem(projectilePrefab, throwingKnifeIcon, "throwing_knife", "Throwing Knife", "가장 가까운 적에게 칼을 자동으로 던진다.");

            var itemDatabase = CreateAsset<ItemDatabase>($"{DataFolder}/Items", "ItemDatabase");
            var itemDatabaseSO = new SerializedObject(itemDatabase);
            var databaseItemsProp = itemDatabaseSO.FindProperty("items");
            var allItems = new ItemDefinition[] { basicShotItem, moveSpeedItem, magnetItem, throwingKnifeItem };
            AssignArray(databaseItemsProp, allItems);
            itemDatabaseSO.ApplyModifiedProperties();

            // Asset rather than a scene component, so it's referenced the
            // same way from the player's loadout, the UI and RunManager
            // (which calls ResetRun() on it) — see ItemInventory's own
            // comment for why.
            var itemInventory = CreateAsset<ItemInventory>($"{DataFolder}/Items", "PlayerItemInventory");
            var itemInventorySO = new SerializedObject(itemInventory);
            itemInventorySO.FindProperty("database").objectReferenceValue = itemDatabase;
            var startingItemsProp = itemInventorySO.FindProperty("startingItems");
            startingItemsProp.arraySize = 1;
            startingItemsProp.GetArrayElementAtIndex(0).objectReferenceValue = throwingKnifeItem;
            itemInventorySO.ApplyModifiedProperties();

            var zombieDefinition = CreateZombieDefinition(enemyPrefab, gemPrefab);
            var waveData = CreateWaveData(zombieDefinition);
            CreateChapterDefinition(waveData);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // --- Player ---
            var playerGo = new GameObject("Player");
            playerGo.tag = "Player";

            var playerSr = playerGo.AddComponent<SpriteRenderer>();
            playerSr.sprite = playerSprite;
            playerSr.sharedMaterial = GetSpriteMaterial();
            playerSr.sortingOrder = 10;

            var playerRb = playerGo.AddComponent<Rigidbody2D>();
            playerRb.gravityScale = 0f;
            playerRb.freezeRotation = true;

            playerGo.AddComponent<CircleCollider2D>();
            var playerHealth = playerGo.AddComponent<Health>();

            var playerFlicker = playerGo.AddComponent<DamageFlicker>();
            new SerializedObject(playerFlicker).ApplySpriteRenderer(playerSr);

            playerGo.AddComponent<PlayerInputHandler>();
            playerGo.AddComponent<PlayerController>();
            playerGo.AddComponent<LootMagnet>();

            var lootMagnet = playerGo.GetComponent<LootMagnet>();
            new SerializedObject(lootMagnet).ApplyLootLayer(lootLayer);

            // --- Item-skill loadout (item = skill; see Items/*) ---
            // Drives every equipped item's own behavior, and is also where
            // PlayerController/LootMagnet read passive stat bonuses from —
            // there's no separate skill system any more. ItemInventory is a
            // ScriptableObject asset (created above, outside the scene)
            // rather than a player component.
            var itemLoadout = playerGo.AddComponent<ItemSkillLoadout>();
            new SerializedObject(itemLoadout).ApplyEnemyLayer(enemyLayer);

            // --- Run manager (waves + bounds) ---
            var runManagerGo = new GameObject("RunManager");

            var waveSpawner = runManagerGo.AddComponent<WaveSpawner>();
            var waveSpawnerSO = new SerializedObject(waveSpawner);
            waveSpawnerSO.FindProperty("player").objectReferenceValue = playerGo.transform;
            waveSpawnerSO.ApplyModifiedProperties();

            var chapterBounds = runManagerGo.AddComponent<ChapterBounds>();
            var chapterBoundsSO = new SerializedObject(chapterBounds);
            chapterBoundsSO.FindProperty("playerBody").objectReferenceValue = playerRb;
            chapterBoundsSO.ApplyModifiedProperties();

            var runManager = runManagerGo.AddComponent<RunManager>();
            var runManagerSO = new SerializedObject(runManager);

            // Re-load rather than reuse the `chapter` reference captured
            // before EditorSceneManager.NewScene() above: that call can
            // invalidate an in-memory handle to an asset that hadn't been
            // flushed to disk yet, silently leaving this field null even
            // though the asset itself was written correctly.
            var chapterAsset = AssetDatabase.LoadAssetAtPath<ChapterDefinition>($"{DataFolder}/Chapters/TestChapter.asset");
            if (chapterAsset == null)
            {
                Debug.LogError("RoguelikeSceneBuilder: failed to (re)load TestChapter.asset for RunManager.chapter binding.");
            }
            runManagerSO.FindProperty("chapter").objectReferenceValue = chapterAsset;
            runManagerSO.FindProperty("player").objectReferenceValue = playerGo.transform;
            runManagerSO.FindProperty("playerHealth").objectReferenceValue = playerHealth;
            runManagerSO.FindProperty("waveSpawner").objectReferenceValue = waveSpawner;
            runManagerSO.FindProperty("bounds").objectReferenceValue = chapterBounds;
            runManagerSO.FindProperty("itemInventory").objectReferenceValue = itemInventory;
            runManagerSO.FindProperty("itemLoadout").objectReferenceValue = itemLoadout;
            var itemPoolProp = runManagerSO.FindProperty("availableItemPool");
            AssignArray(itemPoolProp, new ItemDefinition[] { basicShotItem, moveSpeedItem, magnetItem });
            runManagerSO.FindProperty("itemChoiceCount").intValue = 3;
            runManagerSO.ApplyModifiedProperties();

            // --- Camera ---
            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var cam = cameraGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cameraGo.AddComponent<AudioListener>();

            var camFollow = cameraGo.AddComponent<CameraFollow2D>();
            var camSO = new SerializedObject(camFollow);
            camSO.FindProperty("target").objectReferenceValue = playerGo.transform;
            camSO.FindProperty("offset").vector2Value = Vector2.zero;
            camSO.ApplyModifiedProperties();

            // --- Background ---
            CreateGroundTilemap(cam);

            // --- Managers ---
            new GameObject("GameManager").AddComponent<GameManager>();
            new GameObject("AudioManager").AddComponent<AudioManager>();
            new GameObject("ExperienceManager").AddComponent<ExperienceManager>();
            new GameObject("KillCounter").AddComponent<KillCounter>();

            // --- UI ---
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            // Portrait mobile reference (1080x1920). Match on height: phone
            // widths vary a lot more than heights across devices/aspect
            // ratios, so anchoring the scale to height keeps vertical layout
            // (HUD stacking, joystick reach) consistent between them.
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 1f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // --- Top bar: pause (left) / timer (center) / currency + kill count (right) ---
            var topBarGo = new GameObject("TopBar", typeof(RectTransform));
            topBarGo.transform.SetParent(canvasGo.transform, false);
            var topBarRt = topBarGo.GetComponent<RectTransform>();
            topBarRt.anchorMin = new Vector2(0f, 1f);
            topBarRt.anchorMax = new Vector2(1f, 1f);
            topBarRt.pivot = new Vector2(0.5f, 1f);
            topBarRt.anchoredPosition = Vector2.zero;
            topBarRt.sizeDelta = new Vector2(0f, 190f);
            topBarGo.AddComponent<Image>().color = new Color(0.12f, 0.14f, 0.2f, 0.85f);

            var pauseIconSprite = CreateAndSavePixelSprite(ArtFolder, "RoguelikePauseIconSprite", PauseIconPixels, PauseIconPixel);
            var pauseButton = CreatePauseButton(canvasGo.transform, pauseIconSprite, new Vector2(24f, -24f), new Vector2(96f, 96f));

            var timerTextGo = CreateUIText(canvasGo.transform, "TimerText", "00:00", new Vector2(0f, -40f), new Vector2(300f, 70f));
            var timerRt = timerTextGo.GetComponent<RectTransform>();
            timerRt.anchorMin = new Vector2(0.5f, 1f);
            timerRt.anchorMax = new Vector2(0.5f, 1f);
            timerRt.pivot = new Vector2(0.5f, 1f);
            var timerText = timerTextGo.GetComponent<Text>();
            timerText.alignment = TextAnchor.MiddleCenter;
            timerText.fontSize = 56;
            timerText.fontStyle = FontStyle.Bold;
            var elapsedTimeDisplay = timerTextGo.AddComponent<ElapsedTimeDisplay>();
            new SerializedObject(elapsedTimeDisplay).ApplyTimeText(timerText);

            var currencyIconSprite = CreateAndSaveSquareSprite("RoguelikeCurrencyIconSprite", new Color(0.95f, 0.8f, 0.25f));
            CreateIconCounter(canvasGo.transform, "CurrencyCount", currencyIconSprite, new Vector2(-24f, -28f), "0");

            var killIconSprite = CreateAndSaveSquareSprite("RoguelikeKillIconSprite", new Color(0.85f, 0.25f, 0.25f));
            var killCountText = CreateIconCounter(canvasGo.transform, "KillCount", killIconSprite, new Vector2(-24f, -100f), "0");

            // --- Experience bar (segmented) + level readout ---
            var (experienceSegments, levelTextComponent) = CreateExperienceBar(canvasGo.transform, new Vector2(0f, -220f), ExperienceSegmentCount);

            var healthDotSprite = CreateAndSavePixelSprite(ArtFolder, "RoguelikeHealthDotSprite", HealthDotPixels, HealthDotPixel);
            var healthDots = CreateHealthDots(canvasGo.transform, healthDotSprite, new Vector2(40f, -280f), MaxHealthDots);

            // --- Item-skill slots + inventory panel (item = skill) ---
            var (itemPanel, itemListParent) = CreateItemInventoryPanel(canvasGo.transform);
            itemPanel.SetActive(false);

            var itemEntryPrefab = CreateItemEntryPrefab(throwingKnifeIcon).GetComponent<ItemInventoryEntryUI>();

            var itemInventoryUI = canvasGo.AddComponent<ItemInventoryUI>();
            var itemInventoryUISO = new SerializedObject(itemInventoryUI);
            itemInventoryUISO.FindProperty("inventory").objectReferenceValue = itemInventory;
            itemInventoryUISO.FindProperty("loadout").objectReferenceValue = itemLoadout;
            itemInventoryUISO.FindProperty("panel").objectReferenceValue = itemPanel;
            itemInventoryUISO.FindProperty("listParent").objectReferenceValue = itemListParent;
            itemInventoryUISO.FindProperty("entryPrefab").objectReferenceValue = itemEntryPrefab;
            itemInventoryUISO.ApplyModifiedProperties();

            itemPanel.transform.Find("CloseButton").GetComponent<Button>().onClick.AddListener(itemInventoryUI.Close);

            CreateItemSkillSlotRow(canvasGo.transform, new Vector2(0f, 150f), ItemSlotVisualCount, itemLoadout, itemInventoryUI);

            var levelUpPanel = CreatePanel(canvasGo.transform, "LevelUpPanel", "LEVEL UP!");
            levelUpPanel.SetActive(false);
            levelUpPanel.transform.Find("Label").GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 480f);

            var choiceButtons = new Button[3];
            var choiceLabels = new Text[3];
            float[] yOffsets = { 150f, 0f, -150f };
            for (int i = 0; i < 3; i++)
            {
                var (btn, label) = CreateButton(levelUpPanel.transform, $"Choice_{i}", new Vector2(0f, yOffsets[i]), new Vector2(720f, 110f));
                choiceButtons[i] = btn;
                choiceLabels[i] = label;
            }

            CreateMoveJoystick(canvasGo.transform);

            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<InputSystemUIInputModule>();

            var chapterHud = canvasGo.AddComponent<ChapterHUD>();
            var chapterHudSO = new SerializedObject(chapterHud);
            chapterHudSO.FindProperty("playerHealth").objectReferenceValue = playerHealth;
            AssignArray(chapterHudSO.FindProperty("healthDots"), healthDots);
            AssignArray(chapterHudSO.FindProperty("experienceSegments"), experienceSegments);
            chapterHudSO.FindProperty("levelText").objectReferenceValue = levelTextComponent;
            chapterHudSO.FindProperty("killCountText").objectReferenceValue = killCountText;
            chapterHudSO.ApplyModifiedProperties();

            pauseButton.gameObject.AddComponent<PauseButton>();

            var levelUpUI = canvasGo.AddComponent<LevelUpSelectionUI>();
            var levelUpUISO = new SerializedObject(levelUpUI);
            levelUpUISO.FindProperty("runManager").objectReferenceValue = runManager;
            levelUpUISO.FindProperty("panel").objectReferenceValue = levelUpPanel;
            var choiceButtonsProp = levelUpUISO.FindProperty("choiceButtons");
            choiceButtonsProp.arraySize = 3;
            var choiceLabelsProp = levelUpUISO.FindProperty("choiceLabels");
            choiceLabelsProp.arraySize = 3;
            for (int i = 0; i < 3; i++)
            {
                choiceButtonsProp.GetArrayElementAtIndex(i).objectReferenceValue = choiceButtons[i];
                choiceLabelsProp.GetArrayElementAtIndex(i).objectReferenceValue = choiceLabels[i];
            }
            levelUpUISO.ApplyModifiedProperties();

            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Roguelike game scene created at {ScenePath}");
        }

        private static GameObject CreateProjectilePrefab(Sprite sprite)
        {
            var go = new GameObject("RoguelikeProjectile");
            go.transform.localScale = new Vector3(0.25f, 0.25f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sharedMaterial = GetSpriteMaterial();
            sr.sortingOrder = 5;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            go.AddComponent<Projectile>();

            return SaveAsPrefabAndDestroy(go, $"{PrefabFolder}/RoguelikeProjectile.prefab");
        }

        private static GameObject CreateEnemyPrefab(Sprite sprite, int enemyLayer)
        {
            var go = new GameObject("RoguelikeEnemy_Zombie");
            go.layer = enemyLayer;
            go.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sharedMaterial = GetSpriteMaterial();
            sr.sortingOrder = 4;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            go.AddComponent<CircleCollider2D>();
            go.AddComponent<Health>();
            go.AddComponent<Hazard>();
            go.AddComponent<EnemyChaseAI>();

            return SaveAsPrefabAndDestroy(go, $"{PrefabFolder}/RoguelikeEnemy_Zombie.prefab");
        }

        private static GameObject CreateGemPrefab(Sprite sprite, int lootLayer)
        {
            var go = new GameObject("RoguelikeExperienceGem");
            go.layer = lootLayer;
            go.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sharedMaterial = GetSpriteMaterial();
            sr.sortingOrder = 3;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;

            go.AddComponent<ExperienceGem>();

            return SaveAsPrefabAndDestroy(go, $"{PrefabFolder}/RoguelikeExperienceGem.prefab");
        }

        private static GameObject SaveAsPrefabAndDestroy(GameObject instance, string path)
        {
            if (!AssetDatabase.IsValidFolder(PrefabFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            var prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
            UnityEngine.Object.DestroyImmediate(instance);
            return prefab;
        }

        private static StatBonusItemDefinition CreateStatBonusItem(Sprite icon, string itemKey, string displayName, StatType statType, float bonusValue, string description)
        {
            var item = CreateAsset<StatBonusItemDefinition>($"{DataFolder}/Items", displayName.Replace(" ", string.Empty));
            var so = new SerializedObject(item);
            so.FindProperty("itemKey").stringValue = itemKey;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("icon").objectReferenceValue = icon;
            so.FindProperty("description").stringValue = description;
            so.FindProperty("statType").enumValueIndex = (int)statType;
            so.FindProperty("bonusValue").floatValue = bonusValue;
            so.ApplyModifiedProperties();
            return item;
        }

        private static ProjectileItemDefinition CreateProjectileItem(GameObject projectilePrefab, Sprite icon, string itemKey, string displayName, string description)
        {
            var item = CreateAsset<ProjectileItemDefinition>($"{DataFolder}/Items", displayName.Replace(" ", string.Empty));
            var so = new SerializedObject(item);
            so.FindProperty("itemKey").stringValue = itemKey;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("icon").objectReferenceValue = icon;
            so.FindProperty("description").stringValue = description;
            so.FindProperty("projectilePrefab").objectReferenceValue = projectilePrefab;
            so.FindProperty("range").floatValue = 6f;
            so.FindProperty("cooldown").floatValue = 1.2f;
            so.FindProperty("damage").intValue = 3;
            so.FindProperty("projectileCount").intValue = 1;
            so.FindProperty("pierceCount").intValue = 0;
            so.FindProperty("projectileSpeed").floatValue = 9f;
            so.ApplyModifiedProperties();
            return item;
        }

        private static EnemyDefinition CreateZombieDefinition(GameObject enemyPrefab, GameObject gemPrefab)
        {
            var definition = CreateAsset<EnemyDefinition>($"{DataFolder}/Enemies", "BasicZombie");
            var so = new SerializedObject(definition);
            so.FindProperty("enemyId").stringValue = "basic_zombie";
            so.FindProperty("prefab").objectReferenceValue = enemyPrefab;
            so.FindProperty("maxHealth").intValue = 5;
            so.FindProperty("moveSpeed").floatValue = 1.5f;
            so.FindProperty("contactDamage").intValue = 1;
            so.FindProperty("tier").enumValueIndex = (int)EnemyTier.Normal;
            so.FindProperty("experienceGemPrefab").objectReferenceValue = gemPrefab;
            so.FindProperty("experienceReward").intValue = 1;
            so.ApplyModifiedProperties();
            return definition;
        }

        private static WaveData CreateWaveData(EnemyDefinition zombieDefinition)
        {
            var waveData = CreateAsset<WaveData>($"{DataFolder}/Waves", "TestChapterWaves");
            var so = new SerializedObject(waveData);

            var entriesProp = so.FindProperty("entries");
            entriesProp.arraySize = 1;
            var entry = entriesProp.GetArrayElementAtIndex(0);
            entry.FindPropertyRelative("enemy").objectReferenceValue = zombieDefinition;
            entry.FindPropertyRelative("startTime").floatValue = 0f;
            entry.FindPropertyRelative("endTime").floatValue = 99999f;
            entry.FindPropertyRelative("spawnInterval").floatValue = 1.5f;
            entry.FindPropertyRelative("countPerSpawn").intValue = 1;

            so.FindProperty("bossSpawnTime").floatValue = 99999f;
            so.ApplyModifiedProperties();
            return waveData;
        }

        private static ChapterDefinition CreateChapterDefinition(WaveData waveData)
        {
            var chapter = CreateAsset<ChapterDefinition>($"{DataFolder}/Chapters", "TestChapter");
            var so = new SerializedObject(chapter);
            so.FindProperty("chapterId").stringValue = "test_chapter";
            so.FindProperty("mapType").enumValueIndex = (int)ChapterMapType.Infinite;
            so.FindProperty("energyCost").intValue = 5;
            so.FindProperty("waveData").objectReferenceValue = waveData;
            so.FindProperty("mapBoundsSize").vector2Value = new Vector2(20f, 20f);
            so.ApplyModifiedProperties();
            return chapter;
        }

        private static T CreateAsset<T>(string folder, string assetName) where T : ScriptableObject
        {
            EnsureFolder(folder);
            string path = $"{folder}/{assetName}.asset";

            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                return existing;
            }

            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            int slash = path.LastIndexOf('/');
            string parent = path.Substring(0, slash);
            string folderName = path.Substring(slash + 1);

            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }
            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static int EnsureLayer(string layerName)
        {
            var tagManagerAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];
            var tagManager = new SerializedObject(tagManagerAsset);
            var layersProp = tagManager.FindProperty("layers");

            for (int i = 8; i < layersProp.arraySize; i++)
            {
                if (layersProp.GetArrayElementAtIndex(i).stringValue == layerName)
                {
                    return i;
                }
            }

            for (int i = 8; i < layersProp.arraySize; i++)
            {
                var layerProp = layersProp.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(layerProp.stringValue))
                {
                    layerProp.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                    return i;
                }
            }

            throw new InvalidOperationException($"No free layer slot available for '{layerName}' layer.");
        }

        private static void AssignArray(SerializedProperty arrayProp, UnityEngine.Object[] values)
        {
            arrayProp.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                arrayProp.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        // --- Item-skill slots + inventory panel ---------------------------------
        private const int ItemSlotVisualCount = 6;
        private const float ItemSlotIconSize = 130f;
        private const float ItemSlotSpacing = 146f;
        private const float ItemSlotHeight = ItemSlotIconSize + 36f;

        // Row of compact item-skill slots (icon + name only — the full skill
        // description is shown in the inventory panel entries below, where
        // there's room for it). Each slot wires itself up: tapping it opens
        // the shared ItemInventoryUI targeted at that slot index.
        private static void CreateItemSkillSlotRow(Transform parent, Vector2 anchoredPosition, int count, ItemSkillLoadout loadout, ItemInventoryUI inventoryUI)
        {
            var containerGo = new GameObject("ItemSlotRow", typeof(RectTransform));
            containerGo.transform.SetParent(parent, false);
            var containerRt = containerGo.GetComponent<RectTransform>();
            containerRt.anchorMin = new Vector2(0.5f, 0f);
            containerRt.anchorMax = new Vector2(0.5f, 0f);
            containerRt.pivot = new Vector2(0.5f, 0f);
            containerRt.anchoredPosition = anchoredPosition;
            containerRt.sizeDelta = new Vector2(count * ItemSlotSpacing, ItemSlotHeight);

            var layoutGroup = containerGo.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = ItemSlotSpacing - ItemSlotIconSize;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            for (int i = 0; i < count; i++)
            {
                var slotGo = new GameObject($"ItemSlot_{i}", typeof(RectTransform));
                slotGo.transform.SetParent(containerGo.transform, false);
                slotGo.GetComponent<RectTransform>().sizeDelta = new Vector2(ItemSlotIconSize, ItemSlotHeight);

                var bgImg = slotGo.AddComponent<Image>();
                bgImg.color = new Color(0.2f, 0.14f, 0.05f, 0.5f);
                var button = slotGo.AddComponent<Button>();
                button.targetGraphic = bgImg;

                var iconGo = new GameObject("Icon", typeof(RectTransform));
                iconGo.transform.SetParent(slotGo.transform, false);
                var iconRt = iconGo.GetComponent<RectTransform>();
                iconRt.anchorMin = new Vector2(0f, 1f);
                iconRt.anchorMax = new Vector2(1f, 1f);
                iconRt.pivot = new Vector2(0.5f, 1f);
                iconRt.anchoredPosition = Vector2.zero;
                iconRt.sizeDelta = new Vector2(0f, ItemSlotIconSize);
                var iconImg = iconGo.AddComponent<Image>();
                iconGo.SetActive(false);

                var emptyGo = CreateUIText(slotGo.transform, "Empty", "+", Vector2.zero, new Vector2(ItemSlotIconSize, ItemSlotIconSize));
                var emptyRt = emptyGo.GetComponent<RectTransform>();
                emptyRt.anchorMin = new Vector2(0f, 1f);
                emptyRt.anchorMax = new Vector2(1f, 1f);
                emptyRt.pivot = new Vector2(0.5f, 1f);
                emptyRt.anchoredPosition = Vector2.zero;
                emptyRt.sizeDelta = new Vector2(0f, ItemSlotIconSize);
                var emptyText = emptyGo.GetComponent<Text>();
                emptyText.alignment = TextAnchor.MiddleCenter;
                emptyText.fontSize = 40;
                emptyText.color = new Color(1f, 1f, 1f, 0.4f);

                var nameGo = CreateUIText(slotGo.transform, "Name", "", Vector2.zero, new Vector2(ItemSlotIconSize, 32f));
                var nameRt = nameGo.GetComponent<RectTransform>();
                nameRt.anchorMin = new Vector2(0.5f, 0f);
                nameRt.anchorMax = new Vector2(0.5f, 0f);
                nameRt.pivot = new Vector2(0.5f, 0f);
                nameRt.anchoredPosition = Vector2.zero;
                var nameText = nameGo.GetComponent<Text>();
                nameText.fontSize = 20;
                nameText.alignment = TextAnchor.UpperCenter;

                var slotUI = slotGo.AddComponent<ItemSkillSlotUI>();
                var so = new SerializedObject(slotUI);
                so.FindProperty("loadout").objectReferenceValue = loadout;
                so.FindProperty("inventoryUI").objectReferenceValue = inventoryUI;
                so.FindProperty("slotIndex").intValue = i;
                so.FindProperty("icon").objectReferenceValue = iconImg;
                so.FindProperty("nameLabel").objectReferenceValue = nameText;
                so.FindProperty("emptyState").objectReferenceValue = emptyGo;
                so.ApplyModifiedProperties();
            }
        }

        // Full-screen dim panel with a title, a close button and a scrollable
        // vertical list (viewport + content, driven by a VerticalLayoutGroup +
        // ContentSizeFitter) that ItemInventoryUI populates with entry
        // instances at runtime. Returns the panel and the list's content
        // transform (what entries get parented under).
        private static (GameObject panel, Transform listParent) CreateItemInventoryPanel(Transform canvasParent)
        {
            var panelGo = new GameObject("ItemInventoryPanel", typeof(RectTransform));
            panelGo.transform.SetParent(canvasParent, false);
            SetStretch(panelGo.GetComponent<RectTransform>());
            panelGo.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.8f);

            var titleGo = CreateUIText(panelGo.transform, "Title", "인벤토리", Vector2.zero, new Vector2(500f, 70f));
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 1f);
            titleRt.anchorMax = new Vector2(0.5f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -60f);
            var titleText = titleGo.GetComponent<Text>();
            titleText.fontSize = 44;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;

            var (closeButton, closeLabel) = CreateButton(panelGo.transform, "CloseButton", Vector2.zero, new Vector2(100f, 100f));
            var closeRt = closeButton.GetComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(1f, 1f);
            closeRt.anchorMax = new Vector2(1f, 1f);
            closeRt.pivot = new Vector2(1f, 1f);
            closeRt.anchoredPosition = new Vector2(-24f, -24f);
            closeLabel.text = "X";
            closeLabel.fontSize = 40;

            var scrollGo = new GameObject("ScrollView", typeof(RectTransform));
            scrollGo.transform.SetParent(panelGo.transform, false);
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0.5f, 0f);
            scrollRt.anchorMax = new Vector2(0.5f, 1f);
            scrollRt.pivot = new Vector2(0.5f, 0.5f);
            scrollRt.anchoredPosition = new Vector2(0f, -60f);
            scrollRt.sizeDelta = new Vector2(980f, -320f);

            var viewportGo = new GameObject("Viewport", typeof(RectTransform));
            viewportGo.transform.SetParent(scrollGo.transform, false);
            SetStretch(viewportGo.GetComponent<RectTransform>());
            viewportGo.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f);
            viewportGo.AddComponent<RectMask2D>();

            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRt = contentGo.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = Vector2.zero;

            var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 16f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            contentGo.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scrollRect = scrollGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.viewport = viewportGo.GetComponent<RectTransform>();
            scrollRect.content = contentRt;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            return (panelGo, contentGo.transform);
        }

        // One inventory row: icon, name, skill description and an "equipped"
        // badge that ItemInventoryEntryUI.Bind toggles per item. Saved as a
        // prefab so ItemInventoryUI can Instantiate one per owned item.
        private static GameObject CreateItemEntryPrefab(Sprite placeholderIcon)
        {
            var go = new GameObject("ItemInventoryEntry", typeof(RectTransform));
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(920f, 140f);

            var bgImg = go.AddComponent<Image>();
            bgImg.color = new Color(1f, 1f, 1f, 0.08f);
            var button = go.AddComponent<Button>();
            button.targetGraphic = bgImg;
            go.AddComponent<LayoutElement>().preferredHeight = 140f;

            var iconGo = new GameObject("Icon", typeof(RectTransform));
            iconGo.transform.SetParent(go.transform, false);
            var iconRt = iconGo.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0f, 0.5f);
            iconRt.anchorMax = new Vector2(0f, 0.5f);
            iconRt.pivot = new Vector2(0f, 0.5f);
            iconRt.anchoredPosition = new Vector2(20f, 0f);
            iconRt.sizeDelta = new Vector2(100f, 100f);
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = placeholderIcon;

            var nameGo = CreateUIText(go.transform, "Name", "", Vector2.zero, new Vector2(680f, 44f));
            var nameRt = nameGo.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0f, 1f);
            nameRt.anchorMax = new Vector2(0f, 1f);
            nameRt.pivot = new Vector2(0f, 1f);
            nameRt.anchoredPosition = new Vector2(140f, -16f);
            var nameText = nameGo.GetComponent<Text>();
            nameText.fontSize = 32;
            nameText.fontStyle = FontStyle.Bold;

            var descGo = CreateUIText(go.transform, "Description", "", Vector2.zero, new Vector2(680f, 70f));
            var descRt = descGo.GetComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0f, 1f);
            descRt.anchorMax = new Vector2(0f, 1f);
            descRt.pivot = new Vector2(0f, 1f);
            descRt.anchoredPosition = new Vector2(140f, -62f);
            var descText = descGo.GetComponent<Text>();
            descText.fontSize = 22;
            descText.color = new Color(1f, 1f, 1f, 0.75f);

            var badgeGo = new GameObject("EquippedBadge", typeof(RectTransform));
            badgeGo.transform.SetParent(go.transform, false);
            var badgeRt = badgeGo.GetComponent<RectTransform>();
            badgeRt.anchorMin = new Vector2(1f, 0.5f);
            badgeRt.anchorMax = new Vector2(1f, 0.5f);
            badgeRt.pivot = new Vector2(1f, 0.5f);
            badgeRt.anchoredPosition = new Vector2(-20f, 0f);
            badgeRt.sizeDelta = new Vector2(140f, 50f);
            badgeGo.AddComponent<Image>().color = new Color(0.3f, 0.85f, 0.4f, 0.9f);

            var badgeLabelGo = CreateUIText(badgeGo.transform, "Label", "장착중", Vector2.zero, new Vector2(140f, 50f));
            SetStretch(badgeLabelGo.GetComponent<RectTransform>());
            var badgeLabelText = badgeLabelGo.GetComponent<Text>();
            badgeLabelText.alignment = TextAnchor.MiddleCenter;
            badgeLabelText.fontSize = 22;
            badgeLabelText.color = Color.black;

            var entryUI = go.AddComponent<ItemInventoryEntryUI>();
            var entrySO = new SerializedObject(entryUI);
            entrySO.FindProperty("icon").objectReferenceValue = iconImg;
            entrySO.FindProperty("nameLabel").objectReferenceValue = nameText;
            entrySO.FindProperty("descriptionLabel").objectReferenceValue = descText;
            entrySO.FindProperty("equippedBadge").objectReferenceValue = badgeGo;
            entrySO.ApplyModifiedProperties();

            return SaveAsPrefabAndDestroy(go, $"{PrefabFolder}/ItemInventoryEntry.prefab");
        }

        // --- Top bar: pause button / icon counter ------------------------------
        private const int PauseIconPixels = 16;

        private static Button CreatePauseButton(Transform parent, Sprite icon, Vector2 anchoredPosition, Vector2 size)
        {
            var go = new GameObject("PauseButton", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = size;

            var bgImg = go.AddComponent<Image>();
            bgImg.color = new Color(0.85f, 0.85f, 0.9f, 0.9f);

            var button = go.AddComponent<Button>();
            button.targetGraphic = bgImg;

            var iconGo = new GameObject("Icon", typeof(RectTransform));
            iconGo.transform.SetParent(go.transform, false);
            SetStretch(iconGo.GetComponent<RectTransform>());
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = icon;
            iconImg.color = new Color(0.15f, 0.15f, 0.2f);
            iconImg.raycastTarget = false;

            return button;
        }

        // Two vertical bars on a transparent background.
        private static Color PauseIconPixel(int x, int y)
        {
            bool inBar = (x >= 3 && x <= 6) || (x >= 9 && x <= 12);
            return inBar ? Color.white : new Color(1f, 1f, 1f, 0f);
        }

        // Icon on the left, a number on the right — anchored wherever
        // `anchoredPosition` places it (used top-right, pivoted there).
        private static Text CreateIconCounter(Transform parent, string name, Sprite icon, Vector2 anchoredPosition, string initialValue)
        {
            var containerGo = new GameObject(name, typeof(RectTransform));
            containerGo.transform.SetParent(parent, false);
            var containerRt = containerGo.GetComponent<RectTransform>();
            containerRt.anchorMin = new Vector2(1f, 1f);
            containerRt.anchorMax = new Vector2(1f, 1f);
            containerRt.pivot = new Vector2(1f, 1f);
            containerRt.anchoredPosition = anchoredPosition;
            containerRt.sizeDelta = new Vector2(220f, 60f);

            var iconGo = new GameObject("Icon", typeof(RectTransform));
            iconGo.transform.SetParent(containerGo.transform, false);
            var iconRt = iconGo.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0f, 0.5f);
            iconRt.anchorMax = new Vector2(0f, 0.5f);
            iconRt.pivot = new Vector2(0f, 0.5f);
            iconRt.anchoredPosition = Vector2.zero;
            iconRt.sizeDelta = new Vector2(56f, 56f);
            iconGo.AddComponent<Image>().sprite = icon;

            var textGo = CreateUIText(containerGo.transform, "Count", initialValue, new Vector2(66f, 0f), new Vector2(150f, 56f));
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0f, 0.5f);
            textRt.anchorMax = new Vector2(0f, 0.5f);
            textRt.pivot = new Vector2(0f, 0.5f);
            var text = textGo.GetComponent<Text>();
            text.alignment = TextAnchor.MiddleLeft;
            text.fontSize = 36;

            return text;
        }

        // --- Experience bar (segmented) -----------------------------------------
        private const int ExperienceSegmentCount = 20;
        private const float ExperienceBarWidth = 1000f;
        private const float ExperienceBarHeight = 50f;
        private const float ExperienceSegmentGap = 3f;
        private const float ExperienceBarLevelZoneWidth = 170f;

        // Segments fill the left portion of the bar; the level number sits
        // in a reserved zone at the right end of the SAME bar, so both stay
        // visually locked together as one widget (matching the reference:
        // a single bordered pill with pips on the left, count on the right).
        private static (Image[] segments, Text levelText) CreateExperienceBar(Transform parent, Vector2 anchoredPosition, int count)
        {
            var containerGo = new GameObject("ExperienceBar", typeof(RectTransform));
            containerGo.transform.SetParent(parent, false);
            var containerRt = containerGo.GetComponent<RectTransform>();
            containerRt.anchorMin = new Vector2(0.5f, 1f);
            containerRt.anchorMax = new Vector2(0.5f, 1f);
            containerRt.pivot = new Vector2(0.5f, 1f);
            containerRt.anchoredPosition = anchoredPosition;
            containerRt.sizeDelta = new Vector2(ExperienceBarWidth, ExperienceBarHeight);

            var bgGo = new GameObject("Background", typeof(RectTransform));
            bgGo.transform.SetParent(containerGo.transform, false);
            SetStretch(bgGo.GetComponent<RectTransform>());
            bgGo.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);

            float segmentsAreaWidth = ExperienceBarWidth - ExperienceBarLevelZoneWidth;
            float segmentWidth = (segmentsAreaWidth - (count - 1) * ExperienceSegmentGap) / count;
            var segments = new Image[count];

            for (int i = 0; i < count; i++)
            {
                var segGo = new GameObject($"Segment_{i}", typeof(RectTransform));
                segGo.transform.SetParent(containerGo.transform, false);
                var segRt = segGo.GetComponent<RectTransform>();
                segRt.anchorMin = new Vector2(0f, 0f);
                segRt.anchorMax = new Vector2(0f, 1f);
                segRt.pivot = new Vector2(0f, 0.5f);
                segRt.anchoredPosition = new Vector2(i * (segmentWidth + ExperienceSegmentGap) + ExperienceBarHeight * 0.1f, 0f);
                segRt.sizeDelta = new Vector2(segmentWidth, -ExperienceBarHeight * 0.2f);

                segments[i] = segGo.AddComponent<Image>();
            }

            var levelTextGo = CreateUIText(containerGo.transform, "LevelText", "Lv. 1", Vector2.zero, new Vector2(ExperienceBarLevelZoneWidth - 20f, ExperienceBarHeight));
            var levelTextRt = levelTextGo.GetComponent<RectTransform>();
            levelTextRt.anchorMin = new Vector2(1f, 0.5f);
            levelTextRt.anchorMax = new Vector2(1f, 0.5f);
            levelTextRt.pivot = new Vector2(1f, 0.5f);
            levelTextRt.anchoredPosition = new Vector2(-10f, 0f);
            var levelText = levelTextGo.GetComponent<Text>();
            levelText.alignment = TextAnchor.MiddleRight;
            levelText.fontStyle = FontStyle.Bold;
            levelText.fontSize = 32;

            return (segments, levelText);
        }

        // --- Health dots -----------------------------------------------------
        private const int MaxHealthDots = 5;
        private const int HealthDotPixels = 16;
        private const float HealthDotSize = 48f;
        private const float HealthDotSpacing = 56f;

        private static Image[] CreateHealthDots(Transform parent, Sprite dotSprite, Vector2 anchoredPosition, int count)
        {
            var containerGo = new GameObject("HealthDots", typeof(RectTransform));
            containerGo.transform.SetParent(parent, false);
            var containerRt = containerGo.GetComponent<RectTransform>();
            containerRt.anchorMin = new Vector2(0f, 1f);
            containerRt.anchorMax = new Vector2(0f, 1f);
            containerRt.pivot = new Vector2(0f, 1f);
            containerRt.anchoredPosition = anchoredPosition;
            containerRt.sizeDelta = new Vector2(count * HealthDotSpacing, HealthDotSize);

            var dots = new Image[count];
            for (int i = 0; i < count; i++)
            {
                var dotGo = new GameObject($"Dot_{i}", typeof(RectTransform));
                dotGo.transform.SetParent(containerGo.transform, false);
                var dotRt = dotGo.GetComponent<RectTransform>();
                dotRt.anchorMin = new Vector2(0f, 1f);
                dotRt.anchorMax = new Vector2(0f, 1f);
                dotRt.pivot = new Vector2(0f, 1f);
                dotRt.anchoredPosition = new Vector2(i * HealthDotSpacing, 0f);
                dotRt.sizeDelta = new Vector2(HealthDotSize, HealthDotSize);

                var dotImg = dotGo.AddComponent<Image>();
                dotImg.sprite = dotSprite;
                dotImg.color = new Color(0.85f, 0.15f, 0.15f);

                dots[i] = dotImg;
            }

            return dots;
        }

        // Filled circle with alpha-cut edges, tinted per-dot at runtime via
        // Image.color to show the filled/empty health states.
        private static Color HealthDotPixel(int x, int y)
        {
            float center = (HealthDotPixels - 1) / 2f;
            float radius = HealthDotPixels / 2f - 0.5f;
            float dist = new Vector2(x - center, y - center).magnitude;
            return dist <= radius ? Color.white : new Color(1f, 1f, 1f, 0f);
        }

        private static void CreateMoveJoystick(Transform canvasTransform)
        {
            // Full-screen invisible touch zone that receives the press; the
            // joystick base is repositioned to wherever the touch starts.
            // Placed first among the canvas children so HUD and the level-up
            // panel render above it and keep receiving their own clicks.
            var zoneGo = new GameObject("MoveJoystickZone", typeof(RectTransform));
            zoneGo.transform.SetParent(canvasTransform, false);
            zoneGo.transform.SetAsFirstSibling();
            SetStretch(zoneGo.GetComponent<RectTransform>());
            zoneGo.AddComponent<Image>().color = Color.clear;

            var baseGo = new GameObject("MoveJoystick", typeof(RectTransform));
            baseGo.transform.SetParent(zoneGo.transform, false);
            var baseRt = baseGo.GetComponent<RectTransform>();
            baseRt.anchorMin = new Vector2(0.5f, 0.5f);
            baseRt.anchorMax = new Vector2(0.5f, 0.5f);
            baseRt.pivot = new Vector2(0.5f, 0.5f);
            baseRt.anchoredPosition = Vector2.zero;
            baseRt.sizeDelta = new Vector2(260f, 260f);

            var baseImg = baseGo.AddComponent<Image>();
            baseImg.color = new Color(1f, 1f, 1f, 0.25f);
            baseImg.raycastTarget = false;

            var handleGo = new GameObject("Handle", typeof(RectTransform));
            handleGo.transform.SetParent(baseGo.transform, false);
            var handleRt = handleGo.GetComponent<RectTransform>();
            handleRt.anchorMin = new Vector2(0.5f, 0.5f);
            handleRt.anchorMax = new Vector2(0.5f, 0.5f);
            handleRt.pivot = new Vector2(0.5f, 0.5f);
            handleRt.sizeDelta = new Vector2(110f, 110f);
            handleRt.anchoredPosition = Vector2.zero;

            var handleImg = handleGo.AddComponent<Image>();
            handleImg.color = new Color(1f, 1f, 1f, 0.6f);
            handleImg.raycastTarget = false;

            var moveController = zoneGo.AddComponent<MoveController>();
            var moveControllerSO = new SerializedObject(moveController);
            moveControllerSO.FindProperty("background").objectReferenceValue = baseRt;
            moveControllerSO.FindProperty("handle").objectReferenceValue = handleRt;
            moveControllerSO.FindProperty("handleRange").floatValue = 80f;
            moveControllerSO.ApplyModifiedProperties();
        }

        // --- Ground tilemap -------------------------------------------------
        // Muted stone floor so the brightly coloured player / enemy / gem
        // placeholders stay readable on top of it.
        private const string TileFolder = ArtFolder + "/Tiles";
        private const int TilePixels = 16;

        private static readonly Vector2Int[] CrackPixels =
        {
            new Vector2Int(3, 12), new Vector2Int(4, 11), new Vector2Int(5, 11), new Vector2Int(6, 10),
            new Vector2Int(7, 9), new Vector2Int(8, 9), new Vector2Int(9, 8), new Vector2Int(9, 7),
            new Vector2Int(10, 6), new Vector2Int(11, 5), new Vector2Int(12, 5), new Vector2Int(8, 10),
        };

        private static void CreateGroundTilemap(Camera cam)
        {
            var stone = new Color(0.30f, 0.29f, 0.33f);
            var darkStone = Shade(stone, 0.85f);
            var moss = new Color(0.27f, 0.38f, 0.24f);

            var tiles = new TileBase[]
            {
                CreateGroundTile("GroundStone", (x, y) => StonePixel(x, y, stone, 1)),
                CreateGroundTile("GroundStoneDark", (x, y) => StonePixel(x, y, darkStone, 2)),
                CreateGroundTile("GroundStoneCracked", (x, y) =>
                    Array.IndexOf(CrackPixels, new Vector2Int(x, y)) >= 0 ? Shade(stone, 0.55f) : StonePixel(x, y, stone, 3)),
                CreateGroundTile("GroundStoneMossy", (x, y) =>
                {
                    bool inPatch = new Vector2(x - 6f, y - 9f).sqrMagnitude < 22f && Noise(x, y, 4) < 0.7f;
                    return inPatch ? Shade(moss, 0.9f + Noise(x, y, 5) * 0.2f) : StonePixel(x, y, stone, 6);
                }),
            };
            float[] weights = { 60f, 25f, 8f, 7f };

            var gridGo = new GameObject("Grid");
            gridGo.AddComponent<Grid>();

            var groundGo = new GameObject("Ground");
            groundGo.transform.SetParent(gridGo.transform, false);
            groundGo.AddComponent<Tilemap>();
            var tilemapRenderer = groundGo.AddComponent<TilemapRenderer>();
            tilemapRenderer.sharedMaterial = GetSpriteMaterial();
            tilemapRenderer.sortingOrder = -100;

            var background = groundGo.AddComponent<InfiniteTilemapBackground>();
            var so = new SerializedObject(background);
            so.FindProperty("targetCamera").objectReferenceValue = cam;
            var tilesProp = so.FindProperty("tiles");
            var weightsProp = so.FindProperty("weights");
            tilesProp.arraySize = tiles.Length;
            weightsProp.arraySize = weights.Length;
            for (int i = 0; i < tiles.Length; i++)
            {
                tilesProp.GetArrayElementAtIndex(i).objectReferenceValue = tiles[i];
                weightsProp.GetArrayElementAtIndex(i).floatValue = weights[i];
            }
            so.ApplyModifiedProperties();

            // Pre-paint around the start position so the scene view shows the floor.
            background.Refresh(cam);
        }

        private static Tile CreateGroundTile(string name, Func<int, int, Color> pixelAt)
        {
            var sprite = CreateAndSavePixelSprite(TileFolder, name + "Sprite", TilePixels, pixelAt);

            string path = $"{TileFolder}/{name}.asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, path);
            }

            tile.sprite = sprite;
            tile.color = Color.white;
            tile.colliderType = Tile.ColliderType.None;
            EditorUtility.SetDirty(tile);
            return tile;
        }

        // Slightly noisy fill with a dark grout line on the bottom/left edge
        // and a faint highlight on the top/right, so the grid reads clearly
        // and movement is visible against the floor.
        private static Color StonePixel(int x, int y, Color baseColor, int seed)
        {
            int last = TilePixels - 1;
            if (x == 0 || y == 0)
            {
                return Shade(baseColor, 0.7f);
            }
            if (x == last || y == last)
            {
                return Shade(baseColor, 1.1f);
            }
            return Shade(baseColor, 0.94f + Noise(x, y, seed) * 0.12f);
        }

        private static Color Shade(Color c, float factor)
        {
            return new Color(c.r * factor, c.g * factor, c.b * factor, c.a);
        }

        private static float Noise(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)x * 374761393u + (uint)y * 668265263u + (uint)seed * 2246822519u;
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFF) / 65536f;
            }
        }

        private static void SetStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static (Button button, Text label) CreateButton(Transform parent, string name, Vector2 anchoredPosition, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = size;

            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.25f, 0.95f);

            var button = go.AddComponent<Button>();
            button.targetGraphic = img;

            var labelGo = CreateUIText(go.transform, "Label", name, Vector2.zero);
            SetStretch(labelGo.GetComponent<RectTransform>());
            var labelText = labelGo.GetComponent<Text>();
            labelText.alignment = TextAnchor.MiddleCenter;

            return (button, labelText);
        }

        private static void ApplyEnemyLayer(this SerializedObject so, int enemyLayer)
        {
            so.FindProperty("enemyLayer").intValue = 1 << enemyLayer;
            so.ApplyModifiedProperties();
        }

        private static void ApplyLootLayer(this SerializedObject so, int lootLayer)
        {
            so.FindProperty("lootLayer").intValue = 1 << lootLayer;
            so.ApplyModifiedProperties();
        }

        private static void ApplySpriteRenderer(this SerializedObject so, SpriteRenderer spriteRenderer)
        {
            so.FindProperty("spriteRenderer").objectReferenceValue = spriteRenderer;
            so.ApplyModifiedProperties();
        }

        private static void ApplyTimeText(this SerializedObject so, Text timeText)
        {
            so.FindProperty("timeText").objectReferenceValue = timeText;
            so.ApplyModifiedProperties();
        }

        internal static GameObject CreateUIText(Transform parent, string name, string text, Vector2 anchoredPosition, Vector2? sizeDelta = null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta ?? new Vector2(300f, 40f);

            var txt = go.AddComponent<Text>();
            txt.text = text;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 24;
            txt.alignment = TextAnchor.UpperLeft;
            txt.color = Color.white;

            return go;
        }

        internal static GameObject CreatePanel(Transform parent, string name, string label)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.6f);

            var textGo = CreateUIText(go.transform, "Label", label, Vector2.zero);
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0.5f, 0.5f);
            textRt.anchorMax = new Vector2(0.5f, 0.5f);
            textRt.pivot = new Vector2(0.5f, 0.5f);
            textRt.anchoredPosition = Vector2.zero;
            textRt.sizeDelta = new Vector2(400f, 80f);

            var txt = textGo.GetComponent<Text>();
            txt.fontSize = 48;
            txt.alignment = TextAnchor.MiddleCenter;

            return go;
        }

        // Builds the sprite as a native asset (texture + sprite sub-asset) via
        // AssetDatabase.CreateAsset instead of writing a PNG and waiting on the
        // TextureImporter pipeline — the PNG route depends on the async asset
        // worker and was unreliable under -executeMethod batch runs.
        internal static Sprite CreateAndSaveSquareSprite(string name, Color color)
        {
            return CreateAndSavePixelSprite(ArtFolder, name, 4, (x, y) => color);
        }

        // Generic 1-world-unit sprite whose pixels come from pixelAt(x, y).
        private static Sprite CreateAndSavePixelSprite(string folder, string name, int size, Func<int, int, Color> pixelAt)
        {
            EnsureFolder(folder);

            string path = $"{folder}/{name}.asset";

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.name = name;
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    pixels[y * size + x] = pixelAt(x, y);
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();

            var sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.name = name;

            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            AssetDatabase.CreateAsset(texture, path);
            AssetDatabase.AddObjectToAsset(sprite, texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);

            var loaded = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (loaded == null)
            {
                Debug.LogError($"Failed to load generated sprite at '{path}' after import.");
            }

            return loaded;
        }

        // URP's "Universal Renderer" (as opposed to the 2D Renderer) does not
        // process 2D lights, so the default Sprite-Lit-Default material this
        // project's SpriteRenderers pick up renders pure black. Force an
        // unlit shader so placeholder sprites are visible regardless of the
        // active renderer/lighting setup. Saved as a real asset (rather than
        // a scene-embedded material) so prefabs can share a single GUID
        // reference instead of each embedding their own copy.
        private const string SpriteMaterialPath = ArtFolder + "/SpriteUnlit.mat";
        private static Material spriteMaterial;

        internal static Material GetSpriteMaterial()
        {
            if (spriteMaterial != null)
            {
                return spriteMaterial;
            }

            spriteMaterial = AssetDatabase.LoadAssetAtPath<Material>(SpriteMaterialPath);
            if (spriteMaterial != null)
            {
                return spriteMaterial;
            }

            if (!AssetDatabase.IsValidFolder(ArtFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Art");
            }

            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")
                ?? Shader.Find("Sprites/Default");

            spriteMaterial = new Material(shader) { name = "SpriteUnlit" };
            AssetDatabase.CreateAsset(spriteMaterial, SpriteMaterialPath);
            return spriteMaterial;
        }
    }
}
