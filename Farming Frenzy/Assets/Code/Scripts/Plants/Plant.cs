using System;
using System.Linq;
using Code.Scripts.Enemy;
using Code.Scripts.Plants.GrowthStateExtension;
using Code.Scripts.Plants.Powers;
using Code.Scripts.Plants.Powers.PowerExtension;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.Scripts.Plants
{
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
        private float _fruitingRate;
        private float _health;
        private float _nextHealTime;
        private float MaxHealth => _data._health;
        private int SecsToNextStage
        {
            get
            {
                return _state switch
                {
                    GrowthState.Seedling => Math.Max(0, (int)((_data._maturationCycle - _secsSinceGrowth) / _growthRate)),
                    GrowthState.Mature => _data._cannotHarvest ? -1 : Math.Max(0, (int)((_data._fruitingCycle - _secsSinceGrowth) / _fruitingRate)),
                    _ => 0
                };
            }
        }

        private bool CanHarvestNow => !_data._cannotHarvest && _state == GrowthState.Fruited;

        public string PlantName => _data.name;

        public string StatusRichText => _state.StatusRichText(SecsToNextStage, _data._goldGenerated);

        public delegate void HoverInEvent(Plant plant);

        public delegate void HoverOutEvent(Plant plant);
        public event HoverInEvent OnHoverIn;
        public event HoverOutEvent OnHoverOut;
        private bool _isMouseOverPlant = false;
        #endregion

        #region Methods
        private void Awake()
        {
            _plantSpriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            UpdateState();
            if(Input.GetMouseButton(0) && _isMouseOverPlant)
            {
                if(PlayerController.Instance._currentState == PlayerController.CursorState.Scythe)
                {
                    HarvestPlant();
                } else if (PlayerController.Instance._currentState == PlayerController.CursorState.Shovel)
                {
                    DigPlant();
                }
                
            }
        }

        public void InitPlant(PlantData pdata)
        {
            _data = pdata;
            _state = GrowthState.Seedling;
            _secsSinceGrowth = 0.0f;
            Collider = GetComponent<BoxCollider2D>();
            _health = pdata._health;
            if (_data._isTree)
            {
                Collider.size = new Vector2(3, 2);
                Collider.offset = new Vector2(0, 0.5f);
            }

            _plantSpriteRenderer.sprite = _data._growthSprite.First();
            GetComponent<SpriteRenderer>().sortingOrder = 10000 - Mathf.CeilToInt(gameObject.transform.position.y);
        }

        private void UpdateState()
        {
            _growthRate = LegumePower.CalculateGrowthModifier(Collider);
            _fruitingRate = PlantName == "Corn" ? CornPower.CalculateCornFruitingModifier(Collider) : 1.0f;

            if (Time.time >= _nextHealTime)
            {
                _nextHealTime = Time.time + 2.0f;
                _health = Math.Min(MaxHealth, _health + 2.0f * _growthRate);
            }

            switch (_state)
            {
                case GrowthState.Seedling:
                    _secsSinceGrowth += Time.deltaTime * _growthRate;

                    if (_secsSinceGrowth >= _data._maturationCycle)
                    {
                        // Plant has finished growing!
                        _state = GrowthState.Mature;
                        _data.power.AddTo(gameObject); // Power only enabled when the plant is grown
                        _secsSinceGrowth = 0;
                    } else {
                        // Plant is still growing
                        var spriteIndex = Mathf.FloorToInt(_secsSinceGrowth * _data._maturationSprite.Length / _data._maturationCycle);
                        _plantSpriteRenderer.sprite = _data._maturationSprite[spriteIndex];
                    }
                    break;
                case GrowthState.Mature:
                    // This plant does not fruit
                    if (_data._cannotHarvest)
                    {
                        break;
                    }

                    _secsSinceGrowth += Time.deltaTime * _fruitingRate;

                    if (_secsSinceGrowth <= _data._fruitingCycle)
                    {
                        var spriteIndex = Mathf.FloorToInt(_secsSinceGrowth * _data._growthSprite.Length / _data._fruitingCycle);
                        if (spriteIndex > 0) { _plantSpriteRenderer.sprite = _data._growthSprite[spriteIndex - 1]; }
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
                case GrowthState.Fruited when _secsSinceGrowth >= 3.0:
                    Harvest();
                    break;
                case GrowthState.Fruited:
                    _secsSinceGrowth += Time.deltaTime;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HarvestPlant()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) // TODO placeholder control
            {
                if (CanHarvestNow) Harvest();
                Destroy(gameObject);
                PlayerController.Instance.IncreaseMoney(_data._price / 2);
                AudioManager.Instance.PlaySFX("digMaybe"); // TODO placeholder sound
            }
            else if (CanHarvestNow)
            {
                AudioManager.Instance.PlaySFX("picking");
                Harvest();
            }
        }

        public void Harvest()
        {
            if(!CanHarvestNow)
            {
                return;
            }
            PlayerController.Instance.IncreaseMoney(_data._goldGenerated);
            _state = GrowthState.Harvested;
            _plantSpriteRenderer.sprite = _data._harvestedSprite;
            _secsSinceGrowth = 0.0f;
        }

        private void DigPlant()
        {
            Destroy(gameObject);
        }

        private void OnMouseEnter()
        {
            _isMouseOverPlant = true;
            OnHoverIn?.Invoke(this);
        }

        private void OnMouseExit()
        {
            _isMouseOverPlant = false;
            OnHoverOut?.Invoke(this);
        }

        public bool TakeDamage(float amount)
        {
            _health -= amount;
            print($"{PlantName} took {amount} damage! HP = {_health}");
            if (_health > 0) return false;

            Destroy(gameObject);
            return true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Enemy")) return;

            var enemy = other.GetComponentInParent<EnemyAgent>();
            if (!enemy.CanAttack) return;

            enemy.StartAttacking(this);
        }
        #endregion
    }
}
