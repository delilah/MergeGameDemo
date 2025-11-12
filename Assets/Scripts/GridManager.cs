using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{

    [Header("Grid Settings")]
    [SerializeField] private int _width;
    [SerializeField] private int _height;
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private Transform _camera;

    private Dictionary<Vector2, Tile> _tiles;

    void Start() {
        GenerateGrid();
    }

    void GenerateGrid() {

    if (_tilePrefab == null)
    {
        Debug.LogWarning("No tilePrefab assigned!");
        return;
    }

        _tiles = new Dictionary<Vector2, Tile>();
        for (int x = 0; x < _width; x++) {
            for (int y = 0; y < _height; y++) {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(x, y), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";

                // calculating if it's the next tile to create the checkered look
                var isAlternateTile = (x % 2 != y % 2);
                spawnedTile.Init(isAlternateTile);

                _tiles[new Vector2(x, y)] = spawnedTile;
            }
        }

        _camera.transform.position = new Vector3((float)_width / 2 -0.5f, (float)_height / 2 - 0.5f,-10);
    }

    public Tile GetTileAtPosition(Vector2 pos) {
        if (_tiles.TryGetValue(pos, out var tile)) return tile;
        return null;
    }
}
