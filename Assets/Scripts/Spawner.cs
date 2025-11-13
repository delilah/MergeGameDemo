using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public SpawnerData spawnerData;
    [SerializeField] private Item _itemPrefab;

      [Header("References")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Collider2D _collider;

    public void Initialize(SpawnerData data)
    {
        spawnerData = data;
    }
}