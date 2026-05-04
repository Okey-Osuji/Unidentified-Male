using UnityEngine;

public class LevelExit : MonoBehaviour
{
    // We remove the winCanvas variable here because the GameManager already has it!

    void OnTriggerEnter2D(Collider2D other)
    {
        // Verifies that it's the player GameObject
        if (other.CompareTag("Player"))
        {
            // Finds the GameManager in the scene and tells it to trigger the Win
            GameManager manager = FindFirstObjectByType<GameManager>();

            if (manager != null)
            {
                manager.TriggerWin();
                Debug.Log("Level Exit Triggered: Win Screen Active.");
            }
            else
            {
                Debug.LogError("No GameManager found in the scene! Make sure you have one.");
            }
        }
    }
}