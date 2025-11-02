using UnityEngine;

public class SemiAutoFireMode : IFireMode
{
    public float FireRate => 1f / data.fireRate;
    public float Damage => data.damage;
    public float Range => data.range;
    public float AmmoConsumption => data.ammoConsumption;

    private WeaponBase weapon;
    private readonly SemiAutoFireModeData data;
    private float lastFireTime;

    public SemiAutoFireMode(SemiAutoFireModeData fireModeData)
    {
        data = fireModeData;
    }

    public void Initialize(WeaponBase weapon)
    {
        this.weapon = weapon;
    }

    public void Fire()
    {
        if (Time.time - lastFireTime < FireRate || !weapon.TryUseAmmo(AmmoConsumption))
            return;

        lastFireTime = Time.time;

        Vector3 targetPoint = weapon.GetCameraTargetPoint();
        Vector3 muzzlePos = weapon.muzzlePoint.position;
        Vector3 direction = (targetPoint - muzzlePos).normalized;

        if (Physics.Raycast(muzzlePos, direction, out RaycastHit hit, Range, weapon.hitMask))
        {
            weapon.SpawnTracer(muzzlePos, hit.point);

            if (hit.collider.TryGetComponent(out IImpactable component))
            {
                component.OnImpact(new ImpactInfo
                {
                    Damage = Damage,
                });
            }
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