using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAgent : MonoBehaviour
{
    #region Editor Fields
    [SerializeField] private Transform _target;
    #endregion

    #region Properties
    private NavMeshAgent _agent;
    private Animator _agentAnimator;
    #endregion

    #region Methods
    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agentAnimator = GetComponent<Animator>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
    }

    private void Update()
    {
        _agent.SetDestination(_target.position);
        var direction = _agent.velocity.normalized;
        _agentAnimator.SetFloat("X", direction.x);
        _agentAnimator.SetFloat("Y", direction.y);
        _agentAnimator.SetBool("Movement", direction.magnitude > 0);
    }
    #endregion
}
