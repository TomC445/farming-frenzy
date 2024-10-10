using System;
using System.Collections;
using Code.Scripts.Plants;
using Code.Scripts.Plants.Powers;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
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
        private BoxCollider2D _collider;
        public Transform _spawnPoints;
        private enum State { Hungry, Eating, Scared}
        private State _currentState;
        [CanBeNull] private Coroutine _eatingCoroutine;

        public bool CanAttack => _currentState != State.Scared;
        private const float Damage = 5;

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
            _collider = GetComponentInChildren<BoxCollider2D>();
            _currentState = State.Hungry; //when spawning in for the first time the animal is hungry
            _health = _maxHealth;
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

            // Make the hitbox point in the correct direction
            if (direction.magnitude > 0)
            {
                _collider.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
            }

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
        private bool TakeDamage(int amount)
        {
            print("Taking damage");
            _health -= amount;

            if (!(_health <= 0)) return false;

            RunAway();
            return true;
        }

        private void OnMouseDown()
        {
            TakeDamage(1);
        }
        #endregion

        public void StartAttacking(Plant plant)
        {
            var distToPlant = (_collider.transform.position - plant.transform.position).magnitude;
            var distToCurrent = (_collider.transform.position - _target?.transform.position)?.magnitude ?? float.PositiveInfinity;

            if (distToPlant > distToCurrent) return;

            if (_eatingCoroutine != null)
            {
                StopCoroutine(_eatingCoroutine);
            }

            _target = plant.transform;
            _agent.ResetPath();
            _agent.SetDestination(_target.position);
            _currentState = State.Eating;
            _eatingCoroutine = StartCoroutine(DoEating(plant));
        }

        private IEnumerator DoEating(Plant plant)
        {
            var spiky = plant.GetComponentInChildren<SpikyPower>();

            // Force the agent to stop and eat current plant
            _agent.isStopped = true;
            yield return new WaitForSeconds(0.2f);
            _agent.isStopped = false;
            yield return new WaitForSeconds(2f);

            while (true)
            {
                var plantDies = plant.TakeDamage(Damage);
                var animalRunsAway = spiky && TakeDamage(spiky.Damage);
    
                if (plantDies)
                {
                    _target = null;
                    _currentState = State.Hungry;
                    _eatingCoroutine = null;
                    yield break;
                }
    
                if (animalRunsAway)
                {
                    RunAway();
                    _eatingCoroutine = null;
                    yield break;
                }

                yield return new WaitForSeconds(2f);
            }
        }
    }
}
