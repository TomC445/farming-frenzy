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
    #endregion

    #region Properties
    private bool _isSelected;
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
        _renderer.sprite = tile;
    }
    #endregion
}
