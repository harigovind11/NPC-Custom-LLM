using UnityEngine;
using UnityEngine.UI;

public class ScenarioSceneManager : MonoBehaviour
{
    public Text scenarioIdText;

    void Start()
    {
        string selectedId = ScenarioSelection.SelectedScenarioId;
        scenarioIdText.text = "Scenario ID: " + selectedId;

        // TODO: you can use this ID to look up full data later if needed
    }
}