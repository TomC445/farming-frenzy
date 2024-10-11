using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
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
        public Transform _target;
        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _collider;
        public Transform _spawnPoints;
        private enum State { Hungry, Eating, Scared}
        private State _currentState;

        public bool CanAttack => _currentState != State.Scared;
        public float Damage => 5;

        #endregion

        #region Methods
        private void Start()
        {
            _plantTransform = GameObject.Find("Plants").transform;
            _spawnPoints = GameObject.Find("EnemySpawnPositions").transform;
            _agent = GetComponent<NavMeshAgent>();
            _agentAnimator = GetComponent<Animator>();
            _agent.updateRotation = false;
            _agent.updateUpAxis = false;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<BoxCollider2D>();
            _currentState = State.Hungry; //when spawning in for the first time the animal is hungry
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
            var direction = _agent.velocity.normalized;
            _agentAnimator.SetFloat(X, direction.x);
            _agentAnimator.SetFloat(Y, direction.y);
            _agentAnimator.SetBool(Movement, direction.magnitude > 0);
            _spriteRenderer.sortingOrder = 10000 - Mathf.CeilToInt(transform.position.y);
        
            switch (_currentState)
            {
                case State.Hungry:
                {
                    AcquirePlantTarget();
                    if (!_target)
                    {
                        print("No plant, running away");
                        RunAway();
                    } else
                    {
                        print("Acquired target");
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
                case State.Scared when !_target || _agent.remainingDistance > 0.1f:
                    break;
                case State.Scared:
                    _agent.isStopped = true;
                    Destroy(gameObject);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AcquirePlantTarget()
        {
            _target = GetRandomChild(_plantTransform);
        }

        private void RunAway()
        {
            print("Running away");
            var randomIndex = Random.Range(0, _spawnPoints.childCount);
            var spawnPoint = _spawnPoints.GetChild(randomIndex);
            _target = spawnPoint;
            _agent.SetDestination(spawnPoint.position);
            _currentState = State.Scared;
        }

        /// <summary>
        /// </summary>
        /// <param name="amount"></param>
        /// <returns>True if the animal is now running away</returns>
        public bool TakeDamage(int amount)
        {
            print("Taking damage");
            _health -= amount;

            if (!(_health <= 0)) return false;

            RunAway();
            return true;
        }

        private void OnMouseDown()
        {
            if(PlayerController.Instance._currentState == PlayerController.CursorState.Spray)
            {
                TakeDamage(1);
            }
        }
        #endregion
    }
}
