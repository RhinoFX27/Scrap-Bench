#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ScrapBench.Weapons;

namespace ScrapBench.Codex.Editor
{
    // OPTIONAL: requires the MOD-6 attachment scripts.
    // If you haven't added those yet, delete this file.
    //
    // Menu: Scrap-Bench > Codex > Import Attachments Into Codex
    // Creates one Items entry per AttachmentData asset, using the SAME id,
    // so picking up an attachment can unlock its page with the same id.
    public static class CodexAttachmentImporter
    {
        [MenuItem("Scrap-Bench/Codex/Import Attachments Into Codex")]
        public static void Import()
        {
            string[] dbGuids = AssetDatabase.FindAssets("t:CodexDatabase");
            if (dbGuids.Length == 0)
            {
                Debug.LogWarning("No CodexDatabase found. Run Scrap-Bench > Codex > Create Codex Database first.");
                return;
            }

            var db = AssetDatabase.LoadAssetAtPath<CodexDatabase>(AssetDatabase.GUIDToAssetPath(dbGuids[0]));

            int count = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:AttachmentData"))
            {
                var a = AssetDatabase.LoadAssetAtPath<AttachmentData>(AssetDatabase.GUIDToAssetPath(guid));
                if (a == null || string.IsNullOrEmpty(a.Id)) continue;

                string longDesc =
                    $"Increases {a.Stat} by {a.BaseValue * 100f:0.#}% at Common, Tier 1. " +
                    "The bonus grows with rarity and tier. " +
                    "Several attachments that boost the same stat add together.";

                var entry = CodexAssetCreator.CreateEntry(a.Id, CodexCategory.Items, a.DisplayName,
                    a.Description, longDesc, "", a.Icon);
                CodexAssetCreator.AddToDatabase(db, entry);
                count++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Imported {count} attachment(s) into the Codex.");
        }
    }
}
#endif
