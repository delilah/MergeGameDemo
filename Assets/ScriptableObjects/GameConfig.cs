using UnityEngine;

namespace MergeGame.Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Game/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Gameplay")]
        public int itemsToWin = 7;

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

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (itemsToWin <= 0)
            {
                Debug.LogWarning("GameConfig: itemsToWin should be greater than zero.");
            }

            if (sfx == null)
            {
                Debug.LogWarning("GameConfig: sfx is not initialized.");
            }

            if (backgroundMusic == null)
            {
                Debug.LogWarning("GameConfig: backgroundMusic is not assigned.");
            }

            if (musicVolume < 0f || musicVolume > 1f)
            {
                Debug.LogWarning("GameConfig: musicVolume should be between 0 and 1.");
            }

            if (sfxVolume < 0f || sfxVolume > 1f)
            {
                Debug.LogWarning("GameConfig: sfxVolume should be between 0 and 1.");
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