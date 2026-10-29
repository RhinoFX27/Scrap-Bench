#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ScrapBench.Codex.Editor
{
    // Menu: Scrap-Bench > Codex > Create Codex Database
    // Creates the database asset plus a few placeholder entries (edit or delete them freely).
    public static class CodexAssetCreator
    {
        public const string Folder = "Assets/ScriptableObjects/Codex";

        [MenuItem("Scrap-Bench/Codex/Create Codex Database")]
        public static void CreateDatabase()
        {
            EnsureFolders();

            string dbPath = Folder + "/CodexDatabase.asset";
            var db = AssetDatabase.LoadAssetAtPath<CodexDatabase>(dbPath);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<CodexDatabase>();
                AssetDatabase.CreateAsset(db, dbPath);
            }

            // Placeholder entries so the UI has something to show. Replace with real content.
            AddToDatabase(db, CreateEntry("scrap_drone", CodexCategory.Enemies, "Scrap Drone",
                "A patched-together flying scavenger.",
                "Placeholder: describe the enemy's behavior, attacks and weaknesses here.",
                "Placeholder lore: where these drones came from.", null));

            AddToDatabase(db, CreateEntry("cyberpunk_street", CodexCategory.Areas, "Cyberpunk Street",
                "A neon-lit street at the edge of the scrapyards.",
                "Placeholder: describe the area, what spawns here and what loot to expect.",
                "Placeholder lore: the history of this place.", null));

            AddToDatabase(db, CreateEntry("the_bench", CodexCategory.Lore, "The Bench",
                "Where everything gets rebuilt.",
                "Placeholder: story text goes here.",
                "", null));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = db;
            Debug.Log("Codex database ready at " + dbPath);
        }

        // ---- Helpers (also used by CodexAttachmentImporter) ----

        public static CodexEntry CreateEntry(string id, CodexCategory category, string title,
            string shortDesc, string longDesc, string lore, Sprite icon)
        {
            EnsureFolders();
            string path = $"{Folder}/{id}.asset";

            var existing = AssetDatabase.LoadAssetAtPath<CodexEntry>(path);
            if (existing != null) return existing; // never overwrite hand-edited entries

            var entry = ScriptableObject.CreateInstance<CodexEntry>();
            var so = new SerializedObject(entry);
            so.FindProperty("id").stringValue = id;
            so.FindProperty("category").enumValueIndex = (int)category;
            so.FindProperty("title").stringValue = title;
            so.FindProperty("shortDescription").stringValue = shortDesc;
            so.FindProperty("longDescription").stringValue = longDesc;
            so.FindProperty("lore").stringValue = lore;
            so.FindProperty("icon").objectReferenceValue = icon;
            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(entry, path);
            return entry;
        }

        public static void AddToDatabase(CodexDatabase db, CodexEntry entry)
        {
            var so = new SerializedObject(db);
            var list = so.FindProperty("entries");

            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == entry) return; // already in

            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = entry;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(db);
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Codex");
        }
    }
}
#endif
