using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/GameConfig")]
public class GameConfig : ScriptableObject
{

    [Header("Gameplay")]
    public int itemsToWin = 7;

    [Header("Audio")]
    public AudioClip collectSound;
    public AudioClip backgroundMusic;
    public float musicVolume = 0.5f;
    public float sfxVolume = 1f;

    [Header("Visuals")]
    public Sprite backgroundSprite;
    public Color backgroundColor = Color.white;
}