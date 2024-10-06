using System;
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
            _tooltip.style.left = Math.Max(0, Input.mousePosition.x - _tooltip.resolvedStyle.width);
        }

        public void SubscribeTileEvents(GridTile spawnedTile)
        {
            spawnedTile.OnTileHoverIn += HandleTileHoverIn;
            spawnedTile.OnTileHoverOut += _ => HandleHoverOut();
        }
        
        public void SubscribeObstacleEvents(Obstacle obstacle, string type)
        {
            obstacle.OnObstacleHoverIn += obs => HandleObstacleHoverIn(obs, type);
            obstacle.OnObstacleHoverOut += _ => HandleHoverOut();
        }

        private void HandleTileHoverIn(GridTile tile)
        {
            _root.Q<Label>("name").text = "Grass";
            _root.Q<Label>("status").text = "Click to buy and till. Cost:";
            _root.Q<Label>("water_modifier").style.display = DisplayStyle.None; // TODO water
            _root.Q<Label>("legume_modifier").style.display = DisplayStyle.None; // TODO legumes
            _root.Q<Label>("power").style.display = DisplayStyle.None;
            _root.Q<Label>("money").text = $" ${tile.Cost}";
            _startedHoverTime = Time.time;
        }

        private void HandleObstacleHoverIn(Obstacle obstacle, string type)
        {
            _root.Q<Label>("name").text = type;
            _root.Q<Label>("status").text = "Click to buy and remove. Cost:";
            _root.Q<Label>("water_modifier").style.display = DisplayStyle.None; // TODO water
            _root.Q<Label>("legume_modifier").style.display = DisplayStyle.None; // TODO legumes
            _root.Q<Label>("power").style.display = DisplayStyle.None;
            _root.Q<Label>("money").text = $" ${obstacle.Cost}";
            _startedHoverTime = Time.time;
        }
        
        private void HandleHoverOut()
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
                HandleHoverOut();
                return;
            }

            UpdatePosition();
            _root.visible = true;
        }
    }
}
