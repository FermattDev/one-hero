using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Builds one scene per boss. Levels get harder through boss health, faster enemy fire,
// more obstacles and a growing roster of enemy types (one new type per level).
public static class BossLevelGenerator
{
    private const string EnemyFolder = "Assets/Prefab/Enemies";
    private const string BossFolder = "Assets/Prefab/Enemies/Bosses";
    private const string SceneFolder = "Assets/Scenes/Levels";
    private const string WallSpriteGuid = "311925a002f4447b3a28927169b83ea6";

    private const float HalfWidth = 15f;
    private const float HalfHeight = 8f;
    private const float WallThickness = 1f;
    private const float CameraSize = 10f;

    private static readonly Vector2 PlayerStart = new Vector2(-12f, -6f);

    private class EnemySpawn
    {
        public string Type;
        public Vector2 Position;
        public Action<GameObject> Customize;
    }

    private class LevelDef
    {
        public string Boss;
        public Vector2 BossPosition;
        public Color Floor;
        public Color Wall;
        public List<Rect> Pillars = new List<Rect>();
        public List<EnemySpawn> Enemies = new List<EnemySpawn>();
        public Action<GameObject> CustomizeBoss;
    }

    private static Rect Pillar(float x, float y, float w, float h) => new Rect(x - w / 2f, y - h / 2f, w, h);

    private static EnemySpawn At(string type, float x, float y, Action<GameObject> customize = null) =>
        new EnemySpawn { Type = type, Position = new Vector2(x, y), Customize = customize };

    // Thwomps hang on the top/bottom wall and drop when the player crosses their lane.
    private static Action<GameObject> ThwompLane(bool down)
    {
        return go =>
        {
            SetAll(go, "detectDown", p => p.boolValue = down);
            SetAll(go, "detectUp", p => p.boolValue = !down);
        };
    }

