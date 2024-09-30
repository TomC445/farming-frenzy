using System.Collections.Generic;
using System.Linq;
using Code.GrowthRateExtension;
using Code.Scripts.Plants;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Scripts.Menus
{
    public class ShopUI : MonoBehaviour
    {
        private VisualTreeAsset _itemTemplate;
        private VisualTreeAsset _itemTooltipTemplate;
        private ShopContainerTooltipManipulator _tooltipManipulator;
        private VisualElement _root;

        private void Start()
        {
            _root = ((UIDocument)gameObject.GetComponent(typeof(UIDocument))).rootVisualElement;
            _itemTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/shop_item.uxml");
            _itemTooltipTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/shop_item_tooltip.uxml");

            _tooltipManipulator = new ShopContainerTooltipManipulator();
            _root.AddManipulator(_tooltipManipulator);

            var plants = new List<string>
            {
                // Tier 1
                "Tomato", "Corn", "Clover", "Stinging Nettle",
                
                // Tier 2
                "Marigold", "Pumpkin", "Banana", // "Scarecrow",
                
                // Tier 3
                "Beans", "Shrub Rose", "Tall Grass", "Apple Tree", // "Sprinkler",
            };

            foreach (var plant in plants)
            {
                AddShopItem(Resources.Load<PlantData>(plant));
            }

            Resources.UnloadUnusedAssets();
        }

        private void AddShopItem(PlantData plant)
        {
            // Set up item template
            VisualElement ui = _itemTemplate.Instantiate();
            ui.Q("plant_icon").style.backgroundImage = new StyleBackground(plant._plantSprites.Last());
            ui.Q<Label>("plant_name").text = plant._name;
            ui.Q<Label>("price").text = plant._price.ToString();
            ui.RegisterCallback<ClickEvent>(_ =>
            {
                // TODO(placeables): begin placing
                print($"Clicked on a {plant._name}");
            });

            // Set up tooltip
            VisualElement tooltip = _itemTooltipTemplate.Instantiate();
            tooltip.Q<Label>("plant_name").text = plant._name;
            tooltip.Q<Label>("cost").text = $"${plant._price}";
            tooltip.Q<Label>("yield").text = $"${plant._goldGenerated}";
            tooltip.Q<Label>("flavour").text = plant._flavourText;

            var powerLabel = tooltip.Q<Label>("power");
            powerLabel.enableRichText = true;
            
            powerLabel.text = plant._powerDescription.Length > 0 ? $"<u>{plant._powerDescription}</u>" : "";

            var growth = tooltip.Q<Label>("growth");
            growth.text = plant.GrowthRateBand.Text();
            growth.style.color = plant.GrowthRateBand.Color();

            var fruiting = tooltip.Q<Label>("fruiting");
            fruiting.text = plant.FruitingRateBand.Text();
            fruiting.style.color = plant.FruitingRateBand.Color();

            // Add tooltip to item, and item to list
            ui.AddManipulator(new ShopItemTooltipManipulator(_tooltipManipulator, tooltip));
            _root.Q("shop_list").Add(ui);
        }
    }
}
