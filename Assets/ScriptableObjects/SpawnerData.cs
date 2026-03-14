using UnityEngine;

namespace MergeGame.Data
{
    [CreateAssetMenu(menuName = "Data/Spawner Data")]
    public class SpawnerData : ScriptableObject
    {
        public string spawnerName;
        public Sprite sprite;
        public MergeItemData[] spawnableItems;
        public float spawnCooldown = 1f;
    }
}
