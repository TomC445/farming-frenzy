namespace Code.Scripts.Managers
{
    public class PlantAoeState
    {
        public delegate void VisibilityChangeEvent(bool visible);

        public VisibilityChangeEvent OnVisibilityChange;
        public bool Visible { get; private set; }
        private bool _isPlanting;
        private bool _isHovering;

        private void RecalculateVisibility()
        {
            var old = Visible;
            Visible = _isHovering || _isPlanting;

            if (Visible != old)
            {
                OnVisibilityChange?.Invoke(Visible);
            }
        }
        
        public void SetPlanting(bool isPlanting)
        {
            _isPlanting = isPlanting;
            RecalculateVisibility();
        }

        public void SetHovering(bool isHovering)
        {
            _isHovering = isHovering;
            RecalculateVisibility();
        }
    }
}