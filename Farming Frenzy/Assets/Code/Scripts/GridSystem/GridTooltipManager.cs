using System;
using Code.Scripts.Menus;
using Code.Scripts.Plants;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Scripts.GridSystem
{
    public class GridTooltipManager : MonoBehaviour
    {
        private float _startedHoverTime;
        private VisualElement _root;
        private VisualElement _tooltip;
        private ShopUI _shopUI;
        [CanBeNull] private Plant _currentPlant;
        [CanBeNull] private GridTile _currentTile;
        [CanBeNull] private Obstacle _currentObstacle;
        [CanBeNull] private string _currentObstacleType;

        public void Start()
        {
            _root = gameObject.GetComponent<UIDocument>().rootVisualElement;
            _root.visible = false;
            _tooltip = _root.Q<VisualElement>("tooltip");
            _root.RegisterCallback<MouseMoveEvent>(_ => UpdatePosition());

            // In case these are no longer purchasable
            PlayerController.Instance.OnMoneyChange += _ =>
            {
                if (_currentObstacle) BuildObstacleTooltip(_currentObstacle, _currentObstacleType);
                else if (_currentTile) BuildTileTooltip(_currentTile);
                else if (_currentPlant) BuildPlantTooltip(_currentPlant);
            };
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
            // This is OK to just do directly as if we purchase it, we must be mousing over
            spawnedTile.OnTileClicked += BuildFarmlandTooltip;
        }
        
        public void SubscribeObstacleEvents(Obstacle obstacle, string type)
        {
            obstacle.OnObstacleHoverIn += obs => HandleObstacleHoverIn(obs, type);
            obstacle.OnObstacleHoverOut += _ => HandleHoverOut();
        }
        
        public void SubscribePlantEvents(Plant plant)
        {
            plant.OnHoverIn += HandlePlantHoverIn;
            plant.OnHoverOut += _ => HandleHoverOut();
        }

        private void HandleTileHoverIn(GridTile tile)
        {
            BuildTileTooltip(tile);
            _currentTile = tile;
            _startedHoverTime = Time.time;
        }

        private void BuildTileTooltip([NotNull] GridTile tile)
        {
            if (tile.IsPurchased)
            {
                BuildFarmlandTooltip(tile);
            }
            else
            {
                BuildGrassTooltip(tile);
            }

            _root.Q<Label>("water_modifier").style.display = DisplayStyle.None; // TODO water
            _root.Q<Label>("legume_modifier").style.display = DisplayStyle.None; // TODO legumes
            _root.Q<Label>("power").style.display = DisplayStyle.None;
        }

        private void BuildGrassTooltip([NotNull] GridTile tile)
        {
            _root.Q<Label>("name").text = "Grass";
            _root.Q<Label>("status").text =
                $"Click to buy and till. Cost: {FarmingFrenzyColors.PriceRichText(tile.Cost)}";
        }

        private void BuildFarmlandTooltip([NotNull] GridTile _)
        {
            _root.Q<Label>("name").text = "Farmland";
            _root.Q<Label>("status").text = "Plant here by selecting a plant\n in the sho pand then clicking here.";
        }

        private void HandleObstacleHoverIn(Obstacle obstacle, string type)
        {
            _currentObstacle = obstacle;
            _currentObstacleType = type;
            BuildObstacleTooltip(obstacle, type);
            _startedHoverTime = Time.time;
        }

        private void BuildObstacleTooltip([NotNull] Obstacle obstacle, string type)
        {
            _root.Q<Label>("name").text = type;
            _root.Q<Label>("status").text = $"Click to buy and remove. Cost: {FarmingFrenzyColors.PriceRichText(obstacle.Cost)}";
            _root.Q<Label>("water_modifier").style.display = DisplayStyle.None; // TODO water
            _root.Q<Label>("legume_modifier").style.display = DisplayStyle.None; // TODO legumes
            _root.Q<Label>("power").style.display = DisplayStyle.None;
        }

        private void HandlePlantHoverIn(Plant plant)
        {
            _startedHoverTime = Time.time;
            _currentPlant = plant;
            BuildPlantTooltip(plant);
        }
        
        private void BuildPlantTooltip([NotNull] Plant plant)
        {
            _root.Q<Label>("name").text = plant.PlantName;
            _root.Q<Label>("status").text = plant.StatusRichText;
            _root.Q<Label>("water_modifier").style.display = DisplayStyle.None; // TODO water
            _root.Q<Label>("legume_modifier").style.display = DisplayStyle.None; // TODO legumes
            _root.Q<Label>("power").style.display = DisplayStyle.None;
        }

        private void HandleHoverOut()
        {
            _root.visible = false;
            _startedHoverTime = float.PositiveInfinity;
            _currentPlant = null;
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

            if (_currentPlant) BuildPlantTooltip(_currentPlant); // In case the time to next stage changes
            UpdatePosition();
            _root.visible = true;
        }
    }
}
