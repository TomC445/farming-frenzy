using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.Scripts.GridSystem
{
    public class Obstacle : MonoBehaviour
    {
        #region Editor Fields
        [SerializeField] private int _cost;
        #endregion

        #region Properties
        public int Cost => _cost;
        public Collider2D Collider { get; private set; }
        public delegate void ObstacleHoverIn(Obstacle tile);

        public delegate void ObstacleHoverOut(Obstacle tile);

        public event ObstacleHoverIn OnObstacleHoverIn;
        public event ObstacleHoverOut OnObstacleHoverOut;

        #endregion

        #region Methods

        private void Start()
        {
            Collider = GetComponent<BoxCollider2D>();
        }

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
            if(PlayerController.Instance._currentState != PlayerController.CursorState.Shovel)
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
}
