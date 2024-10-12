using System;
using Code.Scripts.Player;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.Scripts.GridSystem
{
    public class GridTile : MonoBehaviour
    {
        #region Editor Fields
        [Header("Tile Settings")]
        [SerializeField] private Color _baseColour;
        [SerializeField] private Color _offsetColour;
        [SerializeField] public SpriteRenderer _renderer;
        [SerializeField] private GameObject _highlight;
        [SerializeField] private int _cost;
        #endregion

        #region Properties

        private bool _isMouseOver;
        public Collider2D Collider { get; private set; }
        public bool IsPurchased { get; private set; }
        public bool CanBePurchased { get; private set; }
        public bool IsLocked { get; private set; }
        public int Cost => _cost;
        public bool HasPlant;

        public delegate void TileClicked(GridTile tile);

        public delegate void TileHoverIn(GridTile tile);
        public delegate void TileHoverOut(GridTile tile);
        public event TileClicked OnTileClicked;
        public event TileHoverIn OnTileHoverIn;
        public event TileHoverOut OnTileHoverOut;

        #endregion

        #region Methods
        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        public void Init(bool isOffset)
        {
            Collider = GetComponent<BoxCollider2D>();
            _renderer.color = isOffset ? _baseColour : _offsetColour;
        }

        private void OnMouseEnter()
        {
            _isMouseOver = true;

            if (EventSystem.current.IsPointerOverGameObject())
            {
                GridManager.Instance.HighlightTiles(this, false);
                return;
            }

            if(GridManager.Instance.PlantName != "")
            {
                GridManager.Instance.HighlightTiles(this, true);
            }

            if (CanBePurchased && !IsPurchased)
            {
                PlayerController.Instance.StartContextualCursor(PlayerController.CursorState.Shovel);
            }

            _highlight.SetActive(true);
            OnTileHoverIn?.Invoke(this);
        }

        private void OnMouseExit()
        {
            _isMouseOver = false;

            if (GridManager.Instance.PlantName != "")
            {
                GridManager.Instance.HighlightTiles(this, false);
            }

            PlayerController.Instance.EndContextualCursor(PlayerController.CursorState.Shovel);

            _highlight.SetActive(false);
            OnTileHoverOut?.Invoke(this);
        }

        private void OnMouseDown()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            OnTileClicked?.Invoke(this);
        }

        public void PurchaseTile(Sprite tile)
        {
            //print("Tile Purchased! " + tile.name);
            PlayerController.Instance.EndContextualCursor(PlayerController.CursorState.Shovel);
            _renderer.color = Color.white;
            _renderer.sprite = tile;
            IsPurchased = true;
            CanBePurchased = true;
        }

        public void MakePurchasable(Color colour)
        {
            _renderer.color = colour;
            CanBePurchased = true;
        }

        public void LockTile()
        {
            Collider.enabled = false;
            IsLocked = true;
        }

        public void HighlightTile()
        {
            _highlight.SetActive(true);
        }

        public void UnHighlightTile()
        {
            _highlight.SetActive(false);
        }

        public void UnlockTile()
        {
            Collider.enabled = true;
            IsLocked = false;
        }

        private void Update()
        {
            if (_isMouseOver && Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                OnTileClicked?.Invoke(this);
            }
        }

        #endregion
    }
}
