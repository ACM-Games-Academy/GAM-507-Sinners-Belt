using UnityEngine;

public class MissionObjective : MonoBehaviour
{
    [SerializeField] private WaveTrigger[] triggersToEnable;

    private bool isEnabled = true;
    public AK.Wwise.Event ObjectiveCollectedSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out ObjectiveTracker objectiveTracker) && isEnabled)
        {
            isEnabled = false;

            // Enable additional waves of enemies
            foreach (WaveTrigger trigger in triggersToEnable)
            {
                trigger.EnableTrigger();
            }

            // Update mission objective in tracker dictionary
            objectiveTracker.SetFlag("MissionObjectiveCollected", true);
            ObjectiveCollectedSound.Post(gameObject);

            // Disable this object
            gameObject.SetActive(false);
        }
    }

    private void OnDrawGizmos()
    {
        if (!isEnabled) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
