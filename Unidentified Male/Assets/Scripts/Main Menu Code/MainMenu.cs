using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour

{
    public GameObject mainMenuPanel;//GameObject to store main menu UI
    public GameObject settingsPanel;//GameObject to store settings UI
    public GameObject quitConfirmPanel;//Gameobject to store quitting confirmation UI
    
    public void Start()
    {
        mainMenuPanel.SetActive(true);
        quitConfirmPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }
    public void PlayGame()
    {
        //Boots/Loads up the next scene through Scene Manager for the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OpenSettings()
    {
        //Boots/Loads the settings panel
        settingsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        quitConfirmPanel.SetActive(false);
    }

    public void ShowQuitConfirm()
    {
        //Sets quitConfirmation panel to visible
        quitConfirmPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    public void CancelQuit()
    {
        //Sets quitConfirmation to invisible and returns to main menu
        quitConfirmPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void ExitGame()
    {
        //Exits Application
        Debug.Log("Exiting Game...");
        Application.Quit();
    }
}
