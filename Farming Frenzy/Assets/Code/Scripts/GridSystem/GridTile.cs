using UnityEngine;

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
    public event TileClicked OnTileClicked;
    #endregion

    #region Methods
    public void Init(bool isOffset)
    {
        _renderer.color = isOffset ? _baseColour : _offsetColour;
    }

    private void OnMouseEnter()
    {
        _highlight.SetActive(true);
    }

    private void OnMouseExit()
    {
        _highlight.SetActive(false);
    }

    private void OnMouseDown()
    {
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

    public void UnlockTile()
    {
        GetComponent<BoxCollider2D>().enabled = true;
        _isLocked = false;
    }
    #endregion
}
