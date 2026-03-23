using UnityEngine;

namespace MergeGame.Data
{
    [CreateAssetMenu(menuName = "Data/Spawner Data")]
    public class SpawnerData : ScriptableObject
    {
        public string spawnerName;
        public Sprite sprite;
        public MergeItemData[] spawnableItems;

        [Header("Spawn Rate")]
        public float spawnInterval = 0.2f;

        [Header("Exhaustion")]
        public int maxSpawnCount = 20;
        public float rechargeDuration = 10f;
    }
}

