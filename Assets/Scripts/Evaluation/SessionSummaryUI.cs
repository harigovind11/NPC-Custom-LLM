using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

[System.Serializable]
public class SessionDataInfo
{
    public string created_at;
    public string last_activity;
    public int conversation_count;
}

[System.Serializable]
public class SkillBreakdown
{
    public float communication_skills;
    public float problem_solving;
    public float customer_service;
    public float scenario_handling;
}

[System.Serializable]
public class SessionSummaryResponse
{
    public string session_id;
    public SessionDataInfo session_data;
    public int total_messages;
    public int user_interactions;
    public float average_score;
    public int total_evaluations;
    public string conversation_duration;
    public string performance_trend;
    public string[] scenarios_completed;
    public SkillBreakdown skill_breakdown;
}

public class SessionSummaryUI : MonoBehaviour
{
    public TextMeshProUGUI summaryText; // Drag and drop in Inspector

    void Start()
    {
        string sessionId = SessionData.sessionId;
        Debug.Log("Session ID passed: " + sessionId);
        StartCoroutine(GetSessionSummary(sessionId));
    }

    IEnumerator GetSessionSummary(string sessionId)
    {
        string url = $"https://vr-training-bot-api--d8e8mmg.bluetree-7578d21d.eastus.azurecontainerapps.io{sessionId}";
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

            // Unity's JsonUtility does not support nested arrays well. If needed use Newtonsoft.Json
            SessionSummaryResponse data = JsonUtility.FromJson<SessionSummaryResponse>(json);

            DisplaySummary(data);
        }
    }

    void DisplaySummary(SessionSummaryResponse data)
    {
        string summary =
            $"🆔 Session ID: {data.session_id}\n" +
            $"🕒 Created At: {data.session_data.created_at}\n" +
            $"🔄 Last Activity: {data.session_data.last_activity}\n" +
            $"💬 Conversations: {data.session_data.conversation_count}\n" +
            $"📨 Total Messages: {data.total_messages}\n" +
            $"👥 User Interactions: {data.user_interactions}\n" +
            $"⭐ Avg. Score: {data.average_score}\n" +
            $"📊 Evaluations: {data.total_evaluations}\n" +
            $"⏱ Duration: {data.conversation_duration}\n" +
            $"📈 Trend: {data.performance_trend}\n\n" +
            $"✅ Scenarios Completed:\n- {string.Join("\n- ", data.scenarios_completed)}\n\n" +
            $"🧠 Skills:\n" +
            $"• Communication: {data.skill_breakdown.communication_skills}\n" +
            $"• Problem Solving: {data.skill_breakdown.problem_solving}\n" +
            $"• Customer Service: {data.skill_breakdown.customer_service}\n" +
            $"• Scenario Handling: {data.skill_breakdown.scenario_handling}";

        summaryText.text = summary;
    }
}
