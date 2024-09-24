using System.Collections.Generic;
using System.Security.Claims;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    #region Editor Fields
    [Header("Grid Options")]
    [SerializeField] private int _width;
    [SerializeField] private int _height;
    [SerializeField] private GridTile _tilePrefab;
    [Header("Camera")]
    [SerializeField] private Transform _camera;
    #endregion

    #region Properties
    private Dictionary<Vector2, GridTile> _tiles;
    private GridTile _selectedTile;
    #endregion

    #region Methods
    private void Start()
    {
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        _tiles = new Dictionary<Vector2, GridTile>();
        for (int x = 0; x < _width; ++x)
        {
            for (int y = 0; y < _height; ++y)
            {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(x,y), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";

                var isOffset = (x + y) % 2 == 1;
                spawnedTile.Init(isOffset);

                _tiles[new Vector2(x,y)] = spawnedTile;

                spawnedTile.OnTileClicked += HandleTileClicked;
            }
        }

        _camera.transform.position = new Vector3((float)_width/2 - 0.5f, (float)_height/2 - 0.5f, -10);
    }

    public GridTile GetTile(Vector2 pos)
    {
        if (_tiles.TryGetValue(pos, out var tile))
        {
            return tile;
        }
        return null;
    }

    public void HandleTileClicked(GridTile clickedTile)
    {
        if(_selectedTile != null && _selectedTile != clickedTile)
        {
            _selectedTile.DeselectTile();
        }

        _selectedTile = clickedTile;
    }
    #endregion
}
