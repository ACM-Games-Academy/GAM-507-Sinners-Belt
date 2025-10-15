using UnityEngine;

[CreateAssetMenu(fileName = "FireModeData", menuName = "Weapons/Fire Mode Data", order = 0)]
public class FireModeData : ScriptableObject
{
    [Header("Weapon Stats")]
    [Tooltip("Shots per second, e.g. 0.25 = 4 shots per second")]
    public float fireRate = 0.25f;
    public float damage = 25f;
    public float range = 1000f;

    [Header("Extra (optional)")]
    public AudioClip fireSound;
    public GameObject muzzleFlashPrefab;
}