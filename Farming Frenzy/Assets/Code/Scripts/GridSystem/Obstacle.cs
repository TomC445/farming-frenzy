using System;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    #region Editor Fields
    [SerializeField] private int _cost;
    #endregion

    #region Properties
    public int Cost => _cost;
    public delegate void ObstacleHoverIn(Obstacle tile);

    public delegate void ObstacleHoverOut(Obstacle tile);

    public event ObstacleHoverIn OnObstacleHoverIn;
    public event ObstacleHoverOut OnObstacleHoverOut;

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

    private void OnMouseEnter()
    {
        OnObstacleHoverIn?.Invoke(this);
    }

    private void OnMouseExit()
    {
        OnObstacleHoverOut?.Invoke(this);
    }

    #endregion
}
