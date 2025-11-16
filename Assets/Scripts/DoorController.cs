using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Animation")]
    public Animator leftDoorAnimator;
    public Animator rightDoorAnimator;
    public string openTriggerName = "Open";
    public string closeTriggerName = "Close";

    private void OnTriggerEnter(Collider other)
    {
            if (other.CompareTag("Player"))
            {
                leftDoorAnimator.SetTrigger(openTriggerName);
                rightDoorAnimator.SetTrigger(openTriggerName);
            }
    }


    private void OnTriggerExit(Collider other)
    {
            if (other.CompareTag("Player"))
            {
                leftDoorAnimator.SetTrigger(closeTriggerName);
                rightDoorAnimator.SetTrigger(closeTriggerName);
            }
    }
}
