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

    [Header("Combat Movement")]
    public float strafeDistance = 2.5f;
    public float strafeSpeed = 2f;
    public float strafeSwitchTime = 2f;

    private Transform player;
    private Vector3 lastKnownPlayerPos;
    private bool playerVisible;
    private Coroutine strafeRoutine;

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
        }
    }

    private void Update()
    {
        if (player == null || !health.IsAlive()) return;

        if (!playerVisible)
        {
            // Move toward player until visible
            MoveTo(player.position);
        }
        else
        {
            // Maintain distance and attack
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist > followStopDistance)
                MoveTo(player.position);
            else
                movement.StopMovement();

            if (attack != null && HasLineOfSightToPlayer())
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

        // Start strafing
        if (strafeRoutine == null)
            strafeRoutine = StartCoroutine(StrafeRoutine());
    }

    public void OnPlayerLost()
    {
        playerVisible = false;

        // Stop strafing
        if (strafeRoutine != null)
            StopCoroutine(strafeRoutine);
        strafeRoutine = null;
    }

    // --- Interface: IImpactable ---
    public void OnImpact(ImpactInfo data)
    {
        health?.TakeDamage(data.Damage);
    }

    // --- Strafing Combat Routine ---
    private IEnumerator StrafeRoutine()
    {
        float strafeDir = 1f;
        float nextSwitch = Time.time + strafeSwitchTime;

        while (playerVisible && health.IsAlive())
        {
            if (player == null)
                yield break;

            float dist = Vector3.Distance(transform.position, player.position);

            // Maintain safe distance
            if (dist > followStopDistance + 1f)
                MoveTo(player.position);
            else if (dist < followStopDistance - 1f)
                MoveTo(transform.position - (player.position - transform.position).normalized * 1.5f);

            // Strafe side to side
            Vector3 side = Vector3.Cross(Vector3.up, (player.position - transform.position).normalized);
            Vector3 targetPos = transform.position + side * strafeDir * strafeDistance;

            if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
            {
                MoveTo(hit.position);
            }

            if (Time.time >= nextSwitch)
            {
                strafeDir *= -1f;
                nextSwitch = Time.time + strafeSwitchTime;
            }

            yield return new WaitForSeconds(strafeSpeed);
        }
    }

    private bool HasLineOfSightToPlayer()
    {
        if (player == null) return false;

        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 dir = (player.position + Vector3.up * 1f) - origin;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, vision.viewRadius))
        {
            if (!hit.collider.CompareTag("Player"))
                return false;
        }

        return true;
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
        }
    }
}
