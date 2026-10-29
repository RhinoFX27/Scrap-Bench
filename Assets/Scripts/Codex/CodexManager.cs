using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScrapBench.Codex
{
    // Tracks which entries the player has unlocked and saves them between sessions.
    // Put this on an empty "Codex" GameObject. It survives scene changes.
    public class CodexManager : MonoBehaviour
    {
        private const string SaveKey = "ScrapBench_Codex";

        [SerializeField] private CodexDatabase database;

        private readonly HashSet<string> unlocked = new HashSet<string>();

        public static CodexManager Instance { get; private set; }
        public CodexDatabase Database => database;
        public int UnlockedCount => unlocked.Count;

        public event Action<CodexEntry> EntryUnlocked;

        [Serializable]
        private class SaveData { public List<string> ids = new List<string>(); }

        private void Awake()
        {
            // If the Codex object exists in several scenes, only the first one survives.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        // Safe to call from anywhere, even if no Codex exists in the scene.
        public static void TryUnlock(string id)
        {
            if (Instance != null) Instance.Unlock(id);
        }

        public bool Unlock(string id)
        {
            if (database == null) return false;

            var entry = database.GetById(id);
            if (entry == null)
            {
                Debug.LogWarning($"[Codex] No entry with id '{id}' in the database.");
                return false;
            }

            if (!unlocked.Add(id)) return false; // already unlocked

            Save();
            Debug.Log($"[Codex] Unlocked: {entry.Title}");
            EntryUnlocked?.Invoke(entry);
            return true;
        }

        public bool IsUnlocked(string id)
        {
            return !string.IsNullOrEmpty(id) && unlocked.Contains(id);
        }

        private void Save()
        {
            var data = new SaveData { ids = new List<string>(unlocked) };
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        private void Load()
        {
            unlocked.Clear();
            string json = PlayerPrefs.GetString(SaveKey, "");
            if (string.IsNullOrEmpty(json)) return;

            var data = JsonUtility.FromJson<SaveData>(json);
            if (data == null || data.ids == null) return;
            foreach (var id in data.ids) unlocked.Add(id);
        }

        // ---- Debug helpers: right-click the component header while in Play mode ----

        [ContextMenu("Debug: Unlock All")]
        private void DebugUnlockAll()
        {
            if (database == null) return;
            foreach (var e in database.Entries)
                if (e != null) Unlock(e.Id);
        }

        [ContextMenu("Debug: Reset Progress")]
        private void DebugReset()
        {
            unlocked.Clear();
            PlayerPrefs.DeleteKey(SaveKey);
            Debug.Log("[Codex] Progress reset.");
        }
    }
}
