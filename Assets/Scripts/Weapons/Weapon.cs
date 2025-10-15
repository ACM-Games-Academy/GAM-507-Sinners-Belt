using UnityEngine;
using System.Collections;

public abstract class Weapon : MonoBehaviour
{
    [Header("Weapon Setup")]
    public Transform muzzlePoint;
    public LayerMask hitMask;
    public GameObject bulletTrailPrefab;

    [Header("Fire Mode Data")]
    public FireModeData fireModeData;

    protected IFireMode fireMode;
    protected Camera cam;

    protected virtual void Awake()
    {
        cam = Camera.main;
    }

    public virtual void Initialize(IFireMode mode)
    {
        fireMode = mode;
        fireMode.Initialize(this, fireModeData);
    }

    public virtual void Fire()
    {
        fireMode?.Fire();
    }

    public Vector3 GetCameraTargetPoint()
    {
        float range = fireMode?.Range ?? 1000f;
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask))
            return hit.point;

        return ray.origin + ray.direction * range;
    }

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
            trail.position = Vector3.Lerp(start, end, progress);
            yield return null;
        }

        yield return new WaitForSeconds(0.05f);
        Destroy(trail.gameObject);
    }
}