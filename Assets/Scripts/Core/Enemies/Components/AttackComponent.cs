using UnityEngine;
using System;
using System.Collections;

[DisallowMultipleComponent]
public class AttackComponent : MonoBehaviour, IAttacker
{
    [Header("Attack")]
    public float cooldown = 0.25f;    // hitscan can fire faster
    public float range = 50f;
    public int shotsBeforeReload = 5;
    public float reloadDuration = 2.5f;
    public float damage = 10f;

    [Header("Hitscan")]
    public Transform firePoint;
    public TrailRenderer bulletTrailPrefab;
    public float trailSpeed = 200f;

    private float lastAttackTime = -999f;
    private int shotsFired = 0;
    private bool isReloading = false;

    public event Action OnReloadStart;
    public event Action OnReloadEnd;

    private Animator animator;

    public bool CanAttack => !isReloading && Time.time >= lastAttackTime + cooldown;
    public bool IsReloading => isReloading;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void TryAttack(Transform target)
    {
        if (!CanAttack || target == null)
            return;

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist > range)
            return;

        // LOS check
        if (Physics.Raycast(transform.position + Vector3.up * 1f,
            (target.position - transform.position).normalized,
            out RaycastHit hitCheck, range))
        {
            if (!hitCheck.collider.CompareTag("Player"))
                return;
        }

        // Aim
        if (firePoint != null)
            firePoint.LookAt(target.position + Vector3.up * 0.5f);

        // HITSCAN FIRE
        HitscanFire(target);

        // fire animation
        if (animator != null)
        {
            animator.SetBool("IsShooting", true);
            StartCoroutine(ResetShootFlag());
        }

        lastAttackTime = Time.time;
        shotsFired++;

        if (shotsFired >= shotsBeforeReload)
            StartCoroutine(ReloadRoutine());
    }

    // ----------------------------
    // HITSCAN FIRE
    // ----------------------------
    private void HitscanFire(Transform target)
    {
        Vector3 direction = firePoint.forward;

        if (Physics.Raycast(firePoint.position, direction, out RaycastHit hit, range))
        {
            // Apply damage if target has health
            HealthComponent hp = hit.collider.GetComponent<HealthComponent>();
            if (hp != null)
                hp.TakeDamage(damage);

            // Bullet trail to hit point
            if (bulletTrailPrefab != null)
            {
                TrailRenderer trail = Instantiate(bulletTrailPrefab, firePoint.position, Quaternion.identity);
                StartCoroutine(PlayTrail(trail, hit.point));
            }
        }
        else
        {
            // Missed — send trail to max range
            Vector3 endPoint = firePoint.position + direction * range;

            if (bulletTrailPrefab != null)
            {
                TrailRenderer trail = Instantiate(bulletTrailPrefab, firePoint.position, Quaternion.identity);
                StartCoroutine(PlayTrail(trail, endPoint));
            }
        }
    }

    // ----------------------------
    // BULLET TRAIL MOVEMENT
    // ----------------------------
    private IEnumerator PlayTrail(TrailRenderer trail, Vector3 targetPoint)
    {
        Vector3 start = trail.transform.position;
        float dist = Vector3.Distance(start, targetPoint);
        float remaining = dist;

        while (remaining > 0f)
        {
            float step = trailSpeed * Time.deltaTime;
            trail.transform.position = Vector3.MoveTowards(trail.transform.position, targetPoint, step);
            remaining -= step;
            yield return null;
        }

        trail.transform.position = targetPoint;
        Destroy(trail.gameObject, trail.time);
    }

    // ----------------------------
    // RESET SHOOT ANIM
    // ----------------------------
    private IEnumerator ResetShootFlag()
    {
        yield return null;
        if (animator != null)
            animator.SetBool("IsShooting", false);
    }

    // ----------------------------
    // RELOAD
    // ----------------------------
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
