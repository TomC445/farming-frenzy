using System;
using Code.Scripts.Plants;
using Code.Scripts.Plants.GrowthStateExtension;
using UnityEngine;
using UnityEngine.EventSystems;

public class Plant : MonoBehaviour
{
    #region Properties
    private PlantData data;
    private Sprite currentSprite;
    private float time;
    private GrowthState state;
    private SpriteRenderer _plantSpriteRenderer;
    private int _growthSpriteIndex;
    private bool _readyToHarvest;
    
    private int SecsToNextStage
    {
        get
        {
            var currTime = Time.time;
            var seconds = currTime - time;

            return state switch
            {
                GrowthState.Seedling => Math.Max(0, (int)(data._maturationCycle - data._maturationRate * seconds)),
                GrowthState.Mature => data._fruitingRate < 0.0 ? -1 : Math.Max(0, (int)(data._fruitingCycle - data._fruitingRate * seconds)),
                _ => 0
            };
        }
    }

    public string PlantName => data.name;

    public string StatusRichText => state.StatusRichText(SecsToNextStage, data._goldGenerated);

    public delegate void HoverInEvent(Plant plant);

    public delegate void HoverOutEvent(Plant plant);
    public event HoverInEvent OnHoverIn;
    public event HoverOutEvent OnHoverOut;
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
        state = GrowthState.Seedling;
        time = Time.time;
        if(data._isTree)
        {
            GetComponent<BoxCollider2D>().size = new Vector2(3, 2);
            GetComponent<BoxCollider2D>().offset = new Vector2(0, 0.5f);

        }
        GetComponent<SpriteRenderer>().sortingOrder = 10000 - Mathf.CeilToInt(gameObject.transform.position.y);
    }

    private void UpdateState()
    {
        float currTime = Time.time;
        float seconds = currTime - time;
        switch (state)
        {
            case GrowthState.Seedling:
                if (data._maturationRate * seconds <= data._maturationCycle)
                {
                    var spriteIndex = Mathf.FloorToInt((data._maturationRate * seconds * data._maturationSprite.Length) / data._maturationCycle);
                    _plantSpriteRenderer.sprite = data._maturationSprite[spriteIndex];
                } else 
                {
                    state = GrowthState.Mature;
                    time = Time.time;
                }
                break;
            case GrowthState.Mature:
                // This plant does not fruit
                if (data._fruitingCycle < 0.0)
                {
                    break;
                }
                
                if (data._fruitingRate * seconds <= data._fruitingCycle)
                {
                    var spriteIndex = Mathf.FloorToInt((data._fruitingRate * seconds * data._growthSprite.Length) / data._fruitingCycle);
                    if(spriteIndex > 0) { _plantSpriteRenderer.sprite = data._growthSprite[spriteIndex-1];}
                }
                else
                {
                    _plantSpriteRenderer.sprite = data._growthSprite[data._growthSprite.Length-1];
                    state = GrowthState.Fruited;
                }
                break;
            case GrowthState.Harvested:
                state = GrowthState.Mature;
                break;
        }
    }

    public void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        if (data._cannotHarvest)
        {
            return;
        }
        if (state == GrowthState.Fruited)
        {
            //HARVEST AND UPDATE GOLD IN GAME MANAGER
            PlayerController.Instance.IncreaseMoney(data._goldGenerated);
            state = GrowthState.Harvested;
            _plantSpriteRenderer.sprite = data._harvestedSprite;
            time = Time.time;
        }
    }
    
    private void OnMouseEnter()
    {
        OnHoverIn?.Invoke(this);
    }

    private void OnMouseExit()
    {
        OnHoverOut?.Invoke(this);
    }

    #endregion
}

