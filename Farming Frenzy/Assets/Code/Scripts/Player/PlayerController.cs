using NUnit.Framework;
using Unity.VisualScripting;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    #region Editor Fields
    [SerializeField] private Sprite _groundSprite;
    [SerializeField] private int _money;
    [SerializeField] private int _sprayPurchaseAmount;
    [Header("Cursors")]
    [SerializeField] private Texture2D _defaultCursor;
    [SerializeField] private Texture2D _wateringCanCursor;
    [SerializeField] private Texture2D _shovelCursor;
    [SerializeField] private Texture2D _sprayBottleCursor;
    [SerializeField] private Texture2D _scytheCursor;
    [Header("Hotbar Icons")]
    [SerializeField] private Image _defaultCursorBackground;
    [SerializeField] private Image _shovelCursorBackground;
    [SerializeField] private Image _sprayBottleCursorBackground;
    [SerializeField] private Image _scytheCursorBackground;
    [Header("Effects")]
    [SerializeField] private ParticleSystem _sprayBottleParticleSystem;
    #endregion

    #region Properties
    public Sprite GroundSprite => _groundSprite;
    public int Money => _money;
    public delegate void MoneyChangeEvent(int newAmount);
    public event MoneyChangeEvent OnMoneyChange;
    public enum CursorState { Default, Spray, Shovel, Scythe, Planting};
    public CursorState _currentState = CursorState.Default;
    private Color _defaultCursorBackgroundColor;
    private Color _activeCursorBackgroundColor = new Color32(149, 81,19, 255);
    #endregion

    #region Singleton
    public static PlayerController Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
        _defaultCursorBackgroundColor = _defaultCursorBackground.color;
    }
    public void Purchase(int amount)
    {
        _money -= amount;
        OnMoneyChange?.Invoke(_money);
    }

    public void IncreaseMoney(int amount)
    {
        _money += amount;
        OnMoneyChange?.Invoke(_money);
    }

    /// <summary>
    /// Sends out an OnMoneyChange event to refresh UI elements
    /// </summary>
    public void RefreshMoney()
    {
        OnMoneyChange?.Invoke(_money);
    }

    private void Update()
    {
        switch(_currentState)
        {
            case CursorState.Default:
                Cursor.SetCursor(_defaultCursor, Vector2.zero, CursorMode.Auto);
                _defaultCursorBackground.color = _activeCursorBackgroundColor;                
                _shovelCursorBackground.color = _defaultCursorBackgroundColor;
                _sprayBottleCursorBackground.color = _defaultCursorBackgroundColor;
                _scytheCursorBackground.color = _defaultCursorBackgroundColor;
                break;
            case CursorState.Spray:
                Cursor.SetCursor(_sprayBottleCursor, Vector2.zero, CursorMode.Auto);
                _defaultCursorBackground.color = _defaultCursorBackgroundColor;    
                _shovelCursorBackground.color = _defaultCursorBackgroundColor;
                _sprayBottleCursorBackground.color = _activeCursorBackgroundColor;
                _scytheCursorBackground.color = _defaultCursorBackgroundColor;
                break;
            case CursorState.Shovel:
                var hotspot = new Vector2(0, _shovelCursor.height);
                Cursor.SetCursor(_shovelCursor, hotspot, CursorMode.Auto);
                _defaultCursorBackground.color = _defaultCursorBackgroundColor;              
                _shovelCursorBackground.color = _activeCursorBackgroundColor;
                _sprayBottleCursorBackground.color = _defaultCursorBackgroundColor;
                _scytheCursorBackground.color = _defaultCursorBackgroundColor;
                break;
            case CursorState.Scythe:
                Cursor.SetCursor(_scytheCursor, Vector2.zero, CursorMode.Auto);
                _defaultCursorBackground.color = _defaultCursorBackgroundColor;
                _shovelCursorBackground.color = _defaultCursorBackgroundColor;
                _sprayBottleCursorBackground.color = _defaultCursorBackgroundColor;
                _scytheCursorBackground.color = _activeCursorBackgroundColor;
                break;
            default:
                break;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _currentState = CursorState.Default;
        } else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _currentState = CursorState.Spray;
        } else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _currentState = CursorState.Shovel;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            _currentState = CursorState.Scythe;
        }

        if(Input.GetKeyDown(KeyCode.Mouse0) && _currentState == CursorState.Spray)
        {
            Instantiate(_sprayBottleParticleSystem, Camera.main.ScreenToWorldPoint(Input.mousePosition), Quaternion.identity);
            Purchase(_sprayPurchaseAmount);
        }
    }
    #endregion
}
