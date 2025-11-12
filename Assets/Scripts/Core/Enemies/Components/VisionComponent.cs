using UnityEngine;
using System;

/// <summary>
/// Very simple vision component that raises events when it sees the player (by tag).
/// Uses OverlapSphere + line-of-sight raycast for reliability.
/// </summary>
[RequireComponent(typeof(Collider))]
public class VisionComponent : MonoBehaviour
{
    [Header("Vision")]
    public float viewRadius = 15f;
    [Range(1f, 360f)] public float viewAngle = 120f;

    [Header("Masks")]
    public LayerMask obstacleMask;
    public LayerMask playerMask; // set to the player layer(s) in inspector

    public string playerTag = "Player";

    public event Action<Transform> PlayerDetected;
    public event Action PlayerLost;

    private Transform currentPlayer;

    private void Update()
    {
        ScanForPlayer();
    }

    private void ScanForPlayer()
    {
        // OverlapSphere to find potential candidates (cheap and robust)
        Collider[] hits = Physics.OverlapSphere(transform.position, viewRadius, playerMask);
        if (hits.Length == 0)
        {
            if (currentPlayer != null) { currentPlayer = null; PlayerLost?.Invoke(); }
            return;
        }

        // pick the first valid player in view cone and with line of sight
        foreach (var c in hits)
        {
            if (c == null || !c.gameObject.CompareTag(playerTag)) continue;

            Vector3 dir = (c.transform.position - transform.position).normalized;
            float dotAngle = Vector3.Angle(transform.forward, dir);
            if (dotAngle > viewAngle * 0.5f) continue;

            // raycast slightly above origin to avoid ground clipping
            if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, out RaycastHit hit, viewRadius, obstacleMask | playerMask))
            {
                // nothing hit? treat as lost
                continue;
            }

            // check that what we hit is the player
            if (((1 << hit.collider.gameObject.layer) & playerMask.value) != 0)
            {
                if (currentPlayer == null || currentPlayer != c.transform)
                {
                    currentPlayer = c.transform;
                    PlayerDetected?.Invoke(currentPlayer);
                }
                return;
            }
        }

        // if loop completes and no valid player found:
        if (currentPlayer != null) { currentPlayer = null; PlayerLost?.Invoke(); }
    }

    public bool CanSee(Transform target)
    {
        if (target == null) return false;

        Vector3 dir = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > viewAngle * 0.5f) return false;

        // line-of-sight raycast check
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, out RaycastHit hit, viewRadius, obstacleMask | playerMask))
        {
            return hit.transform == target;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
        Vector3 left = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle * 0.5f, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + left * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + right * viewRadius);
    }
}
