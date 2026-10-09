using UnityEditor;
using UnityEngine;

public static class BossPrefabCreator
{
    private const string EnemyFolder = "Assets/Prefab/Enemies";
    private const string BossFolder = "Assets/Prefab/Enemies/Bosses";

    [MenuItem("Tools/Enemies/Create Boss Prefab Variants")]
    public static void CreateBossVariants()
    {
        if (!AssetDatabase.IsValidFolder(BossFolder))
        {
            AssetDatabase.CreateFolder(EnemyFolder, "Bosses");
        }

        foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { EnemyFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.StartsWith(BossFolder))
            {
                continue;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab.GetComponent<EnemyController>() == null)
            {
                continue;
            }

            // Saving a prefab instance as a new asset creates a prefab variant of the original.
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            var serialized = new SerializedObject(instance.GetComponent<EnemyController>());
            serialized.FindProperty("isBoss").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            string bossPath = $"{BossFolder}/Boss{prefab.name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(instance, bossPath);
            Object.DestroyImmediate(instance);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
