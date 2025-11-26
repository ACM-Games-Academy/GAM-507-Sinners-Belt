
using UnityEngine;


public class DualFireGunController : WeaponBase
{
    public SemiAutoFireModeData semiAutoData;
    private SemiAutoFireMode semiAuto;

    private Animator animator;

    public GameObject crosshairUIShooting;

    public GameObject crosshairnotShootingUI;

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
        bool reloadSignal = false;

        // FIRE HELD
        if (Input.GetButton("Fire1"))
        {
            Initialize(semiAuto);

            FireResponse fireResponse = Fire();
            if (animator != null)
            {
                if (fireResponse == FireResponse.NoAmmo)
                    reloadSignal = true;

                if (fireResponse != FireResponse.NoAmmo && fireResponse != FireResponse.NoFireMode && fireResponse != FireResponse.Reloading)
                {
                    animator.SetBool("IsFiring", true);
                    //Enable ShootingCrosshair UI
                    if (crosshairUIShooting != null) crosshairUIShooting.SetActive(true);
                    if (crosshairnotShootingUI != null) crosshairnotShootingUI.SetActive(false);

                }
                else
                {
                    animator.SetBool("IsFiring", false);
                    //Disable ShootingCrosshair UI
                    if (crosshairUIShooting != null) crosshairUIShooting.SetActive(false);
                    if (crosshairnotShootingUI != null) crosshairnotShootingUI.SetActive(true);

                }
            }
        }
        else
        {
            if (animator != null)
                animator.SetBool("IsFiring", false);

            //Disable ShootingCrosshair UI
            if (crosshairUIShooting != null) crosshairUIShooting.SetActive(false);
            if (crosshairnotShootingUI != null) crosshairnotShootingUI.SetActive(true);


        }

        // RELOAD
        if (Input.GetKeyDown(KeyCode.R) || reloadSignal)
        {
            bool didReload = TryReload();

            if (animator != null && didReload)
                animator.SetTrigger("Reload");
        }
    }

}
