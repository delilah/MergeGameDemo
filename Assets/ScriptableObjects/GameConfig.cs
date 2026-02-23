using UnityEngine;

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