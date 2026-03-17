using UnityEngine;
using Zenject;
using MergeGame.Data;

namespace MergeGame.Systems
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _musicSource;

        private CollectionManager _collectionManager;
        private GameConfig _config;

        public enum MergeCategory { Items, Kittens, Generic }

        [Inject]
        public void Construct(GameConfig gameConfig, CollectionManager collectionManager)
        {
            _config = gameConfig;
            _collectionManager = collectionManager;
        }

        private void Start()
        {
            if (_collectionManager != null)
            {
                _collectionManager.OnItemCollected += HandleItemCollected;
            }

            PlayMusic();
        }

        private void OnDestroy()
        {
            if (_collectionManager != null)
            {
                _collectionManager.OnItemCollected -= HandleItemCollected;
            }
        }

        /// <summary>
        /// Plays a spawner click sound.
        /// </summary>
        public void PlaySpawnerClick() => PlaySfx(_config?.sfx?.spawnerClick);

        /// <summary>
        /// Plays a collect sound.
        /// </summary>
        public void PlayCollect() => PlaySfx(_config?.sfx?.collect);

        /// <summary>
        /// Plays a merge sound.
        /// </summary>
        public void PlayMerge() => PlaySfx(_config?.sfx?.merge);

        /// <summary>
        /// Returns the generic merge sound clip for the given level.
        /// Used as a fallback when an item has no specific mergeSound assigned.
        /// </summary>
        /// <param name="level">The level of the merge.</param>
        public AudioClip GetMergeSoundForLevel(int level)
        {
            var category = level switch
            {
                0 => MergeCategory.Items,
                1 => MergeCategory.Kittens,
                _ => MergeCategory.Generic
            };

            return category switch
            {
                MergeCategory.Items => _config?.sfx?.mergeItems,
                MergeCategory.Kittens => _config?.sfx?.mergeKittens,
                _ => _config?.sfx?.merge
            };
        }

        /// <summary>
        /// Plays a sound effect.
        /// </summary>
        /// <param name="clip">The sound effect to play.</param>
        public void PlaySfx(AudioClip clip)
        {
            if (clip == null || _sfxSource == null)
            {
                return;
            }

            _sfxSource.PlayOneShot(clip, _config?.sfxVolume ?? 1f);
        }

        private void HandleItemCollected(MergeItemData itemData, int newCount, Vector3 position)
        {
            PlayCollect();
        }

        private void PlayMusic()
        {
            if (_config == null || _config.backgroundMusic == null || _musicSource == null)
            {
                return;
            }

            _musicSource.clip = _config.backgroundMusic;
            _musicSource.volume = _config.musicVolume;
            _musicSource.loop = true;
            _musicSource.Play();
        }
    }
}