using UnityEngine;

namespace Code.Scripts.Plants
{
    [CreateAssetMenu(menuName = "Farming Frenzy/PlantData")]
    public class PlantData : ScriptableObject
    {
        public Sprite[] _plantSprites;
        public float _maturationRate = 1;
        public float _fruitingRate = 1;
        public float _goldGenerationFactor = 1;
        public float _dryoutRate;
        public float _maturationCycle;
        public float _fruitingCycle;
        public string _name;
        public string _flavourText;
        public string _powerDescription;

        [Range(1,3)]
        public int _tier;

        public int _goldGenerated;
        public float _health;
        public bool _indestructible;
        public int _price;

        public GrowthRate GrowthRateBand
        {
            // Just some example values
            get {
                if (_maturationCycle <= 5)
                {
                    return GrowthRate.Fast;
                }

                return _maturationCycle <= 15 ? GrowthRate.Medium : GrowthRate.Slow;
            }
        }

        public GrowthRate FruitingRateBand
        {
            // Just some example values
            get {
                if (_fruitingCycle <= 3)
                {
                    return GrowthRate.Fast;
                }

                return _maturationCycle <= 7 ? GrowthRate.Medium : GrowthRate.Slow;
            }
        }


    }
}
