using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UIElements;

public class CountdownTimer : MonoBehaviour
{
    [SerializeField] private float timeLimit = 300f; // 5 minutes in seconds
    [SerializeField] private string nextSceneName = "Result";
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private Button skipButton;
    
    private float currentTime;

    private void Start()
    {
        currentTime = timeLimit;
    }

    private void Update()
    {
        currentTime -= Time.deltaTime;
        currentTime = Mathf.Clamp(currentTime, 0f, timeLimit);

        // Format time as MM:SS
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        countdownText.text = $"{minutes:00}:{seconds:00}";

        if (currentTime <= 0f)
        {
            SceneManager.LoadScene(nextSceneName);
        }else if (currentTime <= 10f)
        {
            countdownText.color = Color.red; // Change text color to red when time is low
        }
            
    }


}
