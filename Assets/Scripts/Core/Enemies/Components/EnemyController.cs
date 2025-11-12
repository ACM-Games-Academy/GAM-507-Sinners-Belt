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
    public float losRecheckInterval = 0.5f;

    [Header("Combat Movement")]
    public float strafeDistance = 2.5f;
    public float strafeSpeed = 2f;
    public float strafeSwitchTime = 2f;
    public float avoidRadius = 2f;
    public LayerMask enemyMask;

    private Transform player;
    private Vector3 lastKnownPlayerPos;
    private bool playerVisible;
    private Coroutine strafeRoutine;
    private float nextStrafeSwitch;
    private float strafeDir = 1f;

    // --- Public getters ---
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

        if (vision != null)
        {
            vision.PlayerDetected += OnPlayerDetected;
            vision.PlayerLost += OnPlayerLost;
        }

        if (health != null)
        {
            health.OnDeath += HandleDeath;
        }

        // Let script control rotation, not the NavMeshAgent
        if (agent != null)
            agent.updateRotation = false;
    }

    private void Update()
    {
        if (player == null || !health.IsAlive()) return;

        if (!playerVisible)
        {
            MoveTo(player.position);
        }
        else
        {
            float dist = Vector3.Distance(transform.position, player.position);

            // Maintain distance
            if (dist > followStopDistance + 1f)
                MoveTo(player.position);
            else if (dist < followStopDistance - 1f)
                MoveTo(transform.position - (player.position - transform.position).normalized * 1.5f);
            else
                movement.StopMovement();

            // Avoid overlapping with allies
            AvoidOtherEnemies();

            // Fire if player visible and not blocked
            if (attack != null && HasLineOfSightToPlayer())
                attack.TryAttack(player);
        }

        // Always rotate smoothly to face player
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

    // --- IAggro interface ---
    public void OnPlayerDetected(Transform playerTransform)
    {
        player = playerTransform;
        playerVisible = true;
        lastKnownPlayerPos = player.position;

        if (strafeRoutine == null)
            strafeRoutine = StartCoroutine(StrafeRoutine());
    }

    public void OnPlayerLost()
    {
        playerVisible = false;

        if (strafeRoutine != null)
            StopCoroutine(strafeRoutine);
        strafeRoutine = null;
    }

    // --- IImpactable interface ---
    public void OnImpact(ImpactInfo data)
    {
        health?.TakeDamage(data.Damage);
    }

    // --- Strafing ---
    private IEnumerator StrafeRoutine()
    {
        nextStrafeSwitch = Time.time + strafeSwitchTime;

        while (playerVisible && health.IsAlive())
        {
            if (player == null)
                yield break;

            // Switch strafe direction occasionally
            if (Time.time >= nextStrafeSwitch)
            {
                strafeDir *= -1f;
                nextStrafeSwitch = Time.time + strafeSwitchTime;
            }

            // Calculate side position relative to player
            Vector3 toPlayer = (player.position - transform.position).normalized;
            Vector3 side = Vector3.Cross(Vector3.up, toPlayer) * strafeDir;
            Vector3 targetPos = transform.position + side * strafeDistance;

            if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
            {
                MoveTo(hit.position);
            }

            // Face player even while strafing
            FacePlayer();

            yield return new WaitForSeconds(strafeSpeed);
        }
    }

    // --- Avoidance logic ---
    private void AvoidOtherEnemies()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, avoidRadius, enemyMask);
        foreach (var other in nearby)
        {
            if (other.gameObject == gameObject) continue;

            Vector3 dirAway = (transform.position - other.transform.position).normalized;
            Vector3 offset = dirAway * 0.5f;

            // Move sideways if looking directly at another enemy
            float dot = Vector3.Dot(transform.forward, (other.transform.position - transform.position).normalized);
            if (dot > 0.5f)
            {
                Vector3 sideStep = Vector3.Cross(Vector3.up, transform.forward).normalized *
                                   (UnityEngine.Random.value > 0.5f ? 1 : -1);
                MoveTo(transform.position + sideStep);
            }

            // Keep separation
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
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, avoidRadius);
    }
#endif
}
