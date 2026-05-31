using UnityEngine;
using TMPro; // CRUCIAL: Gives us access to TextMeshPro components
using System.Collections; // Required for using Coroutines (timers)

public class LevelExit : MonoBehaviour
{
    public static LevelExit Instance;

    [Header("UI Display Settings")]
    [SerializeField] private TextMeshProUGUI warningText; // Drag your UI Text element here
    [SerializeField] private float textDuration = 3f; // How long the message stays visible

    private int enemyCount;
    private Coroutine clearTextCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        Debug.Log("Level Exit initialized. Total enemies to defeat: " + enemyCount);

        // Make sure the warning text is completely empty/blank when the game starts
        if (warningText != null)
        {
            warningText.text = "";
        }
    }

    public void EnemyDefeated()
    {
        enemyCount--;
        Debug.Log("An enemy was defeated! Enemies remaining: " + enemyCount);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // CHECK: Are there still enemies left alive?
            if (enemyCount > 0)
            {
                // Shows the message on the screen
                ShowWarningMessage($"Level Exit Blocked! Clear the room first, {enemyCount} enemies remaining.");
                return; 
            }

            // If the enemyCount is 0, proceed to trigger the win
            GameManager manager = FindFirstObjectByType<GameManager>();

            if (manager != null)
            {
                // Clears the warning text just in case it was showing
                if (warningText != null) warningText.text = "";
                
                manager.TriggerWin();
                Debug.Log("Level Exit Triggered: Win Screen Active.");
            }
            else
            {
                Debug.LogError("No GameManager found in the scene! Make sure you have one.");// Debug to check that GameManager exists
            }
        }
    }

    // Method to display win condition message
    private void ShowWarningMessage(string message)
    {
        if (warningText != null)
        {
            warningText.text = message;

            // If a previous timer is already counting down, stops it so they don't overlap
            if (clearTextCoroutine != null)
            {
                StopCoroutine(clearTextCoroutine);
            }

            // Starts a new timer to clear the message after a few seconds
            clearTextCoroutine = StartCoroutine(ClearTextAfterDelay(textDuration));
        }
    }

    // This background timer waits for X seconds, then blanks out the text
    private IEnumerator ClearTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        warningText.text = "";
    }
}