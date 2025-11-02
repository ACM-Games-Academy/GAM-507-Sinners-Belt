using UnityEngine;
using System;

/// <summary>
/// Minimal orchestrator that composes Movement, Attack and Vision and reacts to player detection.
/// Keeps logic intentionally simple: on detect -> follow and attack if in range; on lost -> stop.
/// </summary>
[RequireComponent(typeof(HealthComponent))]
public class EnemyController : MonoBehaviour, IAggro, IImpactable
{
    [Header("Components (auto-assigned if present)")]
    public MovementComponent movement;
    public AttackComponent attack;
    public VisionComponent vision;
    public HealthComponent health;

    [Header("Behavior")]
    public float followStopDistance = 2f; // stop short of exact player position

    private Transform player;

    public void OnImpact(ImpactInfo data)
    {
        if (TryGetComponent(out IHealth component))
        {
            component.TakeDamage(data.Damage);
        }
    }

    private void Awake()
    {
        health = health ?? GetComponent<HealthComponent>();
        movement = movement ?? GetComponent<MovementComponent>();
        attack = attack ?? GetComponent<AttackComponent>();
        vision = vision ?? GetComponent<VisionComponent>();

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
        if (health == null || !health.IsAlive()) return;
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // Attack if in attack range and attacker available
        if (attack != null && dist <= (attack.range))
        {
            movement?.StopMovement();
            attack.TryAttack(player);
        }
        else
        {
            // approach player but stop a little before a collision
            if (movement != null)
            {
                Vector3 targetPos = player.position;
                Vector3 dir = (transform.position - player.position).normalized;
                targetPos += dir * followStopDistance;
                movement.MoveTo(targetPos);
            }
        }
    }

    public void OnPlayerDetected(Transform playerTransform)
    {
        player = playerTransform;
    }

    public void OnPlayerLost()
    {
        player = null;
        movement?.StopMovement();
    }

    // explicit IAggro mapping
    void IAggro.OnPlayerDetected(Transform playerTransform) => OnPlayerDetected(playerTransform);
    void IAggro.OnPlayerLost() => OnPlayerLost();

    private void HandleDeath()
    {
        movement?.StopMovement();
        // Simple death behavior: disable this controller and optionally destroy later
        enabled = false;
        // play effects, disable visuals, etc.
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
