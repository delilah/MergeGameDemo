using UnityEngine;
using MergeGame.MergeDebug;

namespace MergeGame.Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Game/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Gameplay")]
        public int itemsToWin = 7;
        public int maxEnergy = 100;
        public float regenEnergyTime = 60f; // in seconds

        [Header("SFX")]
        public SfxConfig sfx;

        [Header("Music")]
        public AudioClip backgroundMusic;
        public float musicVolume = 0.5f;
        public float sfxVolume = 1f;

        [Header("Visuals")]
        public Sprite backgroundSprite;
        public Color backgroundColor = Color.white;

        [Header("Spawner")]
        public Vector2Int spawnerPlacementStart = new Vector2Int(3, 3);

        [Header("Values")]
        public string itemsParentName = "Items";
        public string tilesParentName = "Tiles";
        public int dragSortingOrderOffset = 20;
        public int targetFrameRate = 60;
        

        [Header("Cat Collection")]
        public CatCollectionConfig catCollectionConfig;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (itemsToWin <= 0)
            {
                DebugController.LogWarning("GameConfig: itemsToWin should be greater than zero.");
            }

            if (maxEnergy <= 0)
            {
                DebugController.LogWarning("GameConfig: maxEnergy should be greater than zero.");
            }

            if (regenEnergyTime <= 0f)
            {
                DebugController.LogWarning("GameConfig: regenEnergyTime should be greater than zero.");
            }

            if (sfx == null)
            {
                DebugController.LogWarning("GameConfig: sfx is not initialized.");
            }

            if (backgroundMusic == null)
            {
                DebugController.LogWarning("GameConfig: backgroundMusic is not assigned.");
            }

            if (musicVolume < 0f || musicVolume > 1f)
            {
                DebugController.LogWarning("GameConfig: musicVolume should be between 0 and 1.");
            }

            if (sfxVolume < 0f || sfxVolume > 1f)
            {
                DebugController.LogWarning("GameConfig: sfxVolume should be between 0 and 1.");
            }
            
            if (catCollectionConfig == null)
            {
                DebugController.LogWarning("GameConfig: catCollectionConfig is not assigned.");
            }

            
        }
#endif
    }

    [System.Serializable]
    public class SfxConfig
    {
        public AudioClip spawnerClick;
        public AudioClip merge;
        public AudioClip mergeItems;
        public AudioClip mergeKittens;
        public AudioClip collect;
    }
}