using System.Collections.Generic;
using System.Linq;
using Code.Scripts.Plants;
using Code.Scripts.Player;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace Code.Scripts.GridSystem
{
    public class GridManager : MonoBehaviour
    {
        #region Editor Fields
        [SerializeField] private Camera _gameCamera;
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
        [SerializeField] private GameObject _healthBar;
        #endregion

        #region Properties
        private Dictionary<Vector2, GridTile> _tiles;
        private List<string> _excludedTilesNames;
        private List<string> _starterTilesNames;
        private List<BoxCollider2D> _obstacleColliders = new();
        private List<GridTile> _groundTiles = new();
        private List<GridTile> _obstructedTiles = new();
        private Transform _plantsTransform;
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

        public void Restart() {
            _excludedTilesNames = _excludedTiles.Select(x => x.name).ToList();
            _starterTilesNames = _starterTiles.Select(x => x.name).ToList();
            _tooltipManager = GameObject.Find("Grid tooltip").GetComponent<GridTooltipManager>();
            _plantsTransform = GameObject.Find("Plants").transform;
            _backgroundGrid = GameObject.Find("Grid").transform.Find("BaseTiles").GetComponent<Tilemap>();
            _camera = GameObject.Find("Main Camera").transform;
            _tilesContainer = GameObject.Find("Tiles").transform;
            _trees = GameObject.Find("Trees").transform;
            _rocks = GameObject.Find("Rocks").transform;
            _tilePrefab = GameObject.Find("Tile").GetComponent<GridTile>();
            InitObstacles();
            GenerateGrid();
        }

        private void InitObstacles()
        {
            _obstacleColliders = new();
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
            var width = tileBounds.x;
            var height = tileBounds.y;
            _groundTiles = new();
            _obstructedTiles = new();
            _tiles = new Dictionary<Vector2, GridTile>();
            for (var x = 0; x < width; ++x)
            {
                for (var y = 0; y < height; ++y)
                {
                    var tilePosition = new Vector3Int(x, y, 0);
                    var instantiatePosition = new Vector3(x + 0.5f, y + 0.5f, 0);
                    var tileBase = _backgroundGrid.GetTile(tilePosition);
                
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
                groundTile.PurchaseTile(PlayerController.Instance.GroundSprite);
                UpdateSurroundingTiles(groundTile);
            }

            foreach (var tile in _obstructedTiles)
            {
                tile.LockTile();
            }
        }

        private GridTile GetTile(Vector2 pos)
        {
            return _tiles.GetValueOrDefault(pos);
        }

        private void HandleTileClicked(GridTile tile)
        {
            lock (tile)
            {
                if (tile.IsLocked || !tile.CanBePurchased) return;

                switch (tile.IsPurchased)
                {
                    // Try place the plant that is selected
                    case true when PlayerController.Instance.CurrentlyActiveCursor == PlayerController.CursorState.Planting && !tile.HasPlant:
                        TryPlacePlant(tile);
                        break;
    
                    // No plant selected
                    case true:
                        break;

                    // Try purchase the tile
                    case false when PlayerController.Instance.TryPurchase(tile.Cost):
                        AudioManager.Instance.PlaySFX("digMaybe");
                        tile.PurchaseTile(PlayerController.Instance.GroundSprite);
                        UpdateSurroundingTiles(tile);
                        break;
                }
            }
        }

        private bool IsPositionValid(Vector2 pos)
        {
            var box = new Bounds(pos, new Vector3(1f, 1f, 0f));
            return _obstacleColliders.All(obstacle => !obstacle.bounds.Intersects(box));
        }

        private void UpdateSurroundingTiles(GridTile selectedTile)
        {
            if(selectedTile == null) return;

            var selectedTilePos = _tiles.FirstOrDefault(tile => tile.Value == selectedTile).Key;
            var directions = new[]
            {
                Vector2.up, Vector2.down, Vector2.left, Vector2.right
            };

            foreach (var direction in directions)
            {
                var surroundingPos = selectedTilePos + direction;
                var surroundingTile = GetTile(surroundingPos);
                if (surroundingTile == null || surroundingTile.IsPurchased) continue;

                surroundingTile.MakePurchasable(Color.red);
            }
        }

        public void UnlockTiles(Bounds obstacleBounds)
        {
            var xStart = Mathf.FloorToInt(obstacleBounds.center.x - obstacleBounds.extents.x);
            var xEnd = Mathf.FloorToInt(obstacleBounds.center.x + obstacleBounds.extents.x);
            var yStart = Mathf.FloorToInt(obstacleBounds.center.y - obstacleBounds.extents.y);
            var yEnd = Mathf.FloorToInt(obstacleBounds.center.y + obstacleBounds.extents.y);
            for (var x = xStart; x <= xEnd; x++)
            {
                for (var y = yStart; y <= yEnd; y++)
                {
                    _tiles[new Vector2(x, y)].UnlockTile();
                }
            }
        }

        public void SetActivePlant(string plantName)
        {
            _plantName = plantName;
            Debug.Log(plantName);
            PlayerController.Instance.SetPickedCursor(PlayerController.CursorState.Planting, Resources.Load<Texture2D>($"SeedBags/{plantName}"));
        }

        private void TryPlacePlant(GridTile tile)
        {
            var plantAmount = PlantManager.Instance.GetPlantData(_plantName)._price;
            if (!PlayerController.Instance.TryPurchase(plantAmount)) return;
            

            AudioManager.Instance.PlaySFX("planting");

            var plant = Instantiate(_plant, tile.transform.position, Quaternion.identity, _plantsTransform);
            var plantHealthBar = Instantiate(_healthBar, new Vector2(0,0),Quaternion.identity, plant.transform);
            Canvas hbCanvas = plantHealthBar.transform.Find("Canvas").gameObject.GetComponent<Canvas>();
            hbCanvas.worldCamera = _gameCamera;
            hbCanvas.transform.position = new Vector2(plant.transform.position.x*1000,plant.transform.position.y*1000 - 530);
            plantHealthBar.transform.localScale = new Vector2(0.001f,0.001f);
            var plantComponent = plant.GetComponent<Plant>();
            plantComponent.InitPlant(PlantManager.Instance.GetPlantData(_plantName), tile, plantHealthBar);
            _tooltipManager.SubscribePlantEvents(plantComponent);
        }

        public void HighlightTiles(GridTile gridTile, bool highlightOn)
        {
            var selectedTilePos = _tiles.FirstOrDefault(tile => tile.Value == gridTile).Key;
            var isTree = _plantName != "" && PlantManager.Instance.GetPlantData(_plantName)._isTree;
            if (isTree)
            {
                for (var x = 0; x < 3; ++x)
                {
                    for (var y = 0; y < 2; ++y)
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
                for (var x = 0; x < 1; ++x)
                {
                    for (var y = 0; y < 1; ++y)
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
}
