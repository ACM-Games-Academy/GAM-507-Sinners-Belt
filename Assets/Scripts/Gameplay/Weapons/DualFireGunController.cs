using UnityEngine;

public class DualFireGunController : WeaponBase
{
    public SemiAutoFireModeData semiAutoData;
    private SemiAutoFireMode semiAuto;

    private Animator animator;

    protected override void Awake()
    {
        base.Awake();
        semiAuto = new SemiAutoFireMode(semiAutoData);
    }

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // FIRE HELD
        if (Input.GetButton("Fire1"))
        {
            Initialize(semiAuto);
            Fire();

            if (animator != null)
                animator.SetBool("IsFiring", true);
        }
        else
        {
            if (animator != null)
                animator.SetBool("IsFiring", false);
        }

        // RELOAD
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();

            if (animator != null)
                animator.SetTrigger("Reload");
        }
    }

}