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
    }

    namespace PowerExtension
    {
        public static class PowerExtensions
        {
            private static readonly Object CloverPower = Resources.Load("Clover Power");
            public static void AddTo(this PowerKind kind, GameObject gameObject)
            {
                var pos = gameObject.transform.position;

                _ = kind switch
                {
                    PowerKind.Clover => Object.Instantiate(CloverPower, pos, Quaternion.identity),
                    PowerKind.None => null,
                    _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
                };
            }

            public static string Text(this PowerKind kind) => kind switch
            {
                PowerKind.Clover => "Nearby plants heal and grow\n5% faster (up to 50%)",
                PowerKind.None => "",
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
        }
    }
}
