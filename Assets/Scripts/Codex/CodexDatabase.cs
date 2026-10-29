using System.Collections.Generic;
using UnityEngine;

namespace ScrapBench.Codex
{
    // A single list of every codex entry in the game.
    // Create via: Right-click > Create > Scrap-Bench > Codex Database
    [CreateAssetMenu(fileName = "CodexDatabase", menuName = "Scrap-Bench/Codex Database")]
    public class CodexDatabase : ScriptableObject
    {
        [SerializeField] private List<CodexEntry> entries = new List<CodexEntry>();

        public IReadOnlyList<CodexEntry> Entries => entries;

        public CodexEntry GetById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var e in entries)
                if (e != null && e.Id == id) return e;
            return null;
        }

        public List<CodexEntry> GetByCategory(CodexCategory category)
        {
            var result = new List<CodexEntry>();
            foreach (var e in entries)
                if (e != null && e.Category == category) result.Add(e);
            return result;
        }
    }
}
