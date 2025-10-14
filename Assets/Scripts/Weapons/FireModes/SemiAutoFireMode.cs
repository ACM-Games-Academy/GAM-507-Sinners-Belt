using UnityEngine;

public class SemiAutoFireMode : IFireMode
{
    private Weapon weapon;
    private Camera mainCamera;

    public void Initialize(Weapon weapon)
    {
        this.weapon = weapon;
        mainCamera = Camera.main;
    }

    public void Fire()
    {
        Vector3 targetPoint = weapon.GetCameraTargetPoint();
        Vector3 muzzlePos = weapon.muzzlePoint.position;
        Vector3 direction = (targetPoint - muzzlePos).normalized;

        if (Physics.Raycast(muzzlePos, direction, out RaycastHit hit, weapon.range, weapon.hitMask))
        {
            // TODO: Damage logic
            weapon.SpawnTracer(muzzlePos, hit.point);
        }
        else
        {
            Vector3 missPoint = muzzlePos + direction * weapon.range;
            weapon.SpawnTracer(muzzlePos, missPoint);
        }
    }
}