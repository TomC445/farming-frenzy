using UnityEngine;
using UnityEngine.EventSystems;

public class Obstacle : MonoBehaviour
{
    #region Editor Fields
    [SerializeField] private int _cost;
    #endregion

    #region Properties
    public int Cost => _cost;
    #endregion

    #region Methods
    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        if (PlayerController.Instance.Money < _cost)
        {
            return;
        }
        PlayerController.Instance.Purchase(_cost);
        GridManager.Instance.UnlockTiles(GetComponent<BoxCollider2D>().bounds);
        Destroy(gameObject);
    }
    #endregion
}
