using System;
using System.Collections;
using Code.Scripts.Managers;
using Code.Scripts.Plants;
using Code.Scripts.Plants.Powers;
using Code.Scripts.Player;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
using Debug = System.Diagnostics.Debug;
using Random = UnityEngine.Random;

namespace Code.Scripts.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAgent : MonoBehaviour
    {
        private static readonly int X = Animator.StringToHash("X");
        private static readonly int Y = Animator.StringToHash("Y");
        private static readonly int Movement = Animator.StringToHash("Movement");

        #region Editor Fields
        [SerializeField] private float _health = 3;
        [SerializeField] private float _maxHealth = 3;
        #endregion

        #region Properties
        private Transform _plantTransform;
        private NavMeshAgent _agent;
        private Animator _agentAnimator;
        [CanBeNull] private Transform _target;
        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rigidbody;
        public Transform _spawnPoints;
        private enum State { Hungry, Eating, Scared}
        private State _currentState;
        [CanBeNull] private Coroutine _eatingCoroutine;
        private Renderer _renderer;

        public bool CanAttack => _currentState != State.Scared && _health >= 0;
        private const float Damage = 5;
        private float _timeToNextPlay;
        private float _timer;
        private AudioManager _audioManager;
        private bool playSFX;
        private bool firstUpdate;
        private GameManager _gameManager;

        #endregion

        #region Methods
        private void Start()
        {
            _audioManager = AudioManager.Instance;
            _gameManager = GameObject.Find("[GameManager]").GetComponent<GameManager>();
            firstUpdate = false;
            _timeToNextPlay = Random.Range(2,9);
            _audioManager.PlayRandomGoatNoise();
            _plantTransform = GameObject.Find("Plants").transform;
            _spawnPoints = GameObject.Find("EnemySpawnPositions").transform;
            _agent = GetComponent<NavMeshAgent>();
            _agentAnimator = GetComponent<Animator>();
            _agent.updateRotation = false;
            _agent.updateUpAxis = false;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _currentState = State.Hungry; //when spawning in for the first time the animal is hungry
            _health = _maxHealth;
            _rigidbody = GetComponentInChildren<Rigidbody2D>();
            _renderer = GetComponent<Renderer>();
        }


        private static Transform GetRandomChild(Transform parent)
        {
            if (parent.childCount == 0)
            {
                return null;
            }
            var randomIndex = Random.Range(0, parent.childCount);
            return parent.GetChild(randomIndex);
        }

        private void Update()
        {

            if(!firstUpdate) {
                if(_gameManager._goats > 10) {
                    float randNum = Random.Range(1,_gameManager._goats+1);
                    if(randNum > 10) playSFX = false;
                    else playSFX = true;
                }
                firstUpdate = true;
            }
            var direction = _agent.velocity.normalized;
            _agentAnimator.SetFloat(X, direction.x);
            _agentAnimator.SetFloat(Y, direction.y);
            _agentAnimator.SetBool(Movement, direction.magnitude > 0);
            _spriteRenderer.sortingOrder = 10000 - Mathf.CeilToInt(transform.position.y);

            if(playSFX){
                _timer += Time.deltaTime;
                if(_timer > _timeToNextPlay) {
                    _audioManager.PlayRandomGoatNoise();
                    _timeToNextPlay = Random.Range(2,9);
                    _timer = 0f;
                }
            }

            lock (this)
            {
                switch (_currentState)
                {
                    case State.Hungry:
                    {
                        AcquirePlantTarget();
                        if (!_target)
                        {
                            RunAway();
                        } else
                        {
                            _agent.SetDestination(_target.position);
                            _currentState = State.Eating;
                        }

                        break;
                    }
                    case State.Eating:
                    {
                        if(!_target)
                        {
                            _currentState = State.Hungry;
                        }

                        break;
                    }
                    case State.Scared when !_target:
                        RunAway();
                        break;
                    case State.Scared when _agent.remainingDistance > 0.1f && _renderer.isVisible:
                        break;
                    case State.Scared:
                        _agent.isStopped = true;
                        Destroy(gameObject);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            }
        }

        private void AcquirePlantTarget()
        {
            _target = GetRandomChild(_plantTransform);
        }

        private void RunAway()
        {
            lock (this)
            {
                print("Running away");
                _currentState = State.Scared;
                _rigidbody.simulated = false;
                CancelEating();

                Transform closest = null;
                var closestDist = float.PositiveInfinity;
                foreach (Transform child in _spawnPoints)
                {
                    var dist = Vector3.Distance(transform.position, child.position);
                    if (dist >= closestDist) continue;
    
                    closest = child;
                    closestDist = dist;
                }
            
                Debug.Assert(closest != null, nameof(closest) + " != null");
            
                _target = closest;
                _agent.SetDestination(closest.position);
                _audioManager.PlaySFX("goatScared");
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="amount"></param>
        /// <returns>True if the animal is now running away</returns>
        private bool TakeDamage(int amount)
        {
            _health -= amount;
            print($"Goat took {amount} damage! HP = {_health}");

            if (_health >= 0) return false;

            RunAway();
            return true;
        }

        private void OnMouseEnter()
        {
            PlayerController.Instance.StartContextualCursor(PlayerController.CursorState.Spray);
        }

        private void OnMouseExit()
        {
            PlayerController.Instance.EndContextualCursor(PlayerController.CursorState.Spray);
        }

        private void OnMouseDown()
        {
            if(PlayerController.Instance.CurrentlyActiveCursor == PlayerController.CursorState.Spray)
            {
                TrySpray();
            }
        }

        public void TrySpray()
        {
            print($"I am goat; _health = {_health}; _state = {_currentState}; _target = {_target?.gameObject}; stopped = {_agent.isStopped}");

            if (!CanAttack) return;
            if (!PlayerController.Instance.TryPurchase(3)) return;

            // TODO make a "-$2" floating text
            TakeDamage(5);
        }
        #endregion

        public void StartAttacking(Plant plant)
        {
            lock (this)
            {
                if (!CanAttack) return;
                if (_eatingCoroutine != null) return;
            
                print($"Going to start attacking {plant.PlantName}, because state = {_currentState}");

                _target = plant.transform;
                _agent.ResetPath();
                _agent.SetDestination(_target.position);
                _currentState = State.Eating;
                _eatingCoroutine = StartCoroutine(DoEating(plant));
            }
        }

        private void CancelEating()
        {
            lock (this)
            {
                if (_eatingCoroutine != null)
                {
                    StopCoroutine(_eatingCoroutine);
                }

                _eatingCoroutine = null;
                _agent.isStopped = false;
            }
        }

        private IEnumerator DoEating(Plant plant)
        {
            var spiky = plant.GetComponentInChildren<SpikyPower>();
            print($"Spike damage: {spiky?.Damage ?? 0}. Plant is {plant.PlantName}");

            // Force the agent to stop and eat current plant
            _agent.isStopped = true;
            yield return new WaitForSeconds(0.2f);
            _agent.isStopped = false;
            yield return new WaitForSeconds(2f);

            if(playSFX) _audioManager.PlaySFX("goatEating");
            

            if(playSFX) _audioManager.PlaySFX("goatEating");
            
            while (true)
            {
                lock (this)
                {
                    var plantDies = !plant || plant.TakeDamage(Damage);
                    var animalRunsAway = spiky && TakeDamage(spiky.Damage);
    
                    if (plantDies)
                    {
                        _target = null;
                        _currentState = State.Hungry;
                        _eatingCoroutine = null;
                    }
    
                    if (animalRunsAway)
                    {
                        RunAway();
                        _eatingCoroutine = null;
                    }

                    if (animalRunsAway || plantDies)
                    {
                        yield break;
                    }
                }

                yield return new WaitForSeconds(2f);
            }
        }
    }
}
