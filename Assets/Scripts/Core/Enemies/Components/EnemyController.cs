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
    public Animator animator;   // NEW

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

        // NEW
        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (vision != null)
        {
            vision.PlayerDetected += OnPlayerDetected;
            vision.PlayerLost += OnPlayerLost;
        }

        if (health != null)
            health.OnDeath += HandleDeath;

        roamBlockedAreaMask = 1 << NavMesh.GetAreaFromName(roamBlockedAreaName);
    }

    private void Update()
    {
        if (!health.IsAlive()) return;

        bool isMoving = agent != null && agent.velocity.sqrMagnitude > 0.2f;
        animator?.SetBool("IsMoving", isMoving);   // NEW

        if (player == null || !playerVisible)
        {
            if (!isRoaming)
                isRoaming = true;

            HandleRoaming(); 
            return;
        }

        HandleCombatMovement();
    }

    private void HandleRoaming()
    {
        if (Time.time < nextRoamTime)
            return;

        nextRoamTime = Time.time + roamInterval;

        Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * roamRadius;
        randomOffset.y = 0;

        Vector3 roamPos = transform.position + randomOffset;

        if (NavMesh.SamplePosition(roamPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            if ((1 << hit.mask) == roamBlockedAreaMask)
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
            animator?.SetTrigger("Shoot");   // NEW — attack animation
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
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }


    public void OnPlayerDetected(Transform playerTransform)
    {
        isRoaming = false;
        player = playerTransform;
        playerVisible = true;
        lastKnownPlayerPos = player.position;

        if (strafeRoutine == null)
            strafeRoutine = StartCoroutine(StrafeRoutine());

        agent.areaMask = NavMesh.AllAreas;
    }

    public void OnPlayerLost()
    {
        playerVisible = false;
        isRoaming = true;

        if (strafeRoutine != null)
            StopCoroutine(strafeRoutine);

        strafeRoutine = null;

        agent.areaMask = ~roamBlockedAreaMask;
    }

    public void OnImpact(ImpactInfo data)
    {
        animator?.SetTrigger("Hit");   // NEW — hit reaction animation
        health?.TakeDamage(data.Damage);
    }

    private IEnumerator StrafeRoutine()
    {
        nextStrafeSwitch = Time.time + strafeSwitchTime;

        while (playerVisible && health.IsAlive())
        {
            if (player == null) yield break;

            animator?.SetBool("Strafe", true);   // NEW

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

        animator?.SetBool("Strafe", false);  // NEW
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

            transform.position += offset * Time.deltaTime;
        }
    }

    private bool HasLineOfSightToPlayer()
    {
        if (player == null) return false;

        Vector3 origin = transform.position + Vector3.up * 1.5f;
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
        agent.isStopped = true;

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
