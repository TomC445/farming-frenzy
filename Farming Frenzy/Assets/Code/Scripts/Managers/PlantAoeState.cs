using UnityEngine;

namespace Code.Scripts.Managers
{
    public class PlantAoeState
    {
        public delegate void VisibilityChangeEvent(bool visible);

        public VisibilityChangeEvent OnVisibilityChange;
        public bool Visible { get; private set; }
        private bool _isPlanting;
        private bool _isHovering;
        private readonly GameObject _aoePlacementIndicator;

        public PlantAoeState(GameObject indicator)
        {
            _aoePlacementIndicator = indicator;
            _aoePlacementIndicator.SetActive(false);
        }

        private void RecalculateVisibility()
        {
            var old = Visible;
            Visible = _isHovering || _isPlanting;

            if (Visible != old)
            {
                OnVisibilityChange?.Invoke(Visible);
            }
        }

        public void Tick(Camera camera)
        {
            if (_isPlanting)
            {
                var pos = camera.ScreenToWorldPoint(Input.mousePosition);
                pos.z = 1;
                _aoePlacementIndicator.transform.position = pos;
            }
        }

        public void SetPlanting(bool isPlanting)
        {
            _isPlanting = isPlanting;
            _aoePlacementIndicator.SetActive(isPlanting);
            RecalculateVisibility();
        }

        public void SetHovering(bool isHovering)
        {
            _isHovering = isHovering;
            RecalculateVisibility();
        }
    }
}