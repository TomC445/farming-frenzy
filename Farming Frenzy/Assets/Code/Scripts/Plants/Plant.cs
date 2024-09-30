using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.Scripts.Plants
{
    public class Plant : MonoBehaviour, IPointerClickHandler
    {
        private readonly PlantData _data;
        private Sprite _currentSprite;
        private float _time;
        private int _currentState; // 0 = seedling, 1 = mature, 2 = fruiting

        public Plant(PlantData pdata) {
            _data = pdata;
            _currentSprite = _data._plantSprites[0];
            _currentState = 0;
            _time = Time.time;
        }

        private void Update()
        {
            UpdateState();
        }

        private void UpdateState() {
            var currTime = Time.time;
            var seconds = currTime-_time;
            switch (_currentState) {
                case 0:
                    if(_data._maturationRate * seconds > _data._maturationCycle) {
                        _currentState = 1;
                        _currentSprite =  _data._plantSprites[1];
                        _time = Time.time;
                    }
                    break;
                case 1:
                    if(_data._fruitingRate * seconds > _data._fruitingCycle) {
                        _currentState = 2;
                        _currentSprite = _data._plantSprites[2];
                    }
                    break;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_currentState != 2) return;
            //HARVEST AND UPDATE GOLD IN GAME MANAGER
            _currentState = 1;
            _currentSprite = _data._plantSprites[1];
            _time = Time.time;
        }
    }
}