    private static List<LevelDef> BuildLevels()
    {
        return new List<LevelDef>
        {
            // 1: Archer boss + archers.
            new LevelDef
            {
                Boss = "Archer", BossPosition = new Vector2(9, 2),
                Floor = new Color(0.16f, 0.24f, 0.18f), Wall = new Color(0.35f, 0.5f, 0.35f),
                Pillars = { Pillar(-3, 0, 2, 2) },
                Enemies =
                {
                    At("Archer", 0, 5), At("Archer", 0, -5),
                },
            },
            // 2: Chaser boss. New: chasers.
            new LevelDef
            {
                Boss = "Chaser", BossPosition = new Vector2(10, 0),
                Floor = new Color(0.2f, 0.2f, 0.28f), Wall = new Color(0.4f, 0.4f, 0.6f),
                Pillars = { Pillar(-3, 3, 2, 2), Pillar(-3, -3, 2, 2) },
                Enemies =
                {
                    At("Archer", -6, 6), At("Archer", 0, -6), At("Archer", -1, 1),
                    At("Chaser", 3, 6), At("Chaser", 3, -6),
                },
            },
            // 3: Thwomp boss. New: sentries.
            new LevelDef
            {
                Boss = "Thwomp", BossPosition = new Vector2(10, 0),
                Floor = new Color(0.27f, 0.22f, 0.16f), Wall = new Color(0.6f, 0.47f, 0.3f),
                Pillars = { Pillar(-4, 0, 2, 6), Pillar(3, 4, 2, 2), Pillar(3, -4, 2, 2) },
                Enemies =
                {
                    At("Archer", -1, 6), At("Archer", -1, -6),
                    At("Chaser", 0, 0), At("Chaser", 6, -7),
                    At("Sentry", -9, 5), At("Sentry", -9, -1.5f),
                },
            },
            // 4: Sentry boss. New: thwomps.
            new LevelDef
            {
                Boss = "Sentry", BossPosition = new Vector2(10, -5),
                Floor = new Color(0.14f, 0.2f, 0.27f), Wall = new Color(0.3f, 0.5f, 0.65f),
                Pillars = { Pillar(-5, 3, 5, 1.5f), Pillar(-5, -3, 5, 1.5f), Pillar(2, 0, 1.5f, 6) },
                CustomizeBoss = go => SetPatrol(go, new Vector2(0, 10)),
                Enemies =
                {
                    At("Archer", -1, 6), At("Archer", -1, -6),
                    At("Chaser", 5, 7), At("Chaser", 5, -7),
                    At("Sentry", -9, 5), At("Sentry", -9, 0),
                    At("Thwomp", -1, 7.2f, ThwompLane(true)), At("Thwomp", 6, -7.2f, ThwompLane(false)),
                },
            },
            // 5: Spinner boss. New: spinners.
            new LevelDef
            {
                Boss = "Spinner", BossPosition = new Vector2(9, 0),
                Floor = new Color(0.25f, 0.15f, 0.25f), Wall = new Color(0.6f, 0.35f, 0.6f),
                Pillars = { Pillar(-4, 0, 2, 2), Pillar(1, 5, 2, 2), Pillar(1, -5, 2, 2) },
                Enemies =
                {
                    At("Archer", -1, 7), At("Archer", -1, -7),
                    At("Chaser", 4, 3), At("Chaser", 4, -3),
                    At("Sentry", -9, 5), At("Sentry", -9, -1),
                    At("Thwomp", -6, 7.2f, ThwompLane(true)), At("Thwomp", 6, -7.2f, ThwompLane(false)),
                    At("Spinner", -6, 3), At("Spinner", -6, -4),
                },
            },
            // 6: Summoner boss. New: regular summoner.
            new LevelDef
            {
                Boss = "Summoner", BossPosition = new Vector2(11, 0),
                Floor = new Color(0.22f, 0.12f, 0.12f), Wall = new Color(0.65f, 0.3f, 0.3f),
                Pillars = { Pillar(-3, 4, 2, 2), Pillar(-3, -4, 2, 2), Pillar(4, 3, 2, 2), Pillar(4, -3, 2, 2) },
                CustomizeBoss = go =>
                {
                    SetAll(go, "spawnInterval", p => p.floatValue = 5f);
                    SetAll(go, "bossSpawnCount", p => p.intValue = 6);
                    SetAll(go, "bossMaxMinions", p => p.intValue = 12);
                    SetAll(go, "moveBounds", p => p.rectValue = new Rect(-HalfWidth + 3f, -HalfHeight + 2.5f, 2 * (HalfWidth - 3f), 2 * (HalfHeight - 2.5f)));
                },
                Enemies =
                {
                    At("Archer", -2, 7), At("Archer", -2, -7), At("Archer", 8, 6),
                    At("Chaser", 2, 6), At("Chaser", 2, -6),
                    At("Sentry", -9, 5), At("Sentry", -9, 0), At("Sentry", -4, -7),
                    At("Thwomp", -6, 7.2f, ThwompLane(true)), At("Thwomp", 8, -7.2f, ThwompLane(false)),
                    At("Spinner", -5.5f, 0.5f), At("Spinner", 1, 0),
                    At("Summoner", 4, 0),
                },
            },
            // 7: Laser boss. New: laser turrets. Lots of cover to break line of sight.
            new LevelDef
            {
                Boss = "LaserTurret", BossPosition = new Vector2(11, 0),
                Floor = new Color(0.1f, 0.1f, 0.12f), Wall = new Color(0.75f, 0.2f, 0.2f),
                Pillars =
                {
                    Pillar(-5, 4, 2, 2), Pillar(-5, -3, 2, 2), Pillar(0, 0, 2, 6),
                    Pillar(5, 5, 2, 2), Pillar(5, -5, 2, 2),
                },
                Enemies =
                {
                    At("Archer", -2, 7), At("Archer", -2, -7),
                    At("Chaser", 2, 6.5f), At("Chaser", 2, -6.5f),
                    At("Sentry", -9, 6), At("Sentry", -9, 0),
                    At("Thwomp", -5, 7.2f, ThwompLane(true)), At("Thwomp", 7, -7.2f, ThwompLane(false)),
                    At("Spinner", -3, 3), At("Spinner", -3, -5),
                    At("Summoner", 4, 0),
                    At("LaserTurret", -13, 7), At("LaserTurret", 13, -7),
                },
            },
        };
    }

