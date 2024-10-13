using System.Collections.Generic;
using UnityEngine;

namespace Code.Scripts.Managers
{
    public class PlantManager : MonoBehaviour
    {
        #region Editor Fields
        [SerializeField] private List<PlantData> _plantData;
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

        public readonly PlantAoeState CornPowerAoe = new();
        public readonly PlantAoeState LegumePowerAoe = new();
        #endregion

        #region Methods
        public PlantData GetPlantData(string plantName)
        {
            return _plantData.Find(plant => plant.name == plantName);
        }
        #endregion
    }
}
