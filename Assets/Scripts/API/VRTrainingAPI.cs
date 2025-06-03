using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class VRTrainingAPI : MonoBehaviour
{
    [Header("API Settings")]
    public string baseUrl = "https://vr-training-bot-api--d8e8mmg.bluetree-7578d21d.eastus.azurecontainerapps.io";

    private string sessionId;
    public static VRTrainingAPI Instance;

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

    // Public method to create a session
    public static void CreateSession(Action<string> onCreated)
    {
        if (Instance == null)
        {
            Debug.LogError("[VRTrainingAPI] No instance found.");
            return;
        }

        Instance.StartCoroutine(Instance.CreateSessionCoroutine(onCreated));
    }

    private IEnumerator CreateSessionCoroutine(Action<string> onCreated)
    {
        string url = $"{baseUrl}/session/create";
        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(url, ""))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                var response = JsonUtility.FromJson<SessionResponse>(json);
                sessionId = response.session_id;
                Debug.Log("[VRTrainingAPI] Session Created: " + sessionId);
                onCreated?.Invoke(sessionId);
            }
            else
            {
                Debug.LogError($"[VRTrainingAPI] Failed to create session: {request.error}");
                onCreated?.Invoke(null);
            }
        }
    }

    // Get all training scenarios
    public static void GetScenarios(Action<string> onResult)
    {
        if (Instance == null)
        {
            Debug.LogError("[VRTrainingAPI] No instance found.");
            return;
        }

        Instance.StartCoroutine(Instance.GetScenariosCoroutine(onResult));
    }

    private IEnumerator GetScenariosCoroutine(Action<string> onResult)
    {
        string url = $"{baseUrl}/scenarios";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                onResult?.Invoke(json);
            }
            else
            {
                Debug.LogError($"[VRTrainingAPI] Failed to get scenarios: {request.error}");
                onResult?.Invoke(null);
            }
        }
    }

    // Send a message (non-streaming)
    public static void SendMessage(string scenarioId, string userInput, Action<string> onResponse)
    {
        if (Instance == null)
        {
            Debug.LogError("[VRTrainingAPI] No instance found.");
            return;
        }

        Instance.StartCoroutine(Instance.SendMessageCoroutine(scenarioId, userInput, onResponse));
    }

    private IEnumerator SendMessageCoroutine(string scenarioId, string userInput, Action<string> onResponse)
    {
        string url = $"{baseUrl}/chat/non-streaming";

        ChatRequestBody body = new ChatRequestBody
        {
            session_id = sessionId,
            scenario_id = scenarioId,
            user_input = userInput
        };

        string jsonBody = JsonUtility.ToJson(body);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                Debug.Log("[VRTrainingAPI] Received response: " + json);
                onResponse?.Invoke(json);
            }
            else
            {
                Debug.LogError($"[VRTrainingAPI] Message failed: {request.error}");
                onResponse?.Invoke(null);
            }
        }
    }

    [Serializable]
    private class SessionResponse
    {
        public string session_id;
        public string message;
    }

    [Serializable]
    private class ChatRequestBody
    {
        public string session_id;
        public string scenario_id;
        public string user_input;
    }
}
