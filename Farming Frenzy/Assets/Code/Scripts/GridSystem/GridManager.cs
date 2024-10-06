using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Code.Scripts.GridSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    #region Editor Fields
    [Header("Grid Options")]
    [SerializeField] private GridTile _tilePrefab;
    [SerializeField] private Tilemap _backgroundGrid;
    [Header("Camera")]
    [SerializeField] private Transform _camera;
    [Header("Cursor")]
    [SerializeField] private Image _cursorImage;
    [Header("Tiles")]
    [SerializeField] private Transform _tilesContainer;
    [SerializeField] private List<Sprite> _excludedTiles;
    [SerializeField] private List<Sprite> _starterTiles;
    [Header("Obstacles")]
    [SerializeField] private Transform _trees;
    [SerializeField] private Transform _rocks;
    [Header("Plants")]
    [SerializeField] private GameObject _plant;
    [SerializeField] private string _plantName;
    #endregion

    #region Properties
    private Dictionary<Vector2, GridTile> _tiles;
    private GridTile _selectedTile;
    private List<string> _excludedTilesNames;
    private List<string> _starterTilesNames;
    private List<BoxCollider2D> _obstacleColliders = new List<BoxCollider2D>();
    private List<GridTile> _purchasedTiles = new List<GridTile>();
    private List<GridTile> _groundTiles = new List<GridTile>();
    private List<GridTile> _obstructedTiles = new List<GridTile>();
    public string PlantName => _plantName;
    private GridTooltipManager _tooltipManager;
    #endregion

    #region Singleton
    public static GridManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Methods
    private void Start()
    {
        _excludedTilesNames = _excludedTiles.Select(x => x.name).ToList();
        _starterTilesNames = _starterTiles.Select(x => x.name).ToList();
        _tooltipManager = GameObject.Find("Grid tooltip").GetComponent<GridTooltipManager>();
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
            _tooltipManager.SubscribeObstacleEvents(child.gameObject.GetComponent<Obstacle>(), "Tree");
            _obstacleColliders.Add(child.gameObject.GetComponent<BoxCollider2D>());
        }

        foreach (Transform child in _rocks)
        {
            var roundedX = Mathf.Round(child.transform.position.x) + 0.5f;
            var roundedY = Mathf.Round(child.transform.position.y) + 0.5f;
            child.gameObject.transform.position = new Vector2(roundedX, roundedY);
            _tooltipManager.SubscribeObstacleEvents(child.gameObject.GetComponent<Obstacle>(), "Rock");
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

                var spawnedTile = Instantiate(_tilePrefab, instantiatePosition, Quaternion.identity, _tilesContainer);
                spawnedTile.name = $"Tile {x} {y}";

                var isOffset = (x + y) % 2 == 1;
                spawnedTile.Init(isOffset);

                _tiles[new Vector2(x,y)] = spawnedTile;

                if (!IsPositionValid(instantiatePosition))
                {
                    _obstructedTiles.Add(spawnedTile);
                }

                if (_starterTilesNames.Contains(tileBase.name))
                {
                    _groundTiles.Add(spawnedTile);
                }

                spawnedTile.OnTileClicked += HandleTileClicked;
                _tooltipManager.SubscribeTileEvents(spawnedTile);
            }
        }

        foreach (var groundTile in _groundTiles)
        {
            groundTile.ChangeTile(PlayerController.Instance.GroundSprite);
            _purchasedTiles.Add(groundTile);
            UpdateSurroundingTiles(groundTile);
        }

        foreach (var tile in _obstructedTiles)
        {
            tile.LockTile();
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
        
        if (_selectedTile.IsLocked)
        {
            return;
        }
        if(!_selectedTile.CanBePurchased)
        {
            return;
        }
        if (_selectedTile.IsPurchased && _plantName != "")
        {
            InstantiatePlant(_selectedTile.transform.position);
            return;
        }
        if (_selectedTile.IsPurchased && _plantName == "")
        {
            return;
        }
        if (PlayerController.Instance.Money < _selectedTile.Cost)
        {
            return;
        }
        PlayerController.Instance.Purchase(_selectedTile.Cost);
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
            Vector2.up, Vector2.down, Vector2.left, Vector2.right
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

    public void UnlockTiles(Bounds obstacleBounds)
    {
        int xStart = Mathf.FloorToInt(obstacleBounds.center.x - obstacleBounds.extents.x);
        int xEnd = Mathf.FloorToInt(obstacleBounds.center.x + obstacleBounds.extents.x);
        int yStart = Mathf.FloorToInt(obstacleBounds.center.y - obstacleBounds.extents.y);
        int yEnd = Mathf.FloorToInt(obstacleBounds.center.y + obstacleBounds.extents.y);
        for (int x = xStart; x <= xEnd; x++)
        {
            for (int y = yStart; y <= yEnd; y++)
            {
                _tiles[new Vector2(x, y)].UnlockTile();
            }
        }
    }

    public void SetActivePlant(string plantName)
    {
        _plantName = plantName;
        _cursorImage.color = Color.white;
        _cursorImage.sprite = PlantManager.Instance.GetPlantData(plantName)._cursorSprite;
    }

    private void InstantiatePlant(Vector3 tilePosition)
    {
        var plantAmount = PlantManager.Instance.GetPlantData(_plantName)._price;
        if(PlayerController.Instance.Money >= plantAmount)
        {
            var plant = Instantiate(_plant, tilePosition, Quaternion.identity);
            plant.GetComponent<Plant>().InitPlant(PlantManager.Instance.GetPlantData(_plantName));
            PlayerController.Instance.Purchase(plantAmount);
        }
    }

    public void HighlightTiles(GridTile gridTile, bool highlightOn)
    {
        var selectedTilePos = _tiles.FirstOrDefault(tile => tile.Value == gridTile).Key;
        var isTree = (_plantName != "") ? PlantManager.Instance.GetPlantData(_plantName)._isTree : false;
        if (isTree)
        {
            for (int x = 0; x < 3; ++x)
            {
                for (int y = 0; y < 2; ++y)
                {
                    if (highlightOn)
                    {
                        _tiles[new Vector2(selectedTilePos.x + x - 1, selectedTilePos.y + y)].HighlightTile();
                    } else
                    {
                        _tiles[new Vector2(selectedTilePos.x + x - 1, selectedTilePos.y + y)].UnHighlightTile();
                    }
                    
                }
            }
        } else
        {
            for (int x = 0; x < 1; ++x)
            {
                for (int y = 0; y < 1; ++y)
                {
                    if (highlightOn)
                    {
                        _tiles[new Vector2(selectedTilePos.x + x, selectedTilePos.y + y)].HighlightTile();
                    }
                    else
                    {
                        _tiles[new Vector2(selectedTilePos.x + x, selectedTilePos.y + y)].UnHighlightTile();
                    }
                }
            }
        }        
    }
    #endregion
}
