using NUnit.Framework;
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
    public delegate void MoneyChangeEvent(int newAmount);
    public event MoneyChangeEvent OnMoneyChange;
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

    //FF000C
    #endregion
}
