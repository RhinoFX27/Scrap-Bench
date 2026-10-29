#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ScrapBench.Weapons;

namespace ScrapBench.Combat.Editor
{
    // Menu: Scrap-Bench > Combat > Create Test Setup
    // Builds starter data, prefabs, a Player and an EnemySpawner in the OPEN scene.
    // Uses Unity's built-in circle sprite as a placeholder until real art (MOD-10/11) is ready.
    public static class CombatSetupTool
    {
        private const string DataFolder = "Assets/ScriptableObjects/Combat";
        private const string PrefabFolder = "Assets/Prefabs/Combat";

        [MenuItem("Scrap-Bench/Combat/Create Test Setup")]
        public static void CreateTestSetup()
        {
            if (GameObject.FindWithTag("Player") != null)
            {
                Debug.LogWarning("[Combat] A GameObject tagged 'Player' already exists in this scene. " +
                                 "Delete or untag it first so there is only one player.");
                return;
            }

            EnsureFolder(DataFolder);
            EnsureFolder(PrefabFolder);

            var sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");

            // ---------- Weapon data ----------
            var pistol = MakeAsset<WeaponData>(DataFolder + "/Pistol.asset", so =>
            {
                Set(so, "id", "pistol");
                Set(so, "displayName", "Scrap Pistol");
                Set(so, "baseDamage", 10f);
                Set(so, "baseFireRate", 4f);
                Set(so, "baseAccuracy", 0.85f);
                Set(so, "baseCritChance", 0.05f);
                Set(so, "baseCritDamage", 1.5f);
                Set(so, "basePenetration", 0f);
                Set(so, "projectilesPerShot", 1);
                Set(so, "projectileSpeed", 14f);
                Set(so, "projectileLifetime", 1.5f);
                Set(so, "maxSpreadDegrees", 20f);
            });

            MakeAsset<WeaponData>(DataFolder + "/Shotgun.asset", so =>
            {
                Set(so, "id", "shotgun");
                Set(so, "displayName", "Scrap Shotgun");
                Set(so, "baseDamage", 6f);
                Set(so, "baseFireRate", 1.2f);
                Set(so, "baseAccuracy", 0.55f);
                Set(so, "baseCritChance", 0.05f);
                Set(so, "baseCritDamage", 1.5f);
                Set(so, "basePenetration", 0f);
                Set(so, "projectilesPerShot", 6);
                Set(so, "projectileSpeed", 12f);
                Set(so, "projectileLifetime", 0.7f);
                Set(so, "maxSpreadDegrees", 30f);
            });

            // ---------- Enemy data ----------
            var drone = MakeEnemyData("scrap_drone", "Scrap Drone", 30f, 0f, 2.5f, 8f, 1f, 1, 3);
            var rusher = MakeEnemyData("rusher", "Rusher", 15f, 0f, 4.5f, 5f, 0.8f, 1, 2);
            var brute = MakeEnemyData("scrap_brute", "Scrap Brute", 120f, 0.4f, 1.6f, 20f, 1.5f, 5, 10);

            // ---------- Prefabs ----------
            var bullet = MakeBulletPrefab(sprite);
            var droneP = MakeEnemyPrefab("ScrapDrone", drone, sprite, new Color(0.9f, 0.4f, 0.3f), 0.9f);
            var rusherP = MakeEnemyPrefab("Rusher", rusher, sprite, new Color(0.95f, 0.8f, 0.2f), 0.7f);
            var bruteP = MakeEnemyPrefab("ScrapBrute", brute, sprite, new Color(0.5f, 0.2f, 0.6f), 1.5f);

            // ---------- Player (in scene) ----------
            var player = new GameObject("Player", typeof(SpriteRenderer), typeof(Rigidbody2D),
                typeof(CircleCollider2D), typeof(Health), typeof(Wallet), typeof(WeaponAttachments),
                typeof(Weapon), typeof(PlayerController), typeof(CombatHUD));
            player.tag = "Player";

            var sr = player.GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.3f, 0.8f, 1f);
            player.transform.localScale = Vector3.one * 1.0f;

            var rb = player.GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            player.GetComponent<CircleCollider2D>().radius = 0.4f;

