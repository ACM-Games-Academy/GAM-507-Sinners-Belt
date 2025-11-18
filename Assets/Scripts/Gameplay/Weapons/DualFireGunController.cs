using UnityEngine;

public class DualFireGunController : WeaponBase
{
    public SemiAutoFireModeData semiAutoData;
    private SemiAutoFireMode semiAuto;

    private Animator animator;

    private void Awake()
    {
        semiAuto = new SemiAutoFireMode(semiAutoData);
    }

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        // FIRE HELD
        if (Input.GetButton("Fire1"))
        {
            Initialize(semiAuto);

            FireResponse fireResponse = Fire();
            if (animator != null)
            {
                if (fireResponse != FireResponse.NoAmmo && fireResponse != FireResponse.NoFireMode && fireResponse != FireResponse.Reloading)
                {
                    animator.SetBool("IsFiring", true);
                }
                else
                {
                    animator.SetBool("IsFiring", false);
                }
            }
        }
        else
        {
            if (animator != null)
                animator.SetBool("IsFiring", false);
        }

        // RELOAD
        if (Input.GetKeyDown(KeyCode.R))
        {
            bool didReload = TryReload();

            if (animator != null && didReload)
                animator.SetTrigger("Reload");
        }
    }

}