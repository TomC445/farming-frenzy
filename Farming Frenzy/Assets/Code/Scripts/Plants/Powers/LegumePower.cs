using UnityEngine;

namespace Code.Scripts.Plants.Powers
{
    public abstract class LegumePower : Power
    {
        private CircleCollider2D _aoe;
        protected abstract float Radius { get; }
        public abstract float EffectStrength { get; }

        private void Start()
        {
            _aoe = gameObject.GetComponent<CircleCollider2D>();
            _aoe.radius = Radius;
        }

        private void OnDrawGizmos()
        {
            if (!_aoe) return;

            var rotationMatrix = Matrix4x4.TRS(_aoe.transform.position, _aoe.transform.rotation, _aoe.transform.lossyScale);
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawWireSphere(_aoe.offset, _aoe.radius);
        }
    }
}
