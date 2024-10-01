using System.Collections.Generic;
using UnityEngine;

public class PlantManager : MonoBehaviour
{
    #region Editor Fields
    [SerializeField] private List<PlantData> _plantData;
    #endregion

    #region Properties
    #endregion

    #region Singleton
    public static PlantManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Methods
    public PlantData GetPlantData(string plantName)
    {
        return _plantData.Find(plant => plant.name == plantName);
    }
    #endregion
}
