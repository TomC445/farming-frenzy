using System;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Code.Scripts.Plants.Powers
{
    public abstract class Power : MonoBehaviour {}

    [Serializable]
    public enum PowerKind
    {
        None = 1,
        Clover = 2,
        Corn = 3,
    }

    namespace PowerExtension
    {
        public static class PowerExtensions
        {
            private static readonly Object CloverPowerPrefab = Resources.Load("Clover Power");
            private static readonly Object CornPowerPrefab = Resources.Load("Corn Power");

            public static void AddTo(this PowerKind kind, GameObject gameObject)
            {
                var pos = gameObject.transform.position;

                _ = kind switch
                {
                    PowerKind.Clover => Object.Instantiate(CloverPowerPrefab, pos, Quaternion.identity),
                    PowerKind.Corn => Object.Instantiate(CornPowerPrefab, pos, Quaternion.identity),
                    PowerKind.None => null,
                    _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
                };
            }

            public static string Text(this PowerKind kind) => kind switch
            {
                PowerKind.Clover => $"Nearby plants heal and grow\n{CloverPower.EffectPercent}% " +
                                    $"faster (up to {LegumePower.MaxEffectPercent}%)",
                PowerKind.Corn => $"Other corn plants in a row\nor column with this one\nfruit" +
                                  $" {CornPower.EffectPercent}% faster (up to {CornPower.MaxEffectPercent}%)",
                PowerKind.None => "",
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
        }
    }
}
