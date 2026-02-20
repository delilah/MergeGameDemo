using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private GameConfig _config;
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioSource _musicSource;

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

    private void HandleItemCollected(MergeItemData itemData, int newCount, Vector3 position)
    {
            Debug.Log($"AudioManager: HandleItemCollected called for {itemData?.itemName}");

        PlayCollectSound();
    }

    public void PlayCollectSound()
    {
            Debug.Log($"PlayCollectSound called. Config={_config != null}, Clip={_config?.collectSound != null}, SfxSource={_sfxSource != null}");

        if (_config == null || _config.collectSound == null) return;
        _sfxSource.PlayOneShot(_config.collectSound, _config.sfxVolume);
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