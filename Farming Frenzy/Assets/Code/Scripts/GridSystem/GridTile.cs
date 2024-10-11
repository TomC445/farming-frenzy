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
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private GameObject _highlight;
        [SerializeField] private GameObject _selectedHighlight;
        [SerializeField] private int _cost;
        #endregion

        #region Properties
        public Collider2D Collider { get; private set; }

        public bool IsPurchased { get; private set; }

        public bool CanBePurchased { get; private set; }

        public bool IsLocked { get; private set; }

        public int Cost => _cost;
        public delegate void TileClicked(GridTile tile);

        public delegate void TileHoverIn(GridTile tile);
        public delegate void TileHoverOut(GridTile tile);
        public event TileClicked OnTileClicked;
        public event TileHoverIn OnTileHoverIn;
        public event TileHoverOut OnTileHoverOut;

        #endregion

        #region Methods
        public void Init(bool isOffset)
        {
            Collider = GetComponent<BoxCollider2D>();
            _renderer.color = isOffset ? _baseColour : _offsetColour;
        }

        private void OnMouseEnter()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                GridManager.Instance.HighlightTiles(this, false);
                return;
            }
            if(GridManager.Instance.PlantName != "")
            {
                GridManager.Instance.HighlightTiles(this, true);
            }
            _highlight.SetActive(true);
            OnTileHoverIn?.Invoke(this);
        }

        private void OnMouseExit()
        {
            if (GridManager.Instance.PlantName != "")
            {
                GridManager.Instance.HighlightTiles(this, false);
            }
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

        public void DeselectTile()
        {
            _selectedHighlight.SetActive(false);
        }

        public void ChangeTile(Sprite tile)
        {
            _renderer.color = Color.white;
            _renderer.sprite = tile;
            IsPurchased = true;
        }

        public void ChangeTileColor(Color colour)
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
        #endregion
    }
}
