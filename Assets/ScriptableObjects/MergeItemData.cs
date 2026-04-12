#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using MergeGame.MergeDebug;

namespace MergeGame.Data
{
    [CreateAssetMenu(menuName = "Data/Merge Item Data")]
    public class MergeItemData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField, HideInInspector] private string _id;
        public string Id => _id;

        [Header("Item Data")]
        public string itemName;
        public int level;
        public Sprite sprite;
        public MergeItemData nextItem;
        public bool isFinal;

        [Header("Audio")]
        public AudioClip mergeSound;
        [Range(-3f, 3f)] public float mergeSoundPitch = 1f;

#if UNITY_EDITOR
        private void OnValidate()
        {
             if (string.IsNullOrEmpty(_id))
            {
                _id = GUID.Generate().ToString();
                EditorUtility.SetDirty(this);
            }

            if (isFinal && nextItem != null)
            {
                DebugController.LogWarning($"MergeItemData '{itemName}': isFinal is true but nextItem is assigned. Clear nextItem or uncheck isFinal.");
            }

            if (level < 0)
            {
                DebugController.LogWarning($"MergeItemData '{itemName}': level should not be negative.");
            }

            if (string.IsNullOrEmpty(itemName))
            {
                DebugController.LogWarning($"MergeItemData on '{name}': itemName is empty.");
            }
        }
#endif
    }
}