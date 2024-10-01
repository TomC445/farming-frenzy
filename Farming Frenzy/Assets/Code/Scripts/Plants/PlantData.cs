using UnityEngine;

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

    [Range(1,3)]
    public int _tier;

    public int _goldGenerated;
    public float _health;
    public bool _indestructible;
}