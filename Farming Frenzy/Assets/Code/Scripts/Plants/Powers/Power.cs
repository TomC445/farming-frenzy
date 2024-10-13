using System;
using Code.Scripts.Managers;
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
        Nettle = 4,
        Bean = 5
    }

    namespace PowerExtension
    {
        public static class PowerExtensions
        {
            private static readonly Object CloverPowerPrefab = Resources.Load("Clover Power");
            private static readonly Object BeanPowerPrefab = Resources.Load("Bean Power");
            private static readonly Object CornPowerPrefab = Resources.Load("Corn Power");
            private static readonly Object NettlePowerPrefab = Resources.Load("Nettle Power");

            public static void AddTo(this PowerKind kind, GameObject gameObject)
            {
                var pos = gameObject.transform.position;
                var id = Quaternion.identity;
                var parent = gameObject.transform;

                var prefab = kind switch
                {
                    PowerKind.Clover => CloverPowerPrefab,
                    PowerKind.Bean => BeanPowerPrefab,
                    PowerKind.Corn => CornPowerPrefab,
                    PowerKind.Nettle => NettlePowerPrefab,
                    PowerKind.None => null,
                    _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
                };

                if (prefab)
                {
                    Object.Instantiate(prefab, pos, id, parent);
                }
            }

            public static string Text(this PowerKind kind) => kind switch
            {
                PowerKind.Clover => $"Nearby plants heal \n{CloverPower.EffectPercent}% " +
                                    $"faster (up to {LegumePower.MaxEffectPercent}%)",
                PowerKind.Bean => $"Nearby plants heal \n{BeanPower.EffectPercent}% " +
                                    $"faster (up to {LegumePower.MaxEffectPercent}%)",
                PowerKind.Corn => "Other corn plants in a row\nor column with this one\nfruit" +
                                  $" {CornPower.EffectPercent}% faster (up to {CornPower.MaxEffectPercent}%)",
                PowerKind.Nettle => "Animals take damage when\nthey eat this plant",
                PowerKind.None => "",
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };

            [CanBeNull]
            public static PlantAoeState AoeState(this PowerKind kind)
            {
                switch (kind)
                {
                    case PowerKind.Bean:
                    case PowerKind.Clover:
                        return PlantManager.Instance.LegumePowerAoe;
                    case PowerKind.Corn:
                        return PlantManager.Instance.CornPowerAoe;
                    case PowerKind.None:
                    case PowerKind.Nettle:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
                }

                return null;
            }
        }
    }
}
