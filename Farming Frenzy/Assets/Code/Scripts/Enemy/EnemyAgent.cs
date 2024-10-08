using Mono.Cecil.Cil;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAgent : MonoBehaviour
{
    #region Editor Fields
    
    #endregion

    #region Properties
    private Transform _plantTransform;
    private NavMeshAgent _agent;
    private Animator _agentAnimator;
    public Transform _target;
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _collider;
    public Transform _spawnPoints;
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
        AcquireTarget();
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
        Debug.Log(_target == null);
        var direction = _agent.velocity.normalized;
        _agentAnimator.SetFloat("X", direction.x);
        _agentAnimator.SetFloat("Y", direction.y);
        _agentAnimator.SetBool("Movement", direction.magnitude > 0);
        _spriteRenderer.sortingOrder = 10000 - Mathf.CeilToInt(transform.position.y);
        if (_target == null || !_target.gameObject.activeInHierarchy)
        {
            AcquireTarget();
        }
    }

    private void AcquireTarget()
    {
        _target = GetRandomChild(_plantTransform);
        if (_target == null)
        {
            int randomIndex = Random.Range(0, _spawnPoints.childCount);
            var _spawnPoint = _spawnPoints.GetChild(randomIndex);
            _target = _spawnPoint;
            _agent.SetDestination(_spawnPoint.position);
            return;
        }
        _agent.SetDestination(_target.position);
    }
    #endregion
}
