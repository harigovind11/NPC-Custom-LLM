using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScenarioUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject scenarioButtonPrefab; // Assign ScenarioButton prefab
    public Transform contentParent;         // Assign ScrollView/Viewport/Content

    [Header("Data Source")]

    public  VRConfig config;
   
    void Start()
    {
        StartCoroutine(LoadScenarios());
    }

    IEnumerator LoadScenarios()
    {

        UnityWebRequest request = UnityWebRequest.Get($"{config.baseUrl}/scenarios");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string rawJson = request.downloadHandler.text;
            ScenarioWrapper wrapper = JsonUtility.FromJson<ScenarioWrapper>(rawJson);
            DisplayScenarioButtons(wrapper.scenarios);
        }
        else
        {
            Debug.LogError("Failed to load: " + request.error);
        }
    }

    void DisplayScenarioButtons(List<Scenario> scenarios)
    {
        Debug.Log($"Instantiating {scenarios.Count} buttons...");

        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        foreach (Scenario scenario in scenarios)
        {
            GameObject buttonObj = Instantiate(scenarioButtonPrefab, contentParent);

            var titleText = buttonObj.transform.Find("TitleText");
            if (titleText == null)
            {
                Debug.LogError("TitleText not found in button prefab!");
                continue;
            }

            var textComponent = titleText.GetComponent<TextMeshProUGUI>();
            if (textComponent == null)
            {
                Debug.LogError("Text component missing on TitleText!");
                continue;
            }

            textComponent.text = scenario.customer_type;
            
            buttonObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log($"Clicked: {scenario.customer_type}");
                ScenarioSelection.SelectedScenarioId = scenario.id;
                SceneManager.LoadScene("Interaction");
            });
        }
        
        
    }

}