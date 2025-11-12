using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(HealthComponent))]
public class EnemyController : MonoBehaviour, IAggro, IImpactable
{
    [Header("Components (auto-assigned if present)")]
    public MovementComponent movement;
    public AttackComponent attack;
    public VisionComponent vision;
    public HealthComponent health;
    public NavMeshAgent agent;

    [Header("Behavior")]
    public float followStopDistance = 6f;
    public float lostSightDuration = 5f;
    public float losRecheckInterval = 0.5f;

    [Header("Cover Settings")]
    public float coverSearchRadius = 8f;
    public float peekInterval = 3f;
    public float peekDuration = 1.5f;
    public float peekOffset = 1.2f;
    public float coverScanRadius = 5f;
    public LayerMask coverMask;
    public LayerMask obstacleMask;

    private Transform player;
    private Vector3 lastKnownPlayerPos;
    private float lostSightTimer;
    private bool playerVisible;

    private Vector3 coverPosition;
    private Vector3 peekPosition;
    private Coroutine coverRoutine;
    private bool usingTallCover;

    // --- Public getters ---
    public Transform GetPlayer() => player;
    public bool CanSeePlayer() => playerVisible;
    public Vector3 GetLastKnownPlayerPos() => lastKnownPlayerPos;

    private void Awake()
    {
        // auto-assign components
        health ??= GetComponent<HealthComponent>();
        movement ??= GetComponent<MovementComponent>();
        attack ??= GetComponent<AttackComponent>();
        vision ??= GetComponent<VisionComponent>();
        agent ??= GetComponent<NavMeshAgent>();

        // subscribe to events
        if (vision != null)
        {
            vision.PlayerDetected += OnPlayerDetected;
            vision.PlayerLost += OnPlayerLost;
        }

        if (health != null)
        {
            health.OnDeath += HandleDeath;
            health.OnHealthChanged += HandleHealthChanged;
        }
    }

    private void Start()
    {
        MoveToNearestCover("ShortCover");
    }

    private void Update()
    {
        if (player == null || !health.IsAlive()) return;

        // Attack if visible and has LOS
        if (playerVisible && attack != null && HasLineOfSightToPlayer())
        {
            attack.TryAttack(player);
        }

        FacePlayer();
    }

    private void FacePlayer()
    {
        if (player == null) return;

        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 6f);
        }
    }

    // --- Interface: IAggro ---
    public void OnPlayerDetected(Transform playerTransform)
    {
        player = playerTransform;
        playerVisible = true;
        lastKnownPlayerPos = player.position;

        if (coverRoutine != null)
            StopCoroutine(coverRoutine);
        coverRoutine = StartCoroutine(CoverRoutine());
    }

    public void OnPlayerLost()
    {
        playerVisible = false;
    }

    // --- Interface: IImpactable ---
    public void OnImpact(ImpactInfo data)
    {
        if (health != null)
            health.TakeDamage(data.Damage);
    }

    private void HandleHealthChanged(float current, float max)
    {
        if (current <= max * 0.5f && !usingTallCover)
        {
            usingTallCover = true;
            MoveToNearestCover("TallCover");
        }
    }

    // --- Main AI Cover Routine ---
    private IEnumerator CoverRoutine()
    {
        while (health.IsAlive())
        {
            yield return new WaitForSeconds(peekInterval);

            if (player == null || !playerVisible)
                continue;

            Vector3 toPlayer = (player.position - coverPosition).normalized;
            Vector3 sideDir = Vector3.Cross(Vector3.up, toPlayer);

            // pick side that gives LOS break
            Vector3 leftPeek = coverPosition - sideDir * peekOffset;
            Vector3 rightPeek = coverPosition + sideDir * peekOffset;

            bool leftVisible = HasLineOfSightFrom(leftPeek);
            bool rightVisible = HasLineOfSightFrom(rightPeek);

            Vector3 chosenPeek = (!leftVisible && rightVisible) ? leftPeek :
                                 (leftVisible && !rightVisible) ? rightPeek :
                                 (UnityEngine.Random.value > 0.5f ? rightPeek : leftPeek);

            if (NavMesh.SamplePosition(chosenPeek + toPlayer * 0.8f, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
            {
                peekPosition = hit.position;
                MoveTo(peekPosition);

                while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
                    yield return null;

                float endTime = Time.time + peekDuration;
                while (Time.time < endTime)
                {
                    if (playerVisible && HasLineOfSightToPlayer())
                        attack.TryAttack(player);
                    yield return null;
                }

                MoveTo(coverPosition);
                while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
                    yield return null;
            }
        }
    }

    private bool HasLineOfSightFrom(Vector3 position)
    {
        if (player == null) return false;

        Vector3 origin = position + Vector3.up * 1.5f;
        Vector3 dir = (player.position + Vector3.up * 1f) - origin;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, vision.viewRadius, obstacleMask | coverMask))
        {
            return hit.collider.CompareTag("Player");
        }

        return true;
    }

    private bool HasLineOfSightToPlayer()
    {
        if (player == null) return false;

        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 dir = (player.position + Vector3.up * 1f) - origin;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, vision.viewRadius, obstacleMask | coverMask))
        {
            if (!hit.collider.CompareTag("Player"))
                return false;
        }

        return true;
    }

    public void MoveToNearestCover(string tag)
    {
        Collider[] covers = Physics.OverlapSphere(transform.position, coverScanRadius, coverMask);
        Collider bestCover = null;
        float bestDist = Mathf.Infinity;

        foreach (var c in covers)
        {
            if (!c.CompareTag(tag)) continue;
            float dist = Vector3.Distance(transform.position, c.transform.position);

            // Check which side is out of player LOS
            if (player != null)
            {
                Vector3 toCover = c.transform.position - player.position;
                Vector3 losOrigin = player.position + Vector3.up * 1.5f;
                Vector3 losDir = toCover.normalized;

                if (Physics.Raycast(losOrigin, losDir, out RaycastHit hit, vision.viewRadius, obstacleMask | coverMask))
                {
                    if (!hit.collider.CompareTag(tag)) continue; // not actually hidden behind cover
                }
            }

            if (dist < bestDist)
            {
                bestCover = c;
                bestDist = dist;
            }
        }

        if (bestCover != null && NavMesh.SamplePosition(bestCover.transform.position, out NavMeshHit hitNav, 2f, NavMesh.AllAreas))
        {
            MoveTo(hitNav.position);
            coverPosition = hitNav.position;
        }
        else
        {
            // fallback: move toward last known player position
            MoveTo(lastKnownPlayerPos);
        }
    }

    public void MoveTo(Vector3 destination)
    {
        if (movement != null)
            movement.MoveTo(destination);
    }

    private void HandleDeath()
    {
        movement?.StopMovement();
        enabled = false;
        Destroy(gameObject, 2f);
    }

    private void OnDestroy()
    {
        if (vision != null)
        {
            vision.PlayerDetected -= OnPlayerDetected;
            vision.PlayerLost -= OnPlayerLost;
        }
        if (health != null)
        {
            health.OnDeath -= HandleDeath;
            health.OnHealthChanged -= HandleHealthChanged;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, coverScanRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(coverPosition, 0.3f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(peekPosition, 0.3f);
    }
}
