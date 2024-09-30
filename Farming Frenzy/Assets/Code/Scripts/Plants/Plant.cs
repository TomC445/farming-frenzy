using UnityEngine;
using UnityEngine.EventSystems;

public class Plant : MonoBehaviour, IPointerClickHandler
{
    private PlantData data;
    private Sprite currentSprite;
    private float time;
    private int currentState; //0 = seedling, 1 = mature, 2 = fruiting

    public Plant(PlantData _pdata) {
        data = _pdata;
        currentSprite = data._plantSprites[0];
        currentState = 0;
        time = Time.time;
    }

    void Update()
    {
        UpdateState();
    }

    private void UpdateState() {
        float currTime = Time.time;
        float seconds = currTime-time;
        switch (currentState) {
            case 0:
                if(data._maturationRate*seconds > data._maturationCycle) {
                    currentState = 1;
                    currentSprite =  data._plantSprites[1];
                    time = Time.time;
                }
            break;
            case 1:
                if(data._fruitingRate*seconds > data._fruitingCycle) {
                    currentState = 2;
                    currentSprite = data._plantSprites[2];
                }
            break;
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        if(currentState == 2) {
            //HARVEST AND UPDATE GOLD IN GAME MANAGER
            currentState = 1;
            currentSprite = data._plantSprites[1];
            time = Time.time; 
        }
    }


}