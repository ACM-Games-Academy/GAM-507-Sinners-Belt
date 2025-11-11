using UnityEngine;
using System;

[RequireComponent(typeof(HealthComponent))]
public class EnemyController : MonoBehaviour, IAggro, IImpactable
{
    [Header("Components (auto-assigned if present)")]
    public MovementComponent movement;
    public AttackComponent attack;
    public VisionComponent vision;
    public HealthComponent health;

    [Header("Behavior")]
    public float followStopDistance = 2f;

    private Transform player;
    public bool HasAggro => player != null && health != null && health.IsAlive();

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
        if (!HasAggro) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // Attack if in attack range
        if (attack != null && dist <= attack.range)
        {
            movement?.StopMovement();
            attack.TryAttack(player);
        }
        else if (movement != null)
        {
            // Approach player but stop before collision
            Vector3 targetPos = player.position;
            Vector3 dir = (transform.position - player.position).normalized;
            targetPos += dir * followStopDistance;
            movement.MoveTo(targetPos);
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
