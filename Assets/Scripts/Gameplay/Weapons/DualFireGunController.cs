using UnityEngine;

public class DualFireGunController : WeaponBase
{
    public SemiAutoFireModeData semiAutoData;
    private SemiAutoFireMode semiAuto;

    protected override void Awake()
    {
        base.Awake();
        semiAuto = new SemiAutoFireMode(semiAutoData);
    }
    private void LateUpdate()
    {
        
        if (Input.GetButton("Fire1"))
        {
            Initialize(semiAuto);
            Fire();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }

        // muzzle alignment fix
        if (cam != null && muzzlePoint != null)
        {
            muzzlePoint.rotation = cam.transform.rotation;

            // uncomment if muzzle position moves to the camera
            // muzzlePoint.position = cam.transform.position + cam.transform.forward * 0.5f;
        }
    }




}