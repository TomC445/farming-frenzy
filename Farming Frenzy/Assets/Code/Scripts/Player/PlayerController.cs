using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Editor Fields
    [SerializeField] private Sprite _groundSprite;
    [SerializeField] private int _money;
    #endregion

    #region Properties
    public Sprite GroundSprite => _groundSprite;
    public int Money => _money;
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
    public void Purchase(int amount)
    {
        _money -= amount;
    }
    #endregion
}
