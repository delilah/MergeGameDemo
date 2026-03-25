using UnityEngine;

namespace MergeGame.Data
{
    [CreateAssetMenu(menuName = "Data/Cat Collection Config")]
    public class CatCollectionConfig : ScriptableObject
    {
        [Header("Cat Items - Order matters!")]
        public MergeItemData[] trackedItems;
    }
}