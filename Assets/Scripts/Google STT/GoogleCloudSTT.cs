using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.Serialization;

public class GoogleCloudSTT : MonoBehaviour
{

    public string languageCode = "en-US";
    public AudioClip recordedClip;
    private string accessToken;
    private GoogleCredential credentials;
   
    public bool isTtsPlaying = false;
    
    [Serializable]
    private class GoogleCredential
    {
        public string type;
        public string project_id;
        public string private_key_id;
        public string private_key;
        public string client_email;
        public string client_id;
        public string auth_uri;
        public string token_uri;
        public string auth_provider_x509_cert_url;
        public string client_x509_cert_url;
    }

    void Start()
    {
        StartCoroutine(LoadCredentials());
    }

    public void StartRecording()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("No microphone found!");
            return;
        }

        Debug.Log("🎙️ Recording...");
        recordedClip = Microphone.Start(null, false, 15, 16000);
        
    }


  public void StopRecording()
   {
       if (recordedClip == null)
       {
           Debug.LogError("No recording to stop.");
           return;
       }
   
       Microphone.End(null);
       Debug.Log("✅ Recording complete.");
   
       int micChannels = recordedClip.channels;
       int sampleCount = recordedClip.samples;
   
       // Step 1: Get raw samples
       float[] samples = new float[sampleCount * micChannels];
       recordedClip.GetData(samples, 0);
   
       // Step 2: Convert stereo to mono (if needed)
       float[] monoSamples = new float[sampleCount];
       for (int i = 0; i < sampleCount; i++)
       {
           float sum = 0f;
           for (int ch = 0; ch < micChannels; ch++)
           {
               sum += samples[i * micChannels + ch];
           }
           monoSamples[i] = sum / micChannels;
       }
   
       // Step 3: Convert to PCM16 byte array
       byte[] audioBytes = ConvertToPCM(monoSamples);
   
       // Optional: Debug size to make sure it's not empty
       Debug.Log($"📦 Audio byte length: {audioBytes.Length}");
   
       if (audioBytes.Length == 0)
       {
           Debug.LogError("❌ Audio data is empty. Nothing to send.");
           return;
       }
   
       // Step 4: Send to Google STT
       StartCoroutine(SendToGoogleStt(audioBytes));
   }

  
  

    public void StopTts()
    {
        // Add your TTS stop logic here (e.g., audioSource.Stop();)
        isTtsPlaying = false;
        Debug.Log("🛑 TTS stopped.");
    }

    public bool IsTtsPlaying()
    {
        return isTtsPlaying;
    }

    byte[] ConvertToPCM(float[] samples)
    {
        var pcm = new List<byte>();
        foreach (float sample in samples)
        {
            short intData = (short)(sample * short.MaxValue);
            byte[] bytes = BitConverter.GetBytes(intData);
            pcm.AddRange(bytes);
        }
        return pcm.ToArray();
    }

    IEnumerator LoadCredentials()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("service-account"); // place in Resources
        if (jsonFile == null)
        {
            Debug.LogError("Could not load service-account.json from Resources.");
            yield break;
        }

        credentials = JsonConvert.DeserializeObject<GoogleCredential>(jsonFile.text);
        yield return StartCoroutine(GetAccessToken());
    }

    IEnumerator GetAccessToken()
    {
        string jwt = CreateJwt();
        WWWForm form = new WWWForm();
        form.AddField("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer");
        form.AddField("assertion", jwt);

        UnityWebRequest tokenRequest = UnityWebRequest.Post(credentials.token_uri, form);
        yield return tokenRequest.SendWebRequest();

        if (tokenRequest.result == UnityWebRequest.Result.Success)
        {
            var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(tokenRequest.downloadHandler.text);
            accessToken = response["access_token"].ToString();
            Debug.Log("🔐 Access token acquired.");
        }
        else
        {
            Debug.LogError("❌ Token request failed: " + tokenRequest.error);
        }
    }

    IEnumerator SendToGoogleStt(byte[] audioBytes)
    {
        string base64Audio = Convert.ToBase64String(audioBytes);

        var requestData = new
        {
            config = new
            {
                encoding = "LINEAR16",
                sampleRateHertz = 16000,
                languageCode = languageCode
            },
            audio = new
            {
                content = base64Audio
            }
        };

        string json = JsonConvert.SerializeObject(requestData);
        UnityWebRequest request = new UnityWebRequest("https://speech.googleapis.com/v1/speech:recognize", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);

        Debug.Log("🛰️ Sending audio to Google STT...");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ STT request failed: " + request.error);
        }
        else
        {
            Debug.Log("✅ STT response received");
            ProcessSttResponse(request.downloadHandler.text);
        }
    }

    void ProcessSttResponse(string jsonResponse)
    {
        try
        {
            var sttResult = JsonConvert.DeserializeObject<GoogleSTTResponse>(jsonResponse);
            if (sttResult.results != null && sttResult.results.Length > 0 &&
                sttResult.results[0].alternatives.Length > 0)
            {
                string transcript = sttResult.results[0].alternatives[0].transcript;
                if (!string.IsNullOrWhiteSpace(transcript))
                {
                    Debug.Log("🗣️ Final Transcript: " + transcript);
                    // GeminiAPI.SendPrompt(transcript);
                    FindFirstObjectByType<TrainingSessionManager>().OnSpeechRecognized(transcript);

                }
                else
                {
                    Debug.LogWarning("Transcript was empty.");
                }
            }
            else
            {
                Debug.LogWarning("STT returned no usable results.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to parse STT response: " + ex.Message);
        }
    }
    
    // === JWT Code (Same as TTS) ===

    string CreateJwt()
    {
        var header = new Dictionary<string, object> {
            { "alg", "RS256" }, { "typ", "JWT" }
        };

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var payload = new Dictionary<string, object> {
            { "iss", credentials.client_email },
            { "scope", "https://www.googleapis.com/auth/cloud-platform" },
            { "aud", credentials.token_uri },
            { "iat", now },
            { "exp", now + 3600 }
        };

        string headerEncoded = Base64UrlEncode(JsonConvert.SerializeObject(header));
        string payloadEncoded = Base64UrlEncode(JsonConvert.SerializeObject(payload));
        string unsignedToken = $"{headerEncoded}.{payloadEncoded}";

        var rsaParams = GetBouncyCastleRSAParameters(credentials.private_key);
        using var rsa = System.Security.Cryptography.RSA.Create();
        rsa.ImportParameters(rsaParams);

        byte[] signature = rsa.SignData(Encoding.UTF8.GetBytes(unsignedToken), System.Security.Cryptography.HashAlgorithmName.SHA256, System.Security.Cryptography.RSASignaturePadding.Pkcs1);
        string signatureEncoded = Base64UrlEncode(signature);

        return $"{unsignedToken}.{signatureEncoded}";
    }

    System.Security.Cryptography.RSAParameters GetBouncyCastleRSAParameters(string privateKeyPEM)
    {
        privateKeyPEM = privateKeyPEM.Replace("-----BEGIN PRIVATE KEY-----", "")
            .Replace("-----END PRIVATE KEY-----", "")
            .Replace("\n", "").Replace("\r", "").Trim();

        byte[] keyData = Convert.FromBase64String(privateKeyPEM);

        Org.BouncyCastle.Crypto.AsymmetricKeyParameter keyParameter = Org.BouncyCastle.Security.PrivateKeyFactory.CreateKey(keyData);
        var rsaParams = (Org.BouncyCastle.Crypto.Parameters.RsaPrivateCrtKeyParameters)keyParameter;

        return new System.Security.Cryptography.RSAParameters
        {
            Modulus = rsaParams.Modulus.ToByteArrayUnsigned(),
            Exponent = rsaParams.PublicExponent.ToByteArrayUnsigned(),
            D = rsaParams.Exponent.ToByteArrayUnsigned(),
            P = rsaParams.P.ToByteArrayUnsigned(),
            Q = rsaParams.Q.ToByteArrayUnsigned(),
            DP = rsaParams.DP.ToByteArrayUnsigned(),
            DQ = rsaParams.DQ.ToByteArrayUnsigned(),
            InverseQ = rsaParams.QInv.ToByteArrayUnsigned()
        };
    }

    string Base64UrlEncode(string input) => Base64UrlEncode(Encoding.UTF8.GetBytes(input));
    string Base64UrlEncode(byte[] input) =>
        Convert.ToBase64String(input).Replace("+", "-").Replace("/", "_").Replace("=", "");

    [Serializable]
    public class GoogleSTTResponse
    {
        public STTResult[] results;
    }

    [Serializable]
    public class STTResult
    {
        public STTAlternative[] alternatives;
    }

    [Serializable]
    public class STTAlternative
    {
        public string transcript;
    }
}
