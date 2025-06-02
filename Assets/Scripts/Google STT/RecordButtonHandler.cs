using UnityEngine;

public class RecordButtonHandler : MonoBehaviour
{
    public GoogleCloudSTT stt;

    public void StartRecord()
    {
        if (stt == null) return;

        if (stt.IsTtsPlaying())
        {
            stt.StopTts();
        }

        stt.StartRecording();
    }

    public void StopRecord()
    {
        if (stt == null) return;

        stt.StopRecording();
    }
}