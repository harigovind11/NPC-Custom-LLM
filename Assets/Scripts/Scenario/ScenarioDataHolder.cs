using UnityEngine;

public class ScenarioDataHolder : MonoBehaviour
{
    public static ScenarioDataHolder Instance { get; private set; }
    public Scenario selectedScenario;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
