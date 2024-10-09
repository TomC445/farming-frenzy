using System.Collections;
using System;
using Code.Scripts.Plants.GrowthStateExtension;
using Code.Scripts.Plants.Powers;
using Code.Scripts.Plants.Powers.PowerExtension;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.Scripts.Plants
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
    private float _health;
    private bool _readyToHarvest;
    private Coroutine _damageCoroutine;
    #endregion

    #region Methods
    private void Awake()
    {
        _plantSpriteRenderer = GetComponent<SpriteRenderer>();   
    }

    void Update()
    {
        UpdateState();
        if(_health <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void InitPlant(PlantData _pdata)
    {
        data = _pdata;
        state = GrowthState.Start;
        time = Time.time;
        _health = _pdata._health;
        if(data._isTree)
    public class Plant : MonoBehaviour
    {
        #region Properties
        private PlantData _data;
        private Sprite _currentSprite;
        private float _secsSinceGrowth;
        private GrowthState _state;
        private SpriteRenderer _plantSpriteRenderer;
        private int _growthSpriteIndex;
        private bool _readyToHarvest;
        public BoxCollider2D Collider { get; private set; }
        private float _growthRate;
    
        private int SecsToNextStage
        {
            get
            {
                return _state switch
                {
                    GrowthState.Seedling => Math.Max(0, (int)((_data._maturationCycle - _secsSinceGrowth) / _growthRate)),
                    GrowthState.Mature => _data._cannotHarvest ? -1 : Math.Max(0, (int)(_data._fruitingCycle - _secsSinceGrowth)),
                    _ => 0
                };
            }
        }

        public string PlantName => _data.name;

        public string StatusRichText => _state.StatusRichText(SecsToNextStage, _data._goldGenerated);

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

        public void InitPlant(PlantData pdata)
        {
            _data = pdata;
            _state = GrowthState.Seedling;
            _secsSinceGrowth = 0.0f;
            Collider = GetComponent<BoxCollider2D>();

            if(_data._isTree)
            {
                Collider.size = new Vector2(3, 2);
                Collider.offset = new Vector2(0, 0.5f);

            }

            GetComponent<SpriteRenderer>().sortingOrder = 10000 - Mathf.CeilToInt(gameObject.transform.position.y);
        }

        private void UpdateState()
        {
            _growthRate = LegumePower.CalculateGrowthModifier(Collider);

            switch (_state)
            {
                case GrowthState.Seedling:
                    _secsSinceGrowth += Time.deltaTime * _growthRate; 

                    if (_secsSinceGrowth <= _data._maturationCycle)
                    {
                        var spriteIndex = Mathf.FloorToInt(_secsSinceGrowth * _data._maturationSprite.Length / _data._maturationCycle);
                        _plantSpriteRenderer.sprite = _data._maturationSprite[spriteIndex];
                    } else 
                    {
                        _state = GrowthState.Mature;
                        _data.power.AddTo(gameObject); // Power only enabled when the plant is grown
                        _secsSinceGrowth = 0; 
                    }
                    break;
                case GrowthState.Mature:
                    // This plant does not fruit
                    if (_data._cannotHarvest)
                    {
                        break;
                    }

                    _secsSinceGrowth += Time.deltaTime; 

                    if (_secsSinceGrowth <= _data._fruitingCycle)
                    {
                        var spriteIndex = Mathf.FloorToInt(_secsSinceGrowth * _data._growthSprite.Length / _data._fruitingCycle);
                        if(spriteIndex > 0) { _plantSpriteRenderer.sprite = _data._growthSprite[spriteIndex - 1];}
                    }
                    else
                    {
                        _plantSpriteRenderer.sprite = _data._growthSprite[^1];
                        _state = GrowthState.Fruited;
                        _secsSinceGrowth = 0;
                    }
                    break;
                case GrowthState.Harvested:
                    _state = GrowthState.Mature;
                    break;
                case GrowthState.Fruited:
                default:
                    break;
            }
        }

        public void OnMouseDown()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            if (_data._cannotHarvest) return;
            if (_state != GrowthState.Fruited) return;

            // HARVEST AND UPDATE GOLD IN GAME MANAGER
            PlayerController.Instance.IncreaseMoney(_data._goldGenerated);
            _state = GrowthState.Harvested;
            _plantSpriteRenderer.sprite = _data._harvestedSprite;
            _secsSinceGrowth = 0.0f;
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Enemy"))
        {
            if(_damageCoroutine != null)
            {
                StopCoroutine(_damageCoroutine);

            }
            _damageCoroutine = StartCoroutine(TakeAnimalDamage());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (_damageCoroutine != null)
            {
                StopCoroutine(_damageCoroutine);

            }
            _damageCoroutine = StartCoroutine(TakeAnimalDamage());
        }
    }

    private IEnumerator TakeAnimalDamage()
    {
        while(true)
        {
            Debug.Log($"Damage: {_health}");
            _health -= 5;
            yield return new WaitForSeconds(1f);
        }
    }
    #endregion
}
}
