using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Simple NavMesh-based movement. Single responsibility: move the agent to a destination.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class MovementComponent : MonoBehaviour, IMovable
{
    private NavMeshAgent agent;

    public bool IsMoving => agent != null && agent.hasPath && agent.remainingDistance > agent.stoppingDistance;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void MoveTo(Vector3 worldPosition)
    {
        if (agent == null || !agent.isActiveAndEnabled) return;
        agent.isStopped = false;
        agent.SetDestination(worldPosition);
    }

    public void StopMovement()
    {
        if (agent == null || !agent.isActiveAndEnabled) return;
        agent.isStopped = true;
        agent.ResetPath();
    }
}
