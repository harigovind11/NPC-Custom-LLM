using UnityEngine;

public class TrainingSessionManager : MonoBehaviour
{
    private string sessionId;

    private GoogleCloudTTS tts;
    private void Start()
    {
        InitializeTraining();
        tts = FindFirstObjectByType<GoogleCloudTTS>();
    }

    private void InitializeTraining()
    {
        Debug.Log("[TrainingSessionManager] Initializing training session...");
        VRTrainingAPI.CreateSession(OnSessionCreated);
    }

    public string preSelectedScenarioId = ScenarioSelection.SelectedScenarioId;

    private void OnSessionCreated(string createdSessionId)
    {
        if (string.IsNullOrEmpty(createdSessionId))
        {
            Debug.LogError("[TrainingSessionManager] Failed to create session.");
            return;
        }

        sessionId = createdSessionId;
        Debug.Log("[TrainingSessionManager] Session created: " + sessionId);

        if (!string.IsNullOrEmpty(preSelectedScenarioId))
        {
            Debug.Log("[TrainingSessionManager] Using pre-selected scenario ID: " + preSelectedScenarioId);
            ScenarioSelection.SelectedScenarioId = preSelectedScenarioId;
            // Optionally proceed directly to next step if needed
        }
        else
        {
            VRTrainingAPI.GetScenarios(OnScenariosLoaded);
        }
    }

    private void OnScenariosLoaded(string scenarioJson)
    {
        if (string.IsNullOrEmpty(scenarioJson))
        {
            Debug.LogError("[TrainingSessionManager] Failed to load scenario.");
            return;
        }

        Debug.Log("[TrainingSessionManager] Scenario loaded:\n" + scenarioJson);

        // TODO: Parse JSON and optionally populate UI for scenario selection
    }

    /// <summary>
    /// Call this method from your STT system to send recognized speech to the VR Training API.
    /// </summary>
    public void OnSpeechRecognized(string recognizedText)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            Debug.LogWarning("[TrainingSessionManager] Session not initialized. Speech input ignored.");
            return;
        }

        if (string.IsNullOrEmpty(recognizedText))
        {
            Debug.LogWarning("[TrainingSessionManager] Empty speech input.");
            return;
        }

        Debug.Log($"[TrainingSessionManager] Sending recognized speech: \"{recognizedText}\"");
        VRTrainingAPI.SendMessage(ScenarioSelection.SelectedScenarioId, recognizedText, OnMessageResponseReceived);
    }

    private void OnMessageResponseReceived(string responseJson)
    {
        if (string.IsNullOrEmpty(responseJson))
        {
            Debug.LogError("[TrainingSessionManager] No response received from API.");
            return;
        }

        // Deserialize into a structured object
        VRChatResponse parsed = JsonUtility.FromJson<VRChatResponse>(responseJson);

        if (parsed != null)
        {
            Debug.Log("🤖 Bot says: " + parsed.response);
            Debug.Log("🧠 Emotion: " + parsed.emotional_state);
            Debug.Log("📈 Score: " + parsed.evaluation_score);
            Debug.Log("📝 Feedback: " + parsed.feedback);

            // Optional: Speak response or display in UI
            
            tts.Speak(parsed.response);
            
            
            // yourUIText.text = parsed.response;
        }
        else
        {
            Debug.LogWarning("⚠️ Failed to parse response JSON.");
        }
    }

}
