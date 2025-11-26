using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeScreenUI : MonoBehaviour
{
    [Header("Initial fade settings (start of game)")]
    [SerializeField] private float fadeDelayStartGame = 4f;
    [SerializeField] private float fadeTimeStartGame = 3f;

    [Header("References")]
    [SerializeField] private Image pannelImage;

    private IEnumerator Start()
    {
        SetUIEnabled(true);
        SetColor(new Color(0, 0, 0, 1));
        yield return new WaitForSeconds(fadeDelayStartGame);
        Fade(0f, fadeTimeStartGame);
        yield return new WaitForSeconds(fadeTimeStartGame);
        SetUIEnabled(false);
    }

    public void SetUIEnabled(bool enabled)
    {
        pannelImage.gameObject.SetActive(enabled);
    }

    public void Fade(float targetAlpha, float fadeTime)
    {
        StartCoroutine(PerformFade(targetAlpha, fadeTime));
    }

    public void SetColor(Color color)
    {
        pannelImage.color = color;
    }

    private IEnumerator PerformFade(float targetAlpha, float fadeTime)
    {
        Color currentColor = pannelImage.color;
        Color targetColor = currentColor;
        targetColor.a = targetAlpha;

        float baseSteps = 100 * (fadeTime + 1);
        float alphaPerStep = (targetColor.a - currentColor.a) / baseSteps;
        
        for (int i = 0; i < baseSteps; i++)
        {
            Color newColor = currentColor;
            newColor.a = newColor.a + (alphaPerStep * i);
            SetColor(newColor);

            yield return null;
        }
    }
}