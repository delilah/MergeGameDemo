using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private GameConfig _config;
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioSource _musicSource;

    public AudioClip SpawnerClickSound => _config?.sfx.spawnerClick;
    public AudioClip MergeSound => _config?.sfx.merge;
    public AudioClip MergeSoundItems => _config?.sfx.mergeItems;
    public AudioClip MergeSoundKittens => _config?.sfx.mergeKittens;
    public AudioClip CollectSound => _config?.sfx.collect;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (CollectionManager.Instance != null)
        {
            CollectionManager.Instance.OnItemCollected += HandleItemCollected;
        }
        else
        {
            Debug.LogError("AudioManager: CollectionManager.Instance is null in Start!");
        }

        PlayMusic();
    }

    private void OnDestroy()
    {
        if (CollectionManager.Instance != null)
            CollectionManager.Instance.OnItemCollected -= HandleItemCollected;
    }

    //Play Collect Sound
    private void HandleItemCollected(MergeItemData itemData, int newCount, Vector3 position)
    {
        PlaySfx(_config?.sfx.collect);
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null || _sfxSource == null) return;
        _sfxSource.PlayOneShot(clip, _config.sfxVolume);
    }

    public void PlayMergeSound(int level)
    {
        if (level == 0)
            PlaySfx(_config.sfx.mergeItems);
        else if (level == 1)
            PlaySfx(_config.sfx.mergeKittens);
        else 
            PlaySfx(_config.sfx.merge);
    }

    private void PlayMusic()
    {
        if (_config == null || _config.backgroundMusic == null) return;
        _musicSource.clip = _config.backgroundMusic;
        _musicSource.volume = _config.musicVolume;
        _musicSource.loop = true;
        _musicSource.Play();
    }
}