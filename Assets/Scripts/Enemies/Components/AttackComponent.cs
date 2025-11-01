using UnityEngine;

[DisallowMultipleComponent]
public class AttackComponent : MonoBehaviour, IAttacker
{
    [Header("Attack")]
    public float cooldown = 1.2f;
    public float range = 15f;
    public float bulletSpeed = 40f;

    [Header("Ranged Attack")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Melee Fallback")]
    public float meleeDamage = 10f;

    private float lastAttackTime = -999f;

    public bool CanAttack => Time.time >= lastAttackTime + cooldown;

    public void TryAttack(Transform target)
    {
        if (!CanAttack || target == null) return;

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist > range) return;

        // Aim at target
        if (firePoint != null)
            firePoint.LookAt(target.position + Vector3.up * 1.0f);

        if (projectilePrefab != null && firePoint != null)
        {
            ShootProjectile(firePoint, target);
        }
        else
        {
            // fallback: instant melee damage
            var health = target.GetComponent<HealthComponent>();
            if (health != null)
                health.TakeDamage(meleeDamage);
        }

        lastAttackTime = Time.time;
    }

    private void ShootProjectile(Transform origin, Transform target)
    {
        GameObject bulletObj = Instantiate(projectilePrefab, origin.position, origin.rotation);

        // Try to apply force/velocity if bullet has a Rigidbody
        if (bulletObj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.linearVelocity = origin.forward * bulletSpeed;
        }
        else
        {
            // fallback: move via script (handled in bullet)
            if (bulletObj.TryGetComponent<Bullet>(out var bullet))
            {
                bullet.Initialize(origin.forward * bulletSpeed);
            }
        }
    }
}
