using UnityEngine;

public class Plant : MonoBehaviour
{
    #region Editor Fields

    #endregion

    #region Properties
    private PlantData data;
    private Sprite currentSprite;
    private float time;
    private enum GrowthState {Start, Growing, Finished, Harvested};
    private GrowthState state;
    private SpriteRenderer _plantSpriteRenderer;
    private int _growthSpriteIndex;
    private bool _readyToHarvest;
    #endregion

    #region Methods
    private void Awake()
    {
        _plantSpriteRenderer = GetComponent<SpriteRenderer>();   
    }

    void Update()
    {
        UpdateState();
    }

    public void InitPlant(PlantData _pdata)
    {
        data = _pdata;
        state = GrowthState.Start;
        time = Time.time;
    }

    private void UpdateState()
    {
        float currTime = Time.time;
        float seconds = currTime - time;
        switch (state)
        {
            case GrowthState.Start:
                if (data._maturationRate * seconds <= data._maturationCycle)
                {
                    var spriteIndex = Mathf.FloorToInt((data._maturationRate * seconds * data._maturationSprite.Length) / data._maturationCycle);
                    _plantSpriteRenderer.sprite = data._maturationSprite[spriteIndex];
                } else 
                {
                    state = GrowthState.Growing;
                    time = Time.time;
                }
                break;
            case GrowthState.Growing:
                if (data._fruitingRate * seconds <= data._fruitingCycle)
                {
                    var spriteIndex = Mathf.FloorToInt((data._fruitingRate * seconds * data._growthSprite.Length) / data._fruitingCycle);
                    if(spriteIndex > 0) { _plantSpriteRenderer.sprite = data._growthSprite[spriteIndex-1];}
                }
                else
                {
                    _plantSpriteRenderer.sprite = data._growthSprite[data._growthSprite.Length-1];
                    state = GrowthState.Finished;
                }
                break;
            case GrowthState.Harvested:
                state = GrowthState.Growing;
                break;
        }
    }

    public void OnMouseDown()
    {
        if (state == GrowthState.Finished)
        {
            //HARVEST AND UPDATE GOLD IN GAME MANAGER
            PlayerController.Instance.IncreaseMoney(data._goldGenerated);
            state = GrowthState.Harvested;
            _plantSpriteRenderer.sprite = data._harvestedSprite;
            time = Time.time;
        }
    }
    #endregion
}