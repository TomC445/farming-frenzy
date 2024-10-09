using Mono.Cecil.Cil;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAgent : MonoBehaviour
{
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


    private Transform GetRandomChild(Transform parent)
    {
        if (parent.childCount == 0)
        {
            return null;
        }
        int randomIndex = Random.Range(0, parent.childCount);
        return parent.GetChild(randomIndex);
    }

    private void Update()
    {
        var direction = _agent.velocity.normalized;
        _agentAnimator.SetFloat("X", direction.x);
        _agentAnimator.SetFloat("Y", direction.y);
        _agentAnimator.SetBool("Movement", direction.magnitude > 0);
        _spriteRenderer.sortingOrder = 10000 - Mathf.CeilToInt(transform.position.y);
        if (_currentState == State.Hungry)
        {
            AcquirePlantTarget();
            if (_target == null)
            {
                _currentState = State.Scared;
            } else
            {
                _agent.SetDestination(_target.position);
                _currentState = State.Eating;
            }
        }
        else if (_currentState == State.Eating)
        {
            if(_target == null)
            {
                _currentState = State.Hungry;
            }
        }
        else if (_currentState == State.Scared)
        {
            if (_target != null)
            {
                if (_agent.remainingDistance <= 0.1f)
                {
                    _agent.isStopped = true;
                    Destroy(gameObject);
                }
                return;
            }

            int randomIndex = Random.Range(0, _spawnPoints.childCount);
            var _spawnPoint = _spawnPoints.GetChild(randomIndex);
            _target = _spawnPoint;
            _agent.SetDestination(_spawnPoint.position);
        }
    }

    private void AcquirePlantTarget()
    {
        _target = GetRandomChild(_plantTransform);
    }

    private void OnMouseDown()
    {
        _health -= 1;
        if(_health <= 0)
        {
            int randomIndex = Random.Range(0, _spawnPoints.childCount);
            var _spawnPoint = _spawnPoints.GetChild(randomIndex);
            _target = _spawnPoint;
            _agent.SetDestination(_spawnPoint.position);
            _currentState = State.Scared;
        }
    }
    #endregion
}
