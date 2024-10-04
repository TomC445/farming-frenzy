using Code.Scripts.Menus;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Code.Scripts.GridSystem
{
    public class GridTooltipManager : MonoBehaviour
    {
        private float _startedHoverTime;
        private VisualElement _root;
        private VisualElement _tooltip;
        private ShopUI _shopUI;

        public void Start()
        {
            _root = gameObject.GetComponent<UIDocument>().rootVisualElement;
            _root.visible = false;
            _tooltip = _root.Q<VisualElement>("tooltip");
            _root.RegisterCallback<MouseMoveEvent>(_ => UpdatePosition());
        }

        private void UpdatePosition()
        {
            _tooltip.style.top = Screen.currentResolution.height - Input.mousePosition.y + 25;
            _tooltip.style.left = Input.mousePosition.x;
        }

        public void SubscribeTileEvents(GridTile spawnedTile)
        {
            spawnedTile.OnTileHoverIn += HandleTileHoverIn;
            spawnedTile.OnTileHoverOut += _ => HandleTileHoverOut();
        }

        private void HandleTileHoverIn(GridTile tile)
        {
            _root.Q<Label>("name").text = "Grass";
            _root.Q<Label>("status").text = "Click to buy and till. Cost:";
            _root.Q<Label>("money").text = $" ${tile.Cost}";
            _startedHoverTime = Time.time;
        }

        private void HandleTileHoverOut()
        {
            _root.visible = false;
            _startedHoverTime = float.PositiveInfinity;
        }

        public void Update()
        {
            if (!(Time.time - _startedHoverTime > 1.0))
            {
                return;
            }

            if (ShopUI.Instance.MouseInShop)
            {
                HandleTileHoverOut();
                return;
            }

            UpdatePosition();
            _root.visible = true;
        }
    }
}
