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

    [Header("Animation")]
    public Animator animator;

    [Header("Behavior")]
    public float followStopDistance = 6f;
    public float losRecheckInterval = 0.5f;

    [Header("Combat Movement")]
    public float strafeDistance = 2.5f;
    public float strafeSpeed = 2f;
    public float strafeSwitchTime = 2f;
    public float avoidRadius = 2f;
    public LayerMask enemyMask;

    [Header("Roaming")]
    public float roamRadius = 10f;
    public float roamInterval = 4f;

    [Header("Roaming Restrictions")]
    public string roamBlockedAreaName = "DoorBlock";
    private int roamBlockedAreaMask;

    private float nextRoamTime = 0f;
    private bool isRoaming = true;

    private Transform player;
    private Vector3 lastKnownPlayerPos;
    private bool playerVisible;
    private Coroutine strafeRoutine;
    private float nextStrafeSwitch;
    private float strafeDir = 1f;

    public Transform GetPlayer() => player;
    public bool CanSeePlayer() => playerVisible;
    public Vector3 GetLastKnownPlayerPos() => lastKnownPlayerPos;

    private void Awake()
    {
        health ??= GetComponent<HealthComponent>();
        movement ??= GetComponent<MovementComponent>();
        attack ??= GetComponent<AttackComponent>();
        vision ??= GetComponent<VisionComponent>();
        agent ??= GetComponent<NavMeshAgent>();

        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (vision != null)
        {
            vision.PlayerDetected += OnPlayerDetected;
            vision.PlayerLost += OnPlayerLost;
        }

        if (health != null)
            health.OnDeath += HandleDeath;

        // get the bitmask for the named area (1 << areaIndex)
        int areaIndex = NavMesh.GetAreaFromName(roamBlockedAreaName);
        roamBlockedAreaMask = 1 << areaIndex;
    }

    private void Update()
    {
        if (!health.IsAlive()) return;

        bool isMoving = agent != null && agent.velocity.sqrMagnitude > 0.2f;
        animator?.SetBool("IsMoving", isMoving);

        if (player == null && !playerVisible)
        {
            if (!isRoaming)
                isRoaming = true;

            HandleRoaming();
            return;
        }

        // Combat: determine target to face
        Vector3 faceTarget = playerVisible && player != null
            ? player.position
            : lastKnownPlayerPos;

        if (!isRoaming)
            FaceTarget(faceTarget);

        HandleCombatMovement();

        // Update last known position while player is visible
        if (playerVisible && player != null)
            lastKnownPlayerPos = player.position;
    }

    private void HandleRoaming()
    {
        if (Time.time < nextRoamTime)
            return;

        nextRoamTime = Time.time + roamInterval;

        Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * roamRadius;
        randomOffset.y = 0;

        Vector3 roamPos = transform.position + randomOffset;

        // Sample with all areas, then check the hit.mask against the blocked mask
        if (NavMesh.SamplePosition(roamPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            // hit.mask is already a bitmask for the area; compare directly
            if (hit.mask == roamBlockedAreaMask)
                return;

            MoveTo(hit.position);
        }
    }

    private void HandleCombatMovement()
    {
        isRoaming = false;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > followStopDistance + 0.5f)
        {
            MoveTo(player.position);
        }
        else
        {
            if (strafeRoutine == null)
                strafeRoutine = StartCoroutine(StrafeRoutine());

            if (dist < followStopDistance - 1f)
                MoveTo(transform.position - (player.position - transform.position).normalized * 1.5f);
        }

        AvoidOtherEnemies();

        if (attack != null && HasLineOfSightToPlayer())
        {
            animator?.SetBool("IsShooting", true);
            attack.TryAttack(player);
            StartCoroutine(ResetShootFlag());
        }

        FacePlayer();
    }

    private IEnumerator ResetShootFlag()
    {
        yield return null; // wait 1 frame
        animator?.SetBool("IsShooting", false);
    }

    private void FacePlayer()
    {
        if (player == null) return;

        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);

            // HARD LOCK (instant rotate)
            transform.rotation = targetRot;
        }
    }

    private void FaceTarget(Vector3 targetPos)
    {
        Vector3 lookDir = targetPos - transform.position;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 5f);
            transform.rotation = targetRot;
        }
    }

    public void OnPlayerDetected(Transform playerTransform)
    {
        isRoaming = false;
        player = playerTransform;
        playerVisible = true;
        lastKnownPlayerPos = player.position;

        if (agent != null)
            agent.areaMask = NavMesh.AllAreas;

        if (strafeRoutine == null)
            strafeRoutine = StartCoroutine(StrafeRoutine());
    }

    public void OnPlayerLost()
    {
        playerVisible = false;
        isRoaming = true;

        if (strafeRoutine != null)
            StopCoroutine(strafeRoutine);

        strafeRoutine = null;

        if (agent != null)
            agent.areaMask = NavMesh.AllAreas & ~roamBlockedAreaMask;
    }

    public void OnImpact(ImpactInfo data)
    {
        animator?.SetTrigger("Hit");
        health?.TakeDamage(data.Damage);
    }

    private IEnumerator StrafeRoutine()
    {
        nextStrafeSwitch = Time.time + strafeSwitchTime;

        while (playerVisible && health.IsAlive())
        {
            if (player == null) yield break;

            animator?.SetBool("Strafe", true);

            float dist = Vector3.Distance(transform.position, player.position);

            if (dist <= followStopDistance + 1.5f)
            {
                if (Time.time >= nextStrafeSwitch)
                {
                    strafeDir *= -1f;
                    nextStrafeSwitch = Time.time + strafeSwitchTime;
                }

                Vector3 toPlayer = (player.position - transform.position).normalized;
                Vector3 side = Vector3.Cross(Vector3.up, toPlayer) * strafeDir;
                Vector3 targetPos = transform.position + side * strafeDistance;

                if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
                    MoveTo(hit.position);
            }
            else
            {
                MoveTo(player.position);
            }

            FacePlayer();
            yield return new WaitForSeconds(strafeSpeed);
        }

        animator?.SetBool("Strafe", false);
        strafeRoutine = null;
    }

    private void AvoidOtherEnemies()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, avoidRadius, enemyMask);
        foreach (var other in nearby)
        {
            if (other.gameObject == gameObject) continue;

            Vector3 dirAway = (transform.position - other.transform.position).normalized;
            Vector3 offset = dirAway * 0.5f;

            float dot = Vector3.Dot(transform.forward, (other.transform.position - transform.position).normalized);

            if (dot > 0.5f)
            {
                Vector3 sideStep = Vector3.Cross(Vector3.up, transform.forward).normalized *
                                   (UnityEngine.Random.value > 0.5f ? 1 : -1);
                MoveTo(transform.position + sideStep);
            }

            // move the agent using agent.Move instead of directly changing transform.position
            if (agent != null && agent.isOnNavMesh)
            {
                agent.Move(offset * Time.deltaTime);
            }
        }
    }

    private bool HasLineOfSightToPlayer()
    {
        if (player == null) return false;

        // offset the ray origin slightly forward so it doesn't start inside nearby geometry
        Vector3 origin = transform.position + Vector3.up * 1.5f + transform.forward * 0.2f;
        Vector3 dir = (player.position + Vector3.up * 1f) - origin;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, vision.viewRadius))
        {
            if (hit.collider.CompareTag("Enemy")) return false;
            return hit.collider.CompareTag("Player");
        }

        return false;
    }

    public void MoveTo(Vector3 destination)
    {
        movement?.MoveTo(destination);
    }

    private void HandleDeath()
    {
        animator?.SetTrigger("Die");

        movement?.StopMovement();
        if (agent != null) agent.isStopped = true;

        Destroy(gameObject, 3f); // More time for animations
        enabled = false;
    }

    private void OnDestroy()
    {
        if (vision != null)
        {
            vision.PlayerDetected -= OnPlayerDetected;
            vision.PlayerLost -= OnPlayerLost;
        }
        if (health != null)
            health.OnDeath -= HandleDeath;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, avoidRadius);
    }
#endif
}
