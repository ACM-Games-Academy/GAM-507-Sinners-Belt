using UnityEngine;

[CreateAssetMenu(fileName = "FireModeData", menuName = "FireModeData/SemiAuto")]
public class SemiAutoFireModeData : ScriptableObject
{
    [Header("Weapon Stats")]
    [Tooltip("Shots per second")]
    public float fireRate = 8f;
    public float damage = 20f;
    public float range = 1000f;
    [Tooltip("Ammo used per shot")]
    public float ammoConsumption = 1f;
    [Tooltip("Kick (jolt) power on firing (recommended = 0.01f)")]
    public float kickPower = 0.01f;

    [Header("Extra (optional)")]
    public AudioClip fireSound;
    public GameObject muzzleFlashPrefab;
}