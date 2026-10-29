using UnityEngine;

namespace ScrapBench.Codex
{
    // One asset per codex page.
    // Create via: Right-click > Create > Scrap-Bench > Codex Entry
    [CreateAssetMenu(fileName = "NewCodexEntry", menuName = "Scrap-Bench/Codex Entry")]
    public class CodexEntry : ScriptableObject
    {
        [Tooltip("Unique id used to unlock this entry from code. For items, use the same id as the AttachmentData.")]
        [SerializeField] private string id;
        [SerializeField] private CodexCategory category;
        [SerializeField] private string title;
        [Tooltip("One-line summary shown under the title.")]
        [SerializeField] private string shortDescription;
        [Tooltip("Deeper explanation than the in-game tooltip (what it does, how it stacks, etc.).")]
        [SerializeField, TextArea(3, 10)] private string longDescription;
        [Tooltip("Optional lore text shown in italics at the bottom.")]
        [SerializeField, TextArea(3, 10)] private string lore;
        [SerializeField] private Sprite icon;

        public string Id => id;
        public CodexCategory Category => category;
        public string Title => title;
        public string ShortDescription => shortDescription;
        public string LongDescription => longDescription;
        public string Lore => lore;
        public Sprite Icon => icon;
    }
}
