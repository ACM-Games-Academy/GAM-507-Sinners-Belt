using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [Header("References")]
    public GameObject tutorialTrigger;
    public GameObject tutorialPanel;
    public HealthComponent dummyEnemy;

    private bool hasSubscribed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        // Show UI
        tutorialPanel.SetActive(true);
    }

    private void OnTriggerExit(Collider other) 
    {
        if (!other.CompareTag("Player")) return;

        //Disable UI
        HideTutorialPanel();

    }
    private void HideTutorialPanel()
    {
        //Disable UI
        tutorialPanel.SetActive(false);
    }
}
