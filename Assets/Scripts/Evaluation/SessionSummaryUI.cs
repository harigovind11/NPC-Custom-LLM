using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


[System.Serializable]
public class DetailedAnalysis
{
    public CommunicationSkills communication_skills;
}

[System.Serializable]
public class CommunicationSkills
{
    public float score;
    public string[] strengths;
    public string[] improvements;
}

[System.Serializable]
public class Feedback
{
    public int turn;
    public string timestamp;
    public string feedback;
    public float score;
    public string emotional_state;
    public DetailedAnalysis detailed_analysis;
    public string scenario_id;
}

[System.Serializable]
public class EmotionalStates
{
    public SerializableDictionary<string, int> emotion_distribution;
    public int positive_interactions;
    public int negative_interactions;
    public string dominant_emotion;
}

[System.Serializable]
public class Metrics
{
    public int total_interactions;
    public float average_score;
    public string score_trend;
    public string performance_level;
    public EmotionalStates emotional_states;
    public int scenarios_practiced;
    public string[] improvement_areas;
}

// Serializable dictionary helper class
[System.Serializable]
public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
{
    public List<TKey> keys = new List<TKey>();
    public List<TValue> values = new List<TValue>();

    private Dictionary<TKey, TValue> _dict = new Dictionary<TKey, TValue>();

    public void OnBeforeSerialize() { }
    public void OnAfterDeserialize()
    {
        _dict = new Dictionary<TKey, TValue>();
        for (int i = 0; i < keys.Count; i++)
            _dict[keys[i]] = values[i];
    }

    public Dictionary<TKey, TValue> ToDictionary() => _dict;
}

[System.Serializable]
public class SessionSummaryResponse
{
    public string session_id;
    public Feedback[] feedbacks;
    public Metrics metrics;
    public int total_feedbacks;
}

public class SessionSummaryUI : MonoBehaviour
{
    public TextMeshProUGUI summaryText; // Drag and drop in Inspector
    public VRConfig config;
    void Start()
    {
        string sessionId = SessionData.sessionId;
        Debug.Log("Session ID passed: " + sessionId);
        StartCoroutine(GetSessionSummary(sessionId));
    }

    IEnumerator GetSessionSummary(string sessionId)
    {
        string url = $"{config.baseUrl}/session/summary/{sessionId}";
        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to fetch summary: " + request.error);
            summaryText.text = "Error loading session summary.";
        }
        else
        {
            string json = request.downloadHandler.text;
            Debug.Log("Raw JSON:\n" + json);

            // Unity's JsonUtility does not support nested arrays well. If needed use Newtonsoft.Json
            SessionSummaryResponse data = Newtonsoft.Json.JsonConvert.DeserializeObject<SessionSummaryResponse>(json);

            DisplaySummary(data);
        }
    }
    void DisplaySummary(SessionSummaryResponse data)
    {
        if (data == null)
        {
            summaryText.text = "Summary data is null.";
            return;
        }

        string feedbackText = (data.feedbacks != null && data.feedbacks.Length > 0)
            ? $"- Turn {data.feedbacks[0].turn}: {data.feedbacks[0].feedback} (Score: {data.feedbacks[0].score})"
            : "No feedback yet.";

        string emotions = "Unavailable";
        if (data.metrics != null && data.metrics.emotional_states != null &&
            data.metrics.emotional_states.emotion_distribution != null)
        {
            var emotionDict = data.metrics.emotional_states.emotion_distribution.ToDictionary();
            emotions = string.Join(", ", emotionDict.Select(kv => $"{kv.Key}: {kv.Value}"));
        }

        string improvementAreas = (data.metrics?.improvement_areas != null)
            ? string.Join(", ", data.metrics.improvement_areas)
            : "None";

        string summary =
            // $" Session ID: {data.session_id}\n" +
            $" Feedback: {feedbackText}\n" +
            "--------------------------------------\n" +
            $" Metrics:\n" +
            "--------------------------------------\n" +
            $"• Total Interactions: {data.metrics?.total_interactions}\n" +
            $"• Avg. Score: {data.metrics?.average_score}\n" +
            $"• Score Trend: {data.metrics?.score_trend}\n" +
            $"• Performance: {data.metrics?.performance_level}\n" +
            $" Emotional States:\n" +
            "--------------------------------------\n" +
            $"• Dominant: {data.metrics?.emotional_states?.dominant_emotion}\n" +
            $"• Distribution: {emotions}\n" +
            $"• Positive: {data.metrics?.emotional_states?.positive_interactions}\n" +
            $"• Negative: {data.metrics?.emotional_states?.negative_interactions}\n" +
            "--------------------------------------\n" +
            $"🎯 Areas to Improve:\n{improvementAreas}";

        summaryText.text = summary;
    }


}
