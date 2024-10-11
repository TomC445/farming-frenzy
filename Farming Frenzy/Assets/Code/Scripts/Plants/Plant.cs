using System;
using System.Collections;
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
        private Coroutine _damageCoroutine;
        private float _health;

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

        void Update()
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

            GetComponent<SpriteRenderer>().sortingOrder = 10000 - Mathf.CeilToInt(gameObject.transform.position.y);
        }

        private void UpdateState()
        {
            _growthRate = LegumePower.CalculateGrowthModifier(Collider);
            _fruitingRate = PlantName == "Corn" ? CornPower.CalculateCornFruitingModifier(Collider, this) : 1.0f;

            switch (_state)
            {
                case GrowthState.Seedling:
                    _secsSinceGrowth += Time.deltaTime * _growthRate;

                    if (_secsSinceGrowth <= _data._maturationCycle)
                    {
                        var spriteIndex = Mathf.FloorToInt(_secsSinceGrowth * _data._maturationSprite.Length / _data._maturationCycle);
                        _plantSpriteRenderer.sprite = _data._maturationSprite[spriteIndex];
                    }
                    else
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
                case GrowthState.Fruited:
                default:
                    break;
            }
        }

        private void HarvestPlant()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            if (_data._cannotHarvest) return;
            if (_state != GrowthState.Fruited) return;

            // HARVEST AND UPDATE GOLD IN GAME MANAGER
            AudioManager.Instance.PlaySFX("picking");
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

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Enemy")) return;
            if (_damageCoroutine != null)
            {
                StopCoroutine(_damageCoroutine);
            }

            var enemy = other.GetComponent<EnemyAgent>();
            if (!enemy.CanAttack)
            {
                return;
            }
            
            _damageCoroutine = StartCoroutine(TakeAnimalDamage(enemy));
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Enemy")) return;
            if (_damageCoroutine != null)
            {
                StopCoroutine(_damageCoroutine);
            }
            
            var enemy = other.GetComponent<EnemyAgent>();
            
            if (!enemy.CanAttack)
            {
                return;
            }

            _damageCoroutine = StartCoroutine(TakeAnimalDamage(enemy));
        }

        private IEnumerator TakeAnimalDamage(EnemyAgent enemy)
        {
            var spiky = GetComponentInChildren<SpikyPower>();
            print(spiky);
            
            while (true)
            {
                _health -= enemy.Damage;
                if (_health <= 0) Destroy(gameObject);
                print(_health);

                if (spiky)
                {
                    print("damage!");
                    if (enemy.TakeDamage(spiky.Damage))
                    {
                        _damageCoroutine = null;
                        print("Goodbye coroutine");
                        yield break;
                    }
                }
                
                yield return new WaitForSeconds(1f);
            }
        }
        #endregion
    }
}
