using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
 
    
    private string _scenarioScene = "Scenario";
    private string _interactionScene = "Interaction";
    private string _resultScene = "Result";
    private ScenarioUIManager _scenarioUIManager;
 
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public  void LoadScenarioScene()
    {
        SceneManager.LoadScene(_scenarioScene);
        FindFirstObjectByType<ScenarioUIManager>().RefreshScenarios();


    }
    
    public  void LoadInteractionScene()
    {
        SceneManager.LoadScene(_interactionScene);
    }
    
    public  void LoadResultScene()
    {
        SceneManager.LoadScene(_resultScene);
    }
}
