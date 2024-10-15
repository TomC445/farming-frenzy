using System.Collections.Generic;
using Code.Scripts.Plants.Powers;
using Code.Scripts.Plants.Powers.PowerExtension;
using UnityEngine;

namespace Code.Scripts.Managers
{
    public class PlantManager : MonoBehaviour
    {
        #region Editor Fields
        [SerializeField] private List<PlantData> _plantData;
        [SerializeField] private Camera _camera;
        #endregion

        #region Singleton
        public static PlantManager Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                CornPowerAoe = new PlantAoeState(PowerKind.Corn.PlaceAoeIndicator());
                LegumePowerAoe = new PlantAoeState(PowerKind.Clover.PlaceAoeIndicator());
                BananaPowerAoe = new PlantAoeState(PowerKind.Banana.PlaceAoeIndicator());
                ChiliPowerAoe = new PlantAoeState(PowerKind.Chili.PlaceAoeIndicator());
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public PlantAoeState CornPowerAoe;
        public PlantAoeState LegumePowerAoe;
        public PlantAoeState BananaPowerAoe;
        public PlantAoeState ChiliPowerAoe;
        #endregion

        #region Methods
        public PlantData GetPlantData(string plantName)
        {
            return _plantData.Find(plant => plant.name == plantName);
        }

        private void Update()
        {
            CornPowerAoe.Tick(_camera);
            LegumePowerAoe.Tick(_camera);
            BananaPowerAoe.Tick(_camera);
            ChiliPowerAoe.Tick(_camera);
        }

        #endregion
    }
}
