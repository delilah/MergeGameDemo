using UnityEngine;

namespace MergeGame.Data
{
    [CreateAssetMenu(menuName = "Data/Merge Item Data")]
    public class MergeItemData : ScriptableObject
    {
        [Header("Item Data")]
        public string itemName;
        public int level;
        public Sprite sprite;
        public MergeItemData nextItem;
        public bool isFinal;

        [Header("Audio")]
        public AudioClip mergeSound;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (isFinal && nextItem != null)
            {
                Debug.LogWarning($"MergeItemData '{itemName}': isFinal is true but nextItem is assigned. Clear nextItem or uncheck isFinal.");
            }

            if (level < 0)
            {
                Debug.LogWarning($"MergeItemData '{itemName}': level should not be negative.");
            }

            if (string.IsNullOrEmpty(itemName))
            {
                Debug.LogWarning($"MergeItemData on '{name}': itemName is empty.");
            }
        }
#endif
    }
}