using UnityEngine;

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
        if(PlayerController.Instance.Money < _cost)
        {
            return;
        }
        PlayerController.Instance.Purchase(_cost);
        GridManager.Instance.UnlockTiles(GetComponent<BoxCollider2D>().bounds);
        Destroy(gameObject);
    }
    #endregion
}
