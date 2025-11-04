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

    void Update()
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
    }


}