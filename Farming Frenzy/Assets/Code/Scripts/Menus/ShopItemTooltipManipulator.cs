using UnityEngine.UIElements;

namespace Code.Scripts.Menus
{
    /// <summary>
    /// The `ShopContainerTooltipManipulator` works in conjunction with the `ShopItemTooltipManipulator` to display
    /// tooltips (defined by `UI/shop_item_tooltip.uxml) for items in the shop. The Container manipulator is attached
    /// to the shop root `VisualElement` (defined by `UI/shop.uxml`), whilst the `Item` manipulator is attached to each
    /// individual item in the shop (defined by `UI/shop_item.uxml`).
    ///
    /// <see cref="ShopContainerTooltipManipulator"/>See `ShopContainerTooltipManipulator` for a more detailed
    /// explanation of how this all works.<see/>
    /// </summary>
    public class ShopItemTooltipManipulator : Manipulator
    {
        private readonly ShopContainerTooltipManipulator _tooltipManipulator;
        private readonly VisualElement _customTooltip;

        public ShopItemTooltipManipulator(ShopContainerTooltipManipulator tooltipManipulator, VisualElement customTooltip)
        {
            _tooltipManipulator = tooltipManipulator;
            _customTooltip = customTooltip;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseEnterEvent>(MouseEnter);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseEnterEvent>(MouseEnter);
        }

        private void MouseEnter(MouseEnterEvent e)
        { 
            _tooltipManipulator.SetTooltip(_customTooltip);   
        }
    }
}
