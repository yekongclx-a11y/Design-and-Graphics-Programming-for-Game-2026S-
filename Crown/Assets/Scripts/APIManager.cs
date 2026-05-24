using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic; // 用于维护长线剧情记忆链
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

    // 【长线历史剧情简报池】
    // 用于存放过往所有回合的政治结果摘要，彻底治好AI的“回合失忆症”
    private List<string> historicalSummaries = new List<string>();

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

    // 【对外接口：重置游戏时由 DialogueSystem 强行调用清洗】
    public void ClearGameMemory()
    {
        historicalSummaries.Clear();
        Debug.Log("[MemorySystem] 游戏历史记忆链已完全清空重置。");
    }

    void LoadEnv()
    {
        string envPath = Path.Combine(Application.streamingAssetsPath, ".env");
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
        string promptPath = Path.Combine(Application.streamingAssetsPath, "prompt_v1.txt");
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

        // 1. 将纯规则指令替换留给 systemRole 传参
        string formattedSystemPrompt = systemPrompt
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
            .Replace("{maxTurns}", maxTurns.ToString());

        // 2. 动态拼装包含“长线政治记忆”的当前 User 状态包
        StringBuilder userContentBuilder = new StringBuilder();
        userContentBuilder.AppendLine("====== GAME STATE MEMORY LAYER ======");
        if (historicalSummaries.Count == 0)
        {
            userContentBuilder.AppendLine("- This is the very beginning of the reign. No past crises recorded yet.");
        }
        else
        {
            userContentBuilder.AppendLine("- Summary of your past actions and political consequences:");
            foreach (var summary in historicalSummaries)
            {
                userContentBuilder.AppendLine($"  * {summary}");
            }
        }
        userContentBuilder.AppendLine("=====================================");
        userContentBuilder.AppendLine($"[CURRENT PLAYER INPUT]: \"{playerInput}\"");

        // 3. System 与 User 彻底角色解耦发送
        var requestBody = new
        {
            model = model,
            messages = new[]
            {
                new { role = "system", content = formattedSystemPrompt },
                new { role = "user", content = userContentBuilder.ToString() }
            },
            max_tokens = 600, 
            temperature = 0.7 
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
                Debug.Log("AI_REPLY:\n" + content);
                
                AIResponse aiResponse = JsonConvert.DeserializeObject<AIResponse>(content);
                
                if (aiResponse.triggerEvent == null)
                    aiResponse.triggerEvent = "none";

                // 4. 长线记忆自我沉淀：如果解出历史简报，自动压入深水池
                if (!string.IsNullOrEmpty(aiResponse.historySummary))
                {
                    historicalSummaries.Add($"[Round {gs.currentRound}] {aiResponse.historySummary}");
                }

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

// ==================== 以下为完全闭环的数据实体解析类 ====================

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
    public int affinityChange;     // 个人好感度更新接口
    public string historySummary;  // 10字历史摘要接口
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