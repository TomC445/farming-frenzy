using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

namespace Code.Scripts.Plants.Powers
{
    public class CornPower : Power
    {
        private const float Distance = 9.9f;
        public const int EffectPercent = 5;
        public const int MaxEffectPercent = 50;

        private BoxCollider2D _vertical;
        private BoxCollider2D _horizontal;

        private void Start()
        {
            var colliders = gameObject.GetComponents<BoxCollider2D>();
            _vertical = colliders[0];
            _horizontal = colliders[1];

            _vertical.size = new Vector2(0.9f, Distance);
            _horizontal.size = new Vector2(Distance, 0.9f);
        }

        private void OnDrawGizmos()
        {
            if (!_vertical) return;
            DrawOneGizmo(_vertical);
            DrawOneGizmo(_horizontal);
        }

        private static void DrawOneGizmo(BoxCollider2D axis)
        {
            var rotationMatrix = Matrix4x4.TRS(axis.transform.position, axis.transform.rotation, axis.transform.lossyScale);
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawWireCube(axis.offset, axis.size);
        }

        /// <summary>
        ///  Calculate the corn fruiting modifier for a given place (either tile or plant)
        /// </summary>
        /// <param name="place"></param>
        /// <param name="plant"></param>
        /// <returns></returns>
        public static float CalculateCornFruitingModifier(Collider2D place)
        {
            var collisions = new List<Collider2D>();
            
            place.Overlap(collisions);
 
            var numberCorn = collisions
                .Select(other => other.gameObject.GetComponent<CornPower>())
                .NotNull()
                .Distinct()
                .Count();

            return 1.0f + Math.Min(MaxEffectPercent, numberCorn * EffectPercent) / 100.0f;
        }

    }
}