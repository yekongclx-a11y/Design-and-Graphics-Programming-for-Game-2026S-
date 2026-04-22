using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.IO;
using Newtonsoft.Json;

public class APIManager : MonoBehaviour
{
    public static APIManager Instance;

    private string apiUrl = "https://ai.liaobots.work/v1/chat/completions";
    private string apiKey = "";
    private string model = "gemini-3.1-flash-lite-preview";
    private string systemPrompt = "";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadEnv();
            LoadPrompt();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadEnv()
    {
        string envPath = Path.Combine(Application.dataPath, "../../.env");
        if (File.Exists(envPath))
        {
            string[] lines = File.ReadAllLines(envPath);
            foreach (string line in lines)
            {
                if (line.StartsWith("API_KEY="))
                {
                    apiKey = line.Substring("API_KEY=".Length).Trim();
                    Debug.Log("API Key loaded.");
                }
            }
        }
        else
        {
            Debug.LogError(".env file not found: " + envPath);
        }
    }

    void LoadPrompt()
    {
        string promptPath = Path.Combine(Application.dataPath,
                            "../../Docs/Prompts/prompt_v1.txt");
        if (File.Exists(promptPath))
        {
            systemPrompt = File.ReadAllText(promptPath, Encoding.UTF8);
            Debug.Log("Prompt loaded.");
        }
        else
        {
            Debug.LogError("prompt_v1.txt not found: " + promptPath);
        }
    }

    public void SendMessage(string npcName, string surfaceRequest,
                            string hiddenMotive, string playerInput,
                            int currentTurn, int maxTurns,
                            bool isDismiss,
                            System.Action<AIResponse> onComplete)
    {
        StartCoroutine(SendRequest(npcName, surfaceRequest,
                                   hiddenMotive, playerInput,
                                   currentTurn, maxTurns,
                                   isDismiss, onComplete));
    }

    IEnumerator SendRequest(string npcName, string surfaceRequest,
                             string hiddenMotive, string playerInput,
                             int currentTurn, int maxTurns,
                             bool isDismiss,
                             System.Action<AIResponse> onComplete)
    {
        GameStateManager gs = GameStateManager.Instance;

        string prompt = systemPrompt
            .Replace("{currentRound}", gs.currentRound.ToString())
            .Replace("{gold}", gs.gold.ToString())
            .Replace("{popularity}", gs.popularity.ToString())
            .Replace("{church}", gs.church.ToString())
            .Replace("{military}", gs.military.ToString())
            .Replace("{suspicion}", gs.suspicion.ToString())
            .Replace("{currentNPC}", npcName)
            .Replace("{surfaceRequest}", surfaceRequest)
            .Replace("{hiddenMotive}", hiddenMotive)
            .Replace("{currentTurn}", currentTurn.ToString())
            .Replace("{maxTurns}", maxTurns.ToString())
            .Replace("{playerInput}", playerInput);

        var requestBody = new
        {
            model = model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            max_tokens = 500,
            temperature = 0.8
        };

        string jsonBody = JsonConvert.SerializeObject(requestBody);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        request.timeout = 30;

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;

            try
            {
                var response = JsonConvert.DeserializeObject<OpenAIResponse>(responseText);
                string content = response.choices[0].message.content;
                content = content.Replace("```json", "").Replace("```", "").Trim();
                AIResponse aiResponse = JsonConvert.DeserializeObject<AIResponse>(content);
                if (aiResponse.triggerEvent == null)
                    aiResponse.triggerEvent = "none";
                onComplete?.Invoke(aiResponse);
            }
            catch (System.Exception e)
            {
                Debug.LogError("JSON解析失败: " + e.Message);
                onComplete?.Invoke(null);
            }
        }
        else
        {
            Debug.LogError("API请求失败: " + request.error);
            Debug.LogError("响应: " + request.downloadHandler.text);
            onComplete?.Invoke(null);
        }
    }
}

[System.Serializable]
public class AIResponse
{
    public string action;
    public string dialogue;
    public int gold;
    public int popularity;
    public int church;
    public int military;
    public int suspicion;
    public string triggerEvent = "none";
}

[System.Serializable]
public class OpenAIResponse
{
    public Choice[] choices;
}

[System.Serializable]
public class Choice
{
    public Message message;
}

[System.Serializable]
public class Message
{
    public string content;
}