            var muzzle = new GameObject("Muzzle", typeof(SpriteRenderer));
            muzzle.transform.SetParent(player.transform, false);
            muzzle.transform.localPosition = new Vector3(0.6f, 0f, 0f);
            muzzle.transform.localScale = Vector3.one * 0.25f;
            var msr = muzzle.GetComponent<SpriteRenderer>();
            msr.sprite = sprite;
            msr.color = Color.white;

            SetOn(player.GetComponent<Health>(), so => { Set(so, "maxHealth", 100f); });
            SetOn(player.GetComponent<Weapon>(), so =>
            {
                Set(so, "data", pistol);
                Set(so, "projectilePrefab", bullet);
                Set(so, "muzzle", muzzle.transform);
                Set(so, "attachments", player.GetComponent<WeaponAttachments>());
            });

            Undo.RegisterCreatedObjectUndo(player, "Create Player");

            // ---------- Spawner (in scene) ----------
            var spawner = new GameObject("EnemySpawner", typeof(EnemySpawner));
            var sSo = new SerializedObject(spawner.GetComponent<EnemySpawner>());
            var list = sSo.FindProperty("enemyPrefabs");
            Enemy[] prefabs = { droneP, rusherP, bruteP };
            list.arraySize = prefabs.Length;
            for (int i = 0; i < prefabs.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = prefabs[i];
            sSo.ApplyModifiedPropertiesWithoutUndo();
            Undo.RegisterCreatedObjectUndo(spawner, "Create Enemy Spawner");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Selection.activeGameObject = player;
            Debug.Log("[Combat] Test setup created. Press Play: WASD to move, mouse to aim, hold left click to shoot.");
        }

        // ---------------- Builders ----------------

        private static EnemyData MakeEnemyData(string id, string name, float hp, float armor, float speed,
            float contactDmg, float cooldown, int scrapMin, int scrapMax)
        {
            return MakeAsset<EnemyData>($"{DataFolder}/{name.Replace(" ", "")}.asset", so =>
            {
                Set(so, "id", id);
                Set(so, "displayName", name);
                Set(so, "maxHealth", hp);
                Set(so, "armor", armor);
                Set(so, "moveSpeed", speed);
                Set(so, "contactDamage", contactDmg);
                Set(so, "contactCooldown", cooldown);
                Set(so, "scrapMin", scrapMin);
                Set(so, "scrapMax", scrapMax);
            });
        }

        private static Projectile MakeBulletPrefab(Sprite sprite)
        {
            string path = PrefabFolder + "/Bullet.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing.GetComponent<Projectile>();

            var go = new GameObject("Bullet", typeof(SpriteRenderer), typeof(Rigidbody2D),
                typeof(CircleCollider2D), typeof(Projectile));
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(1f, 0.9f, 0.3f);
            go.transform.localScale = Vector3.one * 0.25f;

            var rb = go.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            var col = go.GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<Projectile>();
        }

        private static Enemy MakeEnemyPrefab(string name, EnemyData data, Sprite sprite, Color color, float scale)
        {
            string path = $"{PrefabFolder}/{name}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing.GetComponent<Enemy>();

            var go = new GameObject(name, typeof(SpriteRenderer), typeof(Rigidbody2D),
                typeof(CircleCollider2D), typeof(Health), typeof(Enemy));
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            go.transform.localScale = Vector3.one * scale;

            var rb = go.GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            go.GetComponent<CircleCollider2D>().radius = 0.45f;

            SetOn(go.GetComponent<Enemy>(), so => { Set(so, "data", data); });

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<Enemy>();
        }

        // ---------------- Helpers ----------------

        private static T MakeAsset<T>(string path, System.Action<SerializedObject> init) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;

            var asset = ScriptableObject.CreateInstance<T>();
            var so = new SerializedObject(asset);
            init(so);
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void SetOn(Object target, System.Action<SerializedObject> init)
        {
            var so = new SerializedObject(target);
            init(so);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Set(SerializedObject so, string field, object value)
        {
            var p = so.FindProperty(field);
            if (p == null)
            {
                Debug.LogError($"[Combat] Field '{field}' not found on {so.targetObject.GetType().Name}.");
                return;
            }

            switch (value)
            {
                case float f: p.floatValue = f; break;
                case int i:
                    if (p.propertyType == SerializedPropertyType.Float) p.floatValue = i;
                    else p.intValue = i;
                    break;
                case string s: p.stringValue = s; break;
                case Object o: p.objectReferenceValue = o; break;
            }
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