    [MenuItem("Tools/Levels/Generate Boss Levels")]
    public static void GenerateAll()
    {
        EnsureBossPrefabs();

        if (!AssetDatabase.IsValidFolder(SceneFolder))
        {
            AssetDatabase.CreateFolder("Assets/Scenes", "Levels");
        }

        var levels = BuildLevels();
        var scenePaths = levels.Select((l, i) => $"{SceneFolder}/Level_{i + 1}_{SceneLabel(l.Boss)}.unity").ToList();

        for (int i = 0; i < levels.Count; i++)
        {
            string next = i + 1 < levels.Count ? Path.GetFileNameWithoutExtension(scenePaths[i + 1]) : "";
            BuildScene(levels[i], i, levels.Count, scenePaths[i], Path.GetFileNameWithoutExtension(scenePaths[0]), next);
        }

        // Boss levels first, previous scenes kept after them.
        var buildScenes = scenePaths.Select(p => new EditorBuildSettingsScene(p, true)).ToList();
        buildScenes.AddRange(EditorBuildSettings.scenes.Where(s => !scenePaths.Contains(s.path)));
        EditorBuildSettings.scenes = buildScenes.ToArray();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Generated {levels.Count} boss levels in {SceneFolder}");
    }

    private static string SceneLabel(string boss) => boss == "LaserTurret" ? "Laser" : boss;

    private static void EnsureBossPrefabs()
    {
        var missing = new[] { "Archer", "Chaser", "Thwomp", "Sentry", "Spinner", "Summoner", "LaserTurret" }
            .Any(n => AssetDatabase.LoadAssetAtPath<GameObject>($"{BossFolder}/Boss{n}Enemy.prefab") == null);
        if (missing)
        {
            BossPrefabCreator.CreateBossVariants();
        }
    }

    private static void BuildScene(LevelDef level, int index, int total, string scenePath, string firstScene, string nextScene)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var cameraPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Camera/GameCamera.prefab");
        var cameraObject = (GameObject)PrefabUtility.InstantiatePrefab(cameraPrefab);
        cameraObject.transform.position = new Vector3(0, 0, -10);
        var camera = cameraObject.GetComponent<Camera>();
        camera.orthographicSize = CameraSize;
        camera.backgroundColor = level.Floor;

        // Arena
        var map = new GameObject("Map").transform;
        var wallSprite = LoadWallSprite();
        float outerX = HalfWidth + WallThickness / 2f;
        float outerY = HalfHeight + WallThickness / 2f;
        float fullWidth = 2 * (HalfWidth + WallThickness);
        float fullHeight = 2 * HalfHeight;
        CreateWall(map, wallSprite, "Wall Top", new Vector2(0, outerY), new Vector2(fullWidth, WallThickness), level.Wall);
        CreateWall(map, wallSprite, "Wall Bottom", new Vector2(0, -outerY), new Vector2(fullWidth, WallThickness), level.Wall);
        CreateWall(map, wallSprite, "Wall Left", new Vector2(-outerX, 0), new Vector2(WallThickness, fullHeight), level.Wall);
        CreateWall(map, wallSprite, "Wall Right", new Vector2(outerX, 0), new Vector2(WallThickness, fullHeight), level.Wall);

        var floorTint = Color.Lerp(level.Floor, Color.white, 0.06f);
        CreateWall(map, wallSprite, "Floor", Vector2.zero, new Vector2(2 * HalfWidth, 2 * HalfHeight), floorTint, withCollider: false, sortingOrder: -10);

        for (int i = 0; i < level.Pillars.Count; i++)
        {
            var r = level.Pillars[i];
            CreateWall(map, wallSprite, $"Pillar {i + 1}", r.center, r.size, Color.Lerp(level.Wall, Color.black, 0.25f));
        }

