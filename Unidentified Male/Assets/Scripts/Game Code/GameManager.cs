using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject winCanvas;
    public GameObject gameOverCanvas;
    public GameObject pauseMenuCanvas;

    private bool isPaused = false;

    void Start()
    {
        // Forces every panel screen off just in case I forgot
        winCanvas.SetActive(false);
        gameOverCanvas.SetActive(false);
        pauseMenuCanvas.SetActive(false);

        Time.timeScale = 1f; // Ensures that the game isn't accidentally frozen
    }

    void Update()
    {
        // Toggles Pause/Play with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        pauseMenuCanvas.SetActive(true);
        Time.timeScale = 0f; // Freezes game physics/movement
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenuCanvas.SetActive(false);
        Time.timeScale = 1f; // Resumes game physics/movement
        isPaused = false;
    }

    public void TriggerWin()
    {
        winCanvas.SetActive(true); // Triggers the Win panel
        Time.timeScale = 0f;
    }

    public void TriggerGameOver()
    {
        gameOverCanvas.SetActive(true); // Triggers the Game Over panel
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Resets time before reloading
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Always resets time before changing scenes
        SceneManager.LoadScene("Main Menu"); 
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit(); 
    }
}