using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(EnemyController))]
public class GruntController : MonoBehaviour
{
    private enum GruntState { Idle, MovingToCover, InCover, Peeking, Chasing, Searching, Roaming }

    [Header("Grunt Settings")]
    public float minAttackDistance = 5f;
    public float maxAttackDistance = 15f;
    public float searchDuration = 5f;
    public float searchRadius = 4f;
    public float roamRadius = 10f;
    public float coverSearchRadius = 8f;
    public LayerMask coverMask;
    public LayerMask obstacleMask;

    [Header("Cover Behavior")]
    public float peekInterval = 3f;       // seconds between peeks
    public float peekDuration = 1.5f;     // how long grunt stays out to shoot
    public float peekOffset = 1.2f;       // how far to move sideways to peek

    private MovementComponent movement;
    private AttackComponent attack;
    private HealthComponent health;
    private NavMeshAgent agent;
    private Transform player;

    private Vector3 coverPosition;
    private Vector3 peekPosition;
    private GruntState currentState;
    private Coroutine coverRoutine;

    private void Awake()
    {
        movement = GetComponent<MovementComponent>();
        attack = GetComponent<AttackComponent>();
        health = GetComponent<HealthComponent>();
        agent = GetComponent<NavMeshAgent>();
        currentState = GruntState.Idle;
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            MoveToNearestCover();
        }
    }

    private void Update()
    {
        if (!health.IsAlive) return;

        switch (currentState)
        {
            case GruntState.MovingToCover:
                if (agent.remainingDistance <= agent.stoppingDistance)
                    EnterCoverState();
                break;

            case GruntState.InCover:
                // Wait for peek timing handled by coroutine
                break;

            case GruntState.Peeking:
                HandlePeeking();
                break;

            case GruntState.Chasing:
                HandleChasing();
                break;

            case GruntState.Searching:
                HandleSearching();
                break;

            case GruntState.Roaming:
                HandleRoaming();
                break;
        }
    }

    private void HandleChasing()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > maxAttackDistance)
        {
            movement.MoveTo(player.position);
        }
        else if (dist < minAttackDistance)
        {
            Vector3 dir = (transform.position - player.position).normalized;
            movement.MoveTo(transform.position + dir * 2f);
        }
        else
        {
            movement.StopMovement();
            transform.LookAt(player);
            if (HasLineOfSightToPlayer())
                attack.TryAttack(player);
        }
    }

    private void HandlePeeking()
    {
        if (player == null) return;
        transform.LookAt(player);

        if (HasLineOfSightToPlayer())
            attack.TryAttack(player);
    }

    private bool HasLineOfSightToPlayer()
    {
        if (player == null) return false;

        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 dir = (player.position + Vector3.up * 1f) - origin;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, maxAttackDistance, obstacleMask | coverMask))
        {
            if (!hit.collider.CompareTag("Player"))
                return false;
        }

        return true;
    }

    private void EnterCoverState()
    {
        currentState = GruntState.InCover;
        coverPosition = transform.position;

        if (coverRoutine != null)
            StopCoroutine(coverRoutine);

        coverRoutine = StartCoroutine(CoverRoutine());
    }

    private IEnumerator CoverRoutine()
    {
        // Stay behind cover, peek out periodically
        while (health.IsAlive)
        {
            // wait before peeking
            yield return new WaitForSeconds(peekInterval);

            if (player == null) continue;

            // calculate a smarter peek point
            Vector3 toPlayer = (player.position - coverPosition).normalized;
            Vector3 sideDir = Vector3.Cross(Vector3.up, toPlayer);

            // random left or right peek
            float sideSign = Random.value > 0.5f ? 1f : -1f;

            // offset sideways + slightly forward (toward player)
            Vector3 rawPeek = coverPosition + sideDir * sideSign * peekOffset + toPlayer * 0.8f;

            // sample to valid NavMesh spot
            if (NavMesh.SamplePosition(rawPeek, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
            {
                peekPosition = hit.position;

                currentState = GruntState.Peeking;
                movement.MoveTo(peekPosition);

                // wait until grunt arrives
                while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
                    yield return null;

                // shoot while peeking
                float endTime = Time.time + peekDuration;
                while (Time.time < endTime && currentState == GruntState.Peeking)
                {
                    HandlePeeking();
                    yield return null;
                }

                // return to cover
                movement.MoveTo(coverPosition);
                currentState = GruntState.InCover;

                // wait until back at cover
                while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
                    yield return null;
            }
            else
            {
                // fallback: skip peek if position invalid
                currentState = GruntState.InCover;
            }
        }
    }

    private void MoveToNearestCover()
    {
        Collider[] covers = Physics.OverlapSphere(transform.position, coverSearchRadius, coverMask);
        if (covers.Length == 0)
        {
            currentState = GruntState.Chasing;
            return;
        }

        Collider closest = null;
        float closestDist = Mathf.Infinity;

        foreach (var cover in covers)
        {
            float dist = Vector3.Distance(transform.position, cover.transform.position);
            if (dist < closestDist)
            {
                closest = cover;
                closestDist = dist;
            }
        }

        if (closest != null && NavMesh.SamplePosition(closest.transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            movement.MoveTo(hit.position);
            currentState = GruntState.MovingToCover;
        }
        else
        {
            currentState = GruntState.Chasing;
        }
    }

    private void HandleSearching()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 randomOffset = Random.insideUnitSphere * searchRadius;
            randomOffset.y = 0;
            Vector3 newPos = coverPosition + randomOffset;

            if (NavMesh.SamplePosition(newPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                movement.MoveTo(hit.position);
        }
    }

    private void HandleRoaming()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 randomOffset = Random.insideUnitSphere * roamRadius;
            randomOffset.y = 0;
            Vector3 newPos = transform.position + randomOffset;

            if (NavMesh.SamplePosition(newPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                movement.MoveTo(hit.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (currentState == GruntState.InCover || currentState == GruntState.Peeking)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(coverPosition, 0.4f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(peekPosition, 0.3f);
        }

        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + Vector3.up * 1.5f, player.position + Vector3.up * 1f);
        }
    }
}
