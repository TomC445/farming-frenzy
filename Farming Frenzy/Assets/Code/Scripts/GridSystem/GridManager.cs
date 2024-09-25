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
    [SerializeField] private List<Sprite> _starterTiles;
    [Header("Obstacles")]
    [SerializeField] private Transform _trees;
    [SerializeField] private Transform _rocks;
    #endregion

    #region Properties
    private Dictionary<Vector2, GridTile> _tiles;
    private GridTile _selectedTile;
    private List<string> _excludedTilesNames;
    private List<string> _starterTilesNames;
    private List<BoxCollider2D> _obstacleColliders = new List<BoxCollider2D>();
    private List<GridTile> _purchasedTiles = new List<GridTile>();
    #endregion

    #region Methods
    private void Start()
    {
        _excludedTilesNames = _excludedTiles.Select(x => x.name).ToList();
        _starterTilesNames = _starterTiles.Select(x => x.name).ToList();
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

                if(_starterTilesNames.Contains(tileBase.name))
                {
                    spawnedTile.ChangeTile(PlayerController.Instance.GroundSprite);
                    _purchasedTiles.Add(spawnedTile);
                    UpdateSurroundingTiles(spawnedTile);
                }

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
        if(!_selectedTile.CanBePurchased)
        {
            return;
        }
        _selectedTile.ChangeTile(PlayerController.Instance.GroundSprite);
        _purchasedTiles.Add(_selectedTile);
        UpdateSurroundingTiles(_selectedTile);
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

    private void UpdateSurroundingTiles(GridTile selectedTile)
    {
        if(selectedTile == null)
        {
            return;
        }

        var selectedTilePos = _tiles.FirstOrDefault(tile => tile.Value == selectedTile).Key;
        Vector2[] directions = new Vector2[]
        {
            Vector2.up, Vector2.down, Vector2.left, Vector2.right, new Vector2(1, 1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(-1, -1)
        };

        foreach (var direction in directions)
        {
            var surroundingPos = selectedTilePos + direction;
            var surroundingTile = GetTile(surroundingPos);
            if (surroundingTile != null)
            {
                if (!surroundingTile.IsPurchased)
                {
                    surroundingTile.ChangeTileColor(Color.red);
                }
            }
        }
    }
    #endregion
}
