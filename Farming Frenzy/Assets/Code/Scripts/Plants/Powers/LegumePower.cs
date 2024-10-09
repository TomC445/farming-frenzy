using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Code.Scripts.Plants.Powers
{
    public abstract class LegumePower : Power
    {
        private CircleCollider2D _aoe;
        protected abstract float Radius { get; }
        protected abstract float EffectStrength { get; }

        public const int MaxEffectPercent = 50;

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

        /// <summary>
        ///  Calculate the growth modifier for a given place (either tile or plant)
        /// </summary>
        /// <param name="place"></param>
        /// <returns></returns>
        public static float CalculateGrowthModifier(Collider2D place)
        {
            var collisions = new List<Collider2D>();
            place.Overlap(collisions);

            var rate = collisions
                .Select(other => other.gameObject.GetComponent<LegumePower>())
                .NotNull()
                .Select(legume => legume.EffectStrength)
                .Sum();

            return Math.Min(1.0f + MaxEffectPercent / 100.0f, 1.0f + rate);
        }
    }
}
