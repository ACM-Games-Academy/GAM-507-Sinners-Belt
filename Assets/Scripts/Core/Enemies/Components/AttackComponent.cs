using UnityEngine;
using System;
using System.Collections;

[DisallowMultipleComponent]
public class AttackComponent : MonoBehaviour, IAttacker
{
    [Header("Attack")]
    public float cooldown = 1.2f;
    public float range = 15f;
    public float bulletSpeed = 40f;
    public int shotsBeforeReload = 3;
    public float reloadDuration = 2.5f;

    [Header("Ranged Attack")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Melee Fallback")]
    public float meleeDamage = 10f;

    private float lastAttackTime = -999f;
    private int shotsFired = 0;
    private bool isReloading = false;

    public event Action OnReloadStart;
    public event Action OnReloadEnd;

    public bool CanAttack => !isReloading && Time.time >= lastAttackTime + cooldown;
    public bool IsReloading => isReloading;

    public void TryAttack(Transform target)
    {
        if (!CanAttack || target == null) return;

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist > range) return;

        // LOS check
        if (Physics.Raycast(transform.position + Vector3.up * 1.0f,
                            (target.position - transform.position).normalized,
                            out RaycastHit hit, range))
        {
            if (!hit.collider.CompareTag("Player")) return;
        }

        // Aim
        if (firePoint != null)
            firePoint.LookAt(target.position + Vector3.up * 0.6f);

        // Fire
        if (projectilePrefab != null && firePoint != null)
            ShootProjectile(firePoint);
        else
            TryMelee(target);

        lastAttackTime = Time.time;
        shotsFired++;

        if (shotsFired >= shotsBeforeReload)
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    private void TryMelee(Transform target)
    {
        var health = target.GetComponent<HealthComponent>();
        if (health != null)
            health.TakeDamage(meleeDamage);
    }

    private void ShootProjectile(Transform origin)
    {
        GameObject bulletObj = Instantiate(projectilePrefab, origin.position, origin.rotation);
        if (bulletObj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.linearVelocity = origin.forward * bulletSpeed;
        }
        else if (bulletObj.TryGetComponent<Bullet>(out var bullet))
        {
            bullet.Initialize(origin.forward * bulletSpeed);
        }
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        OnReloadStart?.Invoke();

        yield return new WaitForSeconds(reloadDuration);

        shotsFired = 0;
        isReloading = false;
        OnReloadEnd?.Invoke();
    }
}
