using UnityEngine;
using System.Collections;

public abstract class Weapon : MonoBehaviour
{
    [Header("Weapon Stats")]
    public Transform muzzlePoint;
    public LayerMask hitMask;
    public float range = 1000f;
    public float fireRate = 0.2f;
    public GameObject bulletTrailPrefab;

    protected float lastFireTime;
    protected IFireMode fireMode;

    protected Camera cam;

    protected virtual void Awake()
    {
        cam = Camera.main;
    }

    public virtual void Initialize(IFireMode mode)
    {
        fireMode = mode;
        fireMode.Initialize(this);
    }

    public virtual void Fire()
    {
        if (Time.time - lastFireTime < fireRate)
            return;

        lastFireTime = Time.time;
        fireMode.Fire();
    }

    /// <summary>
    /// Gets the target point based on the camera center ray (better for first-person shooters)
    /// </summary>
    public Vector3 GetCameraTargetPoint()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask))
        {
            return hit.point;
        }

        return ray.origin + ray.direction * range;
    }

    /// <summary>
    /// Handles visual trail for hitscan tracers (so players get visual feedback on firing)
    /// </summary>
    public void SpawnTracer(Vector3 start, Vector3 end)
    {
        if (!bulletTrailPrefab) return;

        Transform trail = Instantiate(bulletTrailPrefab).transform;
        StartCoroutine(AnimateTracer(trail, start, end));
    }

    private IEnumerator AnimateTracer(Transform trail, Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        float speed = 200f;
        float traveled = 0f;

        while (traveled < distance)
        {
            traveled += speed * Time.deltaTime;
            float progress = traveled / distance;
            
            Vector3 currentPos = Vector3.Lerp(start, end, progress);
            trail.position = currentPos;

            yield return null;
        }

        yield return new WaitForSeconds(0.05f);
        Destroy(trail.gameObject);
    }
}