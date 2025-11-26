using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MovementComponent : MonoBehaviour, IMovable
{
    private NavMeshAgent agent;

    public bool IsMoving => agent != null && agent.hasPath && agent.remainingDistance > agent.stoppingDistance;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private bool CanReach(Vector3 destination)
    {
        if (agent == null || !agent.isOnNavMesh)
            return false;

        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(destination, path);

        return path.status == NavMeshPathStatus.PathComplete && path.corners.Length > 1;
    }

    public void MoveTo(Vector3 worldPosition)
    {
        if (agent == null || !agent.isActiveAndEnabled || (GetComponent<AttackComponent>()?.IsReloading ?? false))
            return;

        // Prevent agents from running into walls or invalid zones
        if (!CanReach(worldPosition))
        {
            // Stop movement and avoid getting stuck
            agent.isStopped = true;
            agent.ResetPath();
            return;
        }

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
