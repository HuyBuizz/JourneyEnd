using UnityEngine;
using UnityEngine.UI;

public class GameClock : MonoBehaviour
{
    [Header("Settings")]
    public float realSecondsPerGameMinute = 1f; // 1 real second = 1 game minute
    public Text timeText;

    private float gameMinutes; // total in-game minutes

    void Start()
    {
        if (timeText == null)
            timeText = transform.Find("timeText").GetComponent<Text>();

        // Start at 14:00 (14 hours * 60 minutes)
        gameMinutes = 14 * 60;
    }

    void Update()
    {
        // Increase in-game minutes
        gameMinutes += Time.deltaTime * (60f / realSecondsPerGameMinute);

        // Wrap around after 24 hours (1440 minutes in a day)
        gameMinutes %= 1440f;

        // Convert to hours and minutes
        int hours = Mathf.FloorToInt(gameMinutes / 60f);
        int minutes = Mathf.FloorToInt(gameMinutes % 60f);

        // Display in HH:MM format
        timeText.text = $"{hours:00}:{minutes:00}";
    }
}
