using System.Collections.Generic;
using System.Linq;
using Code.GrowthRateExtension;
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
        public bool MouseInShop => _tooltipManipulator.MouseInShop;

        private void Start()
        {
            _root = ((UIDocument)gameObject.GetComponent(typeof(UIDocument))).rootVisualElement;
            _itemTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/shop_item.uxml");
            _itemTooltipTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/shop_item_tooltip.uxml");
            _tooltipManipulator = new ShopContainerTooltipManipulator();
            _root.AddManipulator(_tooltipManipulator);

            var defaultSprite = Resources.Load<Sprite>("Placeholder");
            
            var plants = new List<string>
            {
                // Tier 1
                "Tomato", "Corn", "Clover", "Nettle",
                
                // Tier 2
                "Marigold", "Pumpkin", "Banana", // "Scarecrow",
                
                // Tier 3
                "Beans", "Shrub Rose", "Tall Grass", "Apple Tree", // "Sprinkler",
            };

            foreach (var plant in plants)
            {
                AddShopItem(Resources.Load<PlantData>(plant), defaultSprite);
            }

            Resources.UnloadUnusedAssets();
        }

        private void AddShopItem(PlantData data, Sprite defaultSprite)
        {
            // Set up item template
            VisualElement ui = _itemTemplate.Instantiate();
            var sprite = data._growthSprite?.LastOrDefault() ?? defaultSprite;
            ui.Q("plant_icon").style.backgroundImage = new StyleBackground(sprite);
            ui.Q<Label>("plant_name").text = data.name;
            ui.Q<Label>("price").text = $"${data._price}";
            ui.RegisterCallback<ClickEvent>(_ =>
            {
                // TODO(placeables): begin placing
                GridManager.Instance.SetActivePlant(data.name);
                print($"Clicked on a {data.name}");
            });

            // Set up tooltip
            VisualElement tooltip = _itemTooltipTemplate.Instantiate();
            tooltip.Q<Label>("plant_name").text = data.name;
            tooltip.Q<Label>("cost").text = $"${data._price}";
            tooltip.Q<Label>("yield").text = $"${data._goldGenerated}";
            tooltip.Q<Label>("flavour").text = data.flavorText;

            var powerLabel = tooltip.Q<Label>("power");
            powerLabel.enableRichText = true;
            
            powerLabel.text = (data.powerText?.Length ?? 0) > 0 ? $"<u>{data.powerText}</u>" : "";

            var growth = tooltip.Q<Label>("growth");
            growth.text = data.GrowthRateBand.Text();
            growth.style.color = data.GrowthRateBand.Color();

            var fruiting = tooltip.Q<Label>("fruiting");
            fruiting.text = data.FruitingRateBand.Text();
            fruiting.style.color = data.FruitingRateBand.Color();

            // Add tooltip to item, and item to list
            ui.AddManipulator(new ShopItemTooltipManipulator(_tooltipManipulator, tooltip));
            _root.Q("shop_list").Add(ui);
        }
    }
}
