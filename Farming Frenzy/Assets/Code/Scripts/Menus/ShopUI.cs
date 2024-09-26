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

        private void Start()
        {
            _root = ((UIDocument)gameObject.GetComponent(typeof(UIDocument))).rootVisualElement;
            _itemTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/shop_item.uxml");
            _itemTooltipTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/shop_item_tooltip.uxml");
            var sprites = Resources.LoadAll<Sprite>("crops");

            _tooltipManipulator = new ShopContainerTooltipManipulator();
            _root.AddManipulator(_tooltipManipulator);

            AddShopItem(
                sprites.First(s => s.name == "crops_83"),
                "Watermelon", 
                103, 
                50,
                "",
                "Juicy and sweet!",
                GrowthRate.Slow, 
                GrowthRate.Slow
            );
            AddShopItem(
                sprites.First(s => s.name == "crops_113"),
                "Chilli",
                12,
                2,
                "Deters animals when eaten",
                "A spicy treat for some, and a world of pain for others.",
                GrowthRate.Medium,
                GrowthRate.Fast
            );
            AddShopItem(
                sprites.First(s => s.name == "crops_195"),
                "Cauliflower", 
                103,
                30,
                "",
                "Delicious when oven-roasted with cheese.",
                GrowthRate.Medium,
                GrowthRate.Medium
            );
            Resources.UnloadUnusedAssets();
        }

        private void AddShopItem(
            Sprite icon, string itemName, int price, int yield, string power, string flavourText, GrowthRate growthRate,
            GrowthRate fruitingRate)
        {
            // Set up item template
            VisualElement ui = _itemTemplate.Instantiate();
            ui.Q("plant_icon").style.backgroundImage = new StyleBackground(icon);
            ui.Q<Label>("plant_name").text = itemName;
            ui.Q<Label>("price").text = $"${price}";
            ui.RegisterCallback<ClickEvent>(_ =>
            {
                // TODO(placeables): begin placing
                print($"Clicked on a {itemName}");
            });

            // Set up tooltip
            VisualElement tooltip = _itemTooltipTemplate.Instantiate();
            tooltip.Q<Label>("plant_name").text = itemName;
            tooltip.Q<Label>("cost").text = $"${price}";
            tooltip.Q<Label>("yield").text = $"${yield}";
            tooltip.Q<Label>("flavour").text = flavourText;

            var powerLabel = tooltip.Q<Label>("power");
            powerLabel.enableRichText = true;
            
            powerLabel.text = power.Length > 0 ? $"<u>{power}</u>" : "";

            var growth = tooltip.Q<Label>("growth");
            growth.text = growthRate.Text();
            growth.style.color = growthRate.Color();

            var fruiting = tooltip.Q<Label>("fruiting");
            fruiting.text = fruitingRate.Text();
            fruiting.style.color = fruitingRate.Color();

            // Add tooltip to item, and item to list
            ui.AddManipulator(new ShopItemTooltipManipulator(_tooltipManipulator, tooltip));
            _root.Q("shop_list").Add(ui);
        }
    }
}
