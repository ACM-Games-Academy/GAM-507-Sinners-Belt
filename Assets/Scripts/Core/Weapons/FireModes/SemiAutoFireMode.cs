
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

        // Raycast from camera to crosshair
        Vector3 rayOrigin = weapon.cam.transform.position;
        Vector3 rayDirection = weapon.cam.transform.forward;

        Vector3 targetPoint;
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, Range, weapon.hitMask))
        {
            targetPoint = hit.point;

            if (hit.collider.TryGetComponent(out IImpactable component))
            {
                component.OnImpact(new ImpactInfo
                {
                    Damage = Damage,
                });
            }

            Debug.Log($"[Gun] Hit {hit.collider.name} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
        }
        else
        {
            targetPoint = rayOrigin + rayDirection * Range;
        }

        // Spawn tracer from muzzle to where camera hit
        weapon.SpawnTracer(weapon.muzzlePoint.position, targetPoint);

        if (data.fireSound)
            AudioSource.PlayClipAtPoint(data.fireSound, weapon.muzzlePoint.position);

        if (data.muzzleFlashPrefab)
            Object.Instantiate(data.muzzleFlashPrefab, weapon.muzzlePoint.position, weapon.muzzlePoint.rotation);
    }

}