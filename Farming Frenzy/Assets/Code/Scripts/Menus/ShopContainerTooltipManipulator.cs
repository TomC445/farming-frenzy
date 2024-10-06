using UnityEngine.UIElements;

namespace Code.Scripts.Menus
{
    /// <summary>
    /// The `ShopContainerTooltipManipulator` works in conjunction with the `ShopItemTooltipManipulator` to display
    /// tooltips (defined by `UI/shop_item_tooltip.uxml) for items in the shop. The Container manipulator is attached
    /// to the shop root `VisualElement` (defined by `UI/shop.uxml`), whilst the `Item` manipulator is attached to each
    /// individual item in the shop (defined by `UI/shop_item.uxml`).
    /// 
    /// Unfortunately, it is not easy to have the manipulator only on each item. This is because of the following:
    /// #1. We want the tooltip to follow the mouse. This is done by setting its position (`top` and `left` in USS).
    /// #2. The `MouseMoveEvent` for a target `VisualElement` is relative to its own position.
    /// #3. We can't have the tooltip be the child of the shop item itself, or other shop items overlap it (there is no
    ///     z-index in UI toolkit). Therefore, the tooltip must be the child of the shop root visual element itself.
    /// #4. Given #3 (tooltip must be child of root), it is difficult to implement #1 (following mouse), because of #2
    ///     (`MouseMoveEvent` is relative to parent)
    ///
    /// The solution taken with this implementation is to have the root element control the tooltip's position and
    /// whether it is shown or hidden, and then have the child elements (shop items) control the tooltip's content.
    /// </summary>
    public class ShopContainerTooltipManipulator : Manipulator
    {
        private VisualElement _tooltipContainer;
        private VisualElement _currentTooltip;
        private bool _inRoot;

        public bool MouseInShop => _inRoot;

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseEnterEvent>(MouseEnterRoot);
            target.RegisterCallback<MouseLeaveEvent>(MouseLeaveRoot);
            target.RegisterCallback<MouseMoveEvent>(MouseMove);
            target.Q("after_shop").RegisterCallback<MouseEnterEvent>(MouseEnterAfterShop);
            target.Q("after_shop").RegisterCallback<MouseLeaveEvent>(MouseLeaveAfterShop);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseEnterEvent>(MouseEnterRoot);
            target.UnregisterCallback<MouseLeaveEvent>(MouseLeaveRoot);
            target.UnregisterCallback<MouseMoveEvent>(MouseMove);
            target.Q("after_shop").UnregisterCallback<MouseEnterEvent>(MouseEnterAfterShop);
            target.Q("after_shop").UnregisterCallback<MouseLeaveEvent>(MouseLeaveAfterShop);
        }
        
        private void MouseEnterAfterShop(MouseEnterEvent evt)
        {
            _tooltipContainer.style.visibility = Visibility.Hidden;
        }

        private void MouseLeaveAfterShop(MouseLeaveEvent evt)
        {
            _tooltipContainer.style.visibility = _inRoot ? Visibility.Visible : Visibility.Hidden;
        }

        
        public void SetTooltip(VisualElement tooltip)
        {
            if (_currentTooltip != null)
            {
                _tooltipContainer.Remove(_currentTooltip);
            }
            
            _currentTooltip = tooltip;

            if (tooltip != null)
            {
                _tooltipContainer.Add(_currentTooltip);
            }
        }
        
        private void MouseEnterRoot(MouseEnterEvent e)
        {
            if (_tooltipContainer == null)
            {
                _tooltipContainer = new VisualElement {
                    style = {
                        position = Position.Absolute,
                        left = 0,
                        top = 0
                    }
                };

                target.Add(_tooltipContainer);
            }

            _inRoot = true;
            _tooltipContainer.style.left = e.localMousePosition.x - _tooltipContainer.contentRect.width;
            _tooltipContainer.style.top = e.localMousePosition.y + 25;
            _tooltipContainer.style.visibility = Visibility.Visible;
            _tooltipContainer.BringToFront();
        }
        
        private void MouseMove(MouseMoveEvent e)
        {
            if (_tooltipContainer == null) return;
            
            _tooltipContainer.style.left = e.localMousePosition.x - _tooltipContainer.contentRect.width;
            _tooltipContainer.style.top = e.localMousePosition.y + 25;
        }

        private void MouseLeaveRoot(MouseLeaveEvent e)
        {
            _inRoot = false;
            _tooltipContainer.style.visibility = Visibility.Hidden;
        }
    }
}
