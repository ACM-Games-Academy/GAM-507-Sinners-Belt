using UnityEngine;

public class SemiAutoFireMode : IFireMode
{
    public float FireRate => data.fireRate;
    public float Damage => data.damage;
    public float Range => data.range;

    private Weapon weapon;
    private FireModeData data;
    private float lastFireTime;

    public void Initialize(Weapon weapon, FireModeData data)
    {
        this.weapon = weapon;
        this.data = data;
    }

    public void Fire()
    {
        if (Time.time - lastFireTime < FireRate)
            return;

        lastFireTime = Time.time;

        Vector3 targetPoint = weapon.GetCameraTargetPoint();
        Vector3 muzzlePos = weapon.muzzlePoint.position;
        Vector3 direction = (targetPoint - muzzlePos).normalized;

        if (Physics.Raycast(muzzlePos, direction, out RaycastHit hit, Range, weapon.hitMask))
        {
            // TODO: Damage logic
            weapon.SpawnTracer(muzzlePos, hit.point);
        }
        else
        {
            Vector3 missPoint = muzzlePos + direction * Range;
            weapon.SpawnTracer(muzzlePos, missPoint);
        }

        if (data.fireSound)
            AudioSource.PlayClipAtPoint(data.fireSound, muzzlePos);

        if (data.muzzleFlashPrefab)
            Object.Instantiate(data.muzzleFlashPrefab, weapon.muzzlePoint.position, weapon.muzzlePoint.rotation);
    }
}