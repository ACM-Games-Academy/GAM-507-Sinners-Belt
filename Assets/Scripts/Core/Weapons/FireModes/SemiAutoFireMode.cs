using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class SemiAutoFireMode : IFireMode
{
    public float FireRate => 1f / data.fireRate;
    public float Damage => data.damage;
    public float Range => data.range;
    public int AmmoConsumption => data.ammoConsumption;
    public float KickPower => data.kickPower;
    public int MaxAmmo => data.maxAmmo;
    public int CurrentAmmo => currentAmmo;
    public bool IsReloading => isReloading;

    private WeaponBase weapon;
    private readonly SemiAutoFireModeData data;
    private float lastFireTime;

    private int currentAmmo;
    private bool isReloading;

    public SemiAutoFireMode(SemiAutoFireModeData fireModeData)
    {
        data = fireModeData;
        currentAmmo = fireModeData.maxAmmo;
    }

    public void Initialize(WeaponBase weapon)
    {
        this.weapon = weapon;
    }

    public IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(data.reloadTime);
        isReloading = false;
        currentAmmo = MaxAmmo;
    }

    public bool TryUseAmmo(int count)
    {
        if (currentAmmo - count < 0) 
            return false;

        currentAmmo -= count;
        return true;
    }

    public FireResponse Fire()
    {
        if (isReloading) 
            return FireResponse.Reloading;

        if (Time.time - lastFireTime < FireRate)
            return FireResponse.FireRateDelay;

        if (!TryUseAmmo(AmmoConsumption))
            return FireResponse.NoAmmo;

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

        // Kick camera
        CinemachineImpulseSource impulseSource = weapon.cam.GetComponent<CinemachineImpulseSource>();
        impulseSource.GenerateImpulse(-weapon.cam.transform.forward * KickPower);

        return FireResponse.Fired;
    }
}