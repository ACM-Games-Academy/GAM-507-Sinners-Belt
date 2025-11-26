using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    //Play Button
    public void PlayButton()
    {
        Debug.Log("Play Button Pressed");
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    //Quit Button
    public void QuitButton()
    {   
        Debug.Log("Quit Button Pressed");
        Application.Quit();
    }
}
