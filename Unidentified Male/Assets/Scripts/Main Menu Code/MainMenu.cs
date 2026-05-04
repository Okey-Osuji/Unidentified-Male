using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour

{
    public GameObject settingsPanel;//GameObject to store settings UI
    
    public void PlayGame()
    {
        //Boots/Loads up the next scene through Scene Manager for the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OpenSettings()
    {
        //Boots/Loads the settings panel
        settingsPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Debug.Log("Are you sure you want to QUIT?");//Debug message to log if player indeed wants to exit game
        Application.Quit();//Exits the application/game entirely
    }
}
