#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ScrapBench.Weapons.Editor
{
    // Menu: Scrap-Bench > Create Starter Attachments
    // Generates the 7 first-pass attachment assets in Assets/ScriptableObjects/Attachments
    public static class AttachmentAssetCreator
    {
        private const string Folder = "Assets/ScriptableObjects/Attachments";

        [MenuItem("Scrap-Bench/Create Starter Attachments")]
        public static void CreateStarterAttachments()
        {
            EnsureFolder("Assets", "ScriptableObjects");
            EnsureFolder("Assets/ScriptableObjects", "Attachments");

            Make("suppressor",   "Suppressor",            "Muffles the shot and sharpens critical hits.",          StatType.CritDamage,  0.10f);
            Make("sight",        "Sight",                 "Improves aim precision.",                               StatType.Accuracy,    0.08f);
            Make("ext_mag",      "Extended Mag",          "Faster sustained fire.",                                StatType.FireRate,    0.06f);
            Make("ap_rounds",    "Armor Piercing Rounds", "Rounds that punch through armor.",                      StatType.Penetration, 0.10f);
            Make("buckshot",     "Buckshot",              "Chance to fire extra pellets.",                         StatType.Multishot,   0.10f);
            Make("hollow_point", "Hollow Point",          "Expanding rounds that deal more damage.",               StatType.Damage,      0.10f);
            Make("ub_laser",     "Under Barrel Laser",    "Highlights weak points for more critical hits.",        StatType.CritChance,  0.05f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Starter attachments created in " + Folder);
        }

        private static void Make(string id, string name, string desc, StatType stat, float baseValue)
        {
            string path = $"{Folder}/{name}.asset";
            if (AssetDatabase.LoadAssetAtPath<AttachmentData>(path) != null) return; // don't overwrite

            var asset = ScriptableObject.CreateInstance<AttachmentData>();
            var so = new SerializedObject(asset);
            so.FindProperty("id").stringValue = id;
            so.FindProperty("displayName").stringValue = name;
            so.FindProperty("description").stringValue = desc;
            so.FindProperty("stat").enumValueIndex = (int)stat;
            so.FindProperty("baseValue").floatValue = baseValue;
            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(asset, path);
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{child}"))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
#endif
