using UnityEngine;
using UnityEngine.EventSystems;

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
    private bool _isSelected;
    private bool _isPurchased;
    private bool _canBePurchased;
    private bool _isLocked;
    public bool IsPurchased => _isPurchased;
    public bool CanBePurchased => _canBePurchased;
    public bool IsLocked => _isLocked;
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
        if (OnTileClicked != null)
        {
            OnTileClicked(this);
        }
    }

    public void SelectTile()
    {
        if(!_isSelected)
        {
            _selectedHighlight.SetActive(true);
            _isSelected = true;
        }
    }

    public void DeselectTile()
    {
        _selectedHighlight.SetActive(false);
        _isSelected = false;
    }

    public void ChangeTile(Sprite tile)
    {
        _renderer.color = Color.white;
        _renderer.sprite = tile;
        _isPurchased = true;
    }

    public void ChangeTileColor(Color colour)
    {
        _renderer.color = colour;
        _canBePurchased = true;
    }

    public void SetCanPurchaseTile(bool canPurchaseTile)
    {
        _canBePurchased = canPurchaseTile;
    }

    public void LockTile()
    {
        GetComponent<BoxCollider2D>().enabled = false;
        _isLocked = true;
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
        GetComponent<BoxCollider2D>().enabled = true;
        _isLocked = false;
    }
    #endregion
}