        // Player + level manager
        var start = new GameObject("PlayerStartPos").transform;
        start.position = PlayerStart;

        var managerObject = new GameObject("LevelManager");
        var manager = managerObject.AddComponent<LevelManager>();
        var managerSo = new SerializedObject(manager);
        managerSo.FindProperty("playerPrefab").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Characters/Player.prefab").GetComponent<PlayerController>();
        managerSo.FindProperty("ghostPrefab").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Characters/Ghost.prefab").GetComponent<GhostController>();
        managerSo.FindProperty("playerStartPosition").objectReferenceValue = start;
        managerSo.ApplyModifiedPropertiesWithoutUndo();

        // Progression
        var progressionObject = new GameObject("LevelProgression");
        var progression = progressionObject.AddComponent<LevelProgression>();
        var progressionSo = new SerializedObject(progression);
        progressionSo.FindProperty("levelTitle").stringValue = $"Level {index + 1}/{total} - {SceneLabel(level.Boss)} Boss";
        progressionSo.FindProperty("nextSceneName").stringValue = nextScene;
        progressionSo.FindProperty("firstSceneName").stringValue = firstScene;
        progressionSo.ApplyModifiedPropertiesWithoutUndo();

        // Difficulty ramp: 0 on the first level, 1 on the last.
        float t = total > 1 ? (float)index / (total - 1) : 0f;
        int bossHealth = 5 + index;
        float fireCharge = Mathf.Lerp(1f, 0.5f, t);

        // Enemies
        var enemies = new GameObject("Enemies").transform;

        var boss = SpawnEnemy($"{BossFolder}/Boss{level.Boss}Enemy.prefab", level.BossPosition, enemies);
        SetAll(boss, "maxHealth", p => p.intValue = bossHealth);
        SetAll(boss, "fireCharge", p => p.floatValue = fireCharge);
        level.CustomizeBoss?.Invoke(boss);

        foreach (var spawn in level.Enemies)
        {
            var enemy = SpawnEnemy($"{EnemyFolder}/{spawn.Type}Enemy.prefab", spawn.Position, enemies);
            SetAll(enemy, "fireCharge", p => p.floatValue = fireCharge);
            spawn.Customize?.Invoke(enemy);
        }

        EditorSceneManager.SaveScene(scene, scenePath);
    }

    private static Sprite LoadWallSprite()
    {
        string path = AssetDatabase.GUIDToAssetPath(WallSpriteGuid);
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().First();
    }

    private static void CreateWall(Transform parent, Sprite sprite, string name, Vector2 center, Vector2 size, Color color,
        bool withCollider = true, int sortingOrder = 0)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = center;

        Vector2 spriteSize = sprite.bounds.size;
        go.transform.localScale = new Vector3(size.x / spriteSize.x, size.y / spriteSize.y, 1);

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;

        if (withCollider)
        {
            go.AddComponent<BoxCollider2D>().size = spriteSize;
        }
    }

    private static GameObject SpawnEnemy(string prefabPath, Vector2 position, Transform parent)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            throw new FileNotFoundException($"Missing prefab {prefabPath}");
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.transform.SetParent(parent, false);
        instance.transform.position = position;
        return instance;
    }

    // Applies an edit to the serialized field `propertyName` on every component of the object that has it.
    private static void SetAll(GameObject go, string propertyName, Action<SerializedProperty> edit)
    {
        foreach (var component in go.GetComponents<MonoBehaviour>())
        {
            var so = new SerializedObject(component);
            var property = so.FindProperty(propertyName);
            if (property == null)
            {
                continue;
            }

            edit(property);
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void SetPatrol(GameObject go, params Vector2[] offsets)
    {
        SetAll(go, "patrolOffsets", p =>
        {
            p.arraySize = offsets.Length;
            for (int i = 0; i < offsets.Length; i++)
            {
                p.GetArrayElementAtIndex(i).vector2Value = offsets[i];
            }
        });
    }
}
