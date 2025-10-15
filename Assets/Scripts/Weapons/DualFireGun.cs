using UnityEngine;

public class DualFireGun : Weapon
{
    private SemiAutoFireMode primary;

    protected override void Awake()
    {
        base.Awake();
        primary = new SemiAutoFireMode();
        Initialize(primary);
    }

    void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            Initialize(primary);
            Fire();
        }

        if (Input.GetButtonDown("Fire2"))
        {
            // Secondary fire
        }

        if (Input.GetButtonDown("Fire2"))
        {
            // Secondary release
        }
    }
}