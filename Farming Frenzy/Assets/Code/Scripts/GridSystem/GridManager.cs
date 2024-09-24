using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    #region Editor Fields
    [Header("Grid Options")]
    [SerializeField] private GridTile _tilePrefab;
    [SerializeField] private Tilemap _backgroundGrid;
    [Header("Camera")]
    [SerializeField] private Transform _camera;
    [Header("Tiles")]
    [SerializeField] private Transform _tilesContainer;
    [SerializeField] private List<Sprite> _excludedTiles;
    [Header("Obstacles")]
    [SerializeField] private Transform _trees;
    [SerializeField] private Transform _rocks;
    #endregion

    #region Properties
    private Dictionary<Vector2, GridTile> _tiles;
    private GridTile _selectedTile;
    private List<string> _excludedTilesNames;
    private List<BoxCollider2D> _obstacleColliders = new List<BoxCollider2D>();
    #endregion

    #region Methods
    private void Start()
    {
        _excludedTilesNames = _excludedTiles.Select(x => x.name).ToList();
        InitObstacles();
        GenerateGrid();
    }

    private void InitObstacles()
    {
        foreach(Transform child in _trees)
        {
            var roundedX = Mathf.Round(child.transform.position.x) + 0.5f;
            var roundedY = Mathf.Round(child.transform.position.y) + 0.5f;
            child.gameObject.transform.position = new Vector2(roundedX, roundedY);
            _obstacleColliders.Add(child.gameObject.GetComponent<BoxCollider2D>());
        }

        foreach (Transform child in _rocks)
        {
            var roundedX = Mathf.Round(child.transform.position.x) + 0.5f;
            var roundedY = Mathf.Round(child.transform.position.y) + 0.5f;
            child.gameObject.transform.position = new Vector2(roundedX, roundedY);
            _obstacleColliders.Add(child.gameObject.GetComponent<BoxCollider2D>());
        }

        Physics2D.SyncTransforms();
    }

    private void GenerateGrid()
    {
        var tileBounds = _backgroundGrid.localBounds.size;
        var _width = tileBounds.x;
        var _height = tileBounds.y;
        _tiles = new Dictionary<Vector2, GridTile>();
        for (int x = 0; x < _width; ++x)
        {
            for (int y = 0; y < _height; ++y)
            {
                var tilePosition = new Vector3Int(x, y, 0);
                var instantiatePosition = new Vector3(x + 0.5f, y + 0.5f, 0);
                TileBase tileBase = _backgroundGrid.GetTile(tilePosition);
                
                if (_excludedTilesNames.Contains(tileBase.name))
                {
                    continue;
                }

                if (!IsPositionValid(instantiatePosition))
                {
                    continue;
                }

                var spawnedTile = Instantiate(_tilePrefab, instantiatePosition, Quaternion.identity, _tilesContainer);
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
        _selectedTile.ChangeTile(PlayerController.Instance.GroundSprite);
    }

    private bool IsPositionValid(Vector2 pos)
    {
        Bounds box = new Bounds(pos, new Vector3(1f, 1f, 0f));
        foreach (var collider in _obstacleColliders)
        {
            if (collider.bounds.Intersects(box))
            {
                return false;
            }
        }
        return true;
    }
    #endregion
}
