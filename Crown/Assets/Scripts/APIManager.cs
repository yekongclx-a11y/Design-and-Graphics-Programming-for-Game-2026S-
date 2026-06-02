using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;

public class APIManager : MonoBehaviour
{
    public static APIManager Instance;

    private string systemPrompt = "";

    // Lazy fallback defaults — only used when config.json fields are empty.
    private const string FallbackUrl   = "https://generativelanguage.googleapis.com/v1beta/openai/chat/completions";
    private const string FallbackModel = "gemini-2.0-flash";

    private string ActiveUrl =>
        (GameConfig.Instance != null && !string.IsNullOrEmpty(GameConfig.Instance.Config.apiUrl))
            ? GameConfig.Instance.Config.apiUrl : FallbackUrl;

    private string ActiveKey =>
        GameConfig.Instance?.Config.apiKey ?? "";

    private string ActiveModel =>
        (GameConfig.Instance != null && !string.IsNullOrEmpty(GameConfig.Instance.Config.model))
            ? GameConfig.Instance.Config.model : FallbackModel;

    // 【长线历史剧情简报池】
    private List<string> historicalSummaries = new List<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPrompt();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ClearGameMemory()
    {
        historicalSummaries.Clear();
        Debug.Log("[MemorySystem] 游戏历史记忆链已完全清空重置。");
    }

    void LoadPrompt()
    {
        string promptPath = Path.Combine(Application.streamingAssetsPath, "prompt_v1.txt");
        if (File.Exists(promptPath))
        {
            systemPrompt = File.ReadAllText(promptPath, Encoding.UTF8);
            Debug.Log("[APIManager] Prompt loaded.");
        }
        else
        {
            Debug.LogError("[APIManager] prompt_v1.txt not found: " + promptPath);
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
        // Guard: refuse to send if no key is configured.
        if (string.IsNullOrEmpty(ActiveKey))
        {
            Debug.LogError(
                "[APIManager] No API key found. Edit config.json at:\n" +
                GameConfig.ConfigPath);
            onComplete?.Invoke(null);
            yield break;
        }

        GameStateManager gs = GameStateManager.Instance;

        string formattedSystemPrompt = systemPrompt
            .Replace("{currentRound}", gs.currentRound.ToString())
            .Replace("{gold}",         gs.gold.ToString())
            .Replace("{popularity}",   gs.popularity.ToString())
            .Replace("{church}",       gs.church.ToString())
            .Replace("{military}",     gs.military.ToString())
            .Replace("{suspicion}",    gs.suspicion.ToString())
            .Replace("{currentNPC}",   npcName)
            .Replace("{surfaceRequest}", surfaceRequest)
            .Replace("{hiddenMotive}", hiddenMotive)
            .Replace("{currentTurn}",  currentTurn.ToString())
            .Replace("{maxTurns}",     maxTurns.ToString());

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
                userContentBuilder.AppendLine($"  * {summary}");
        }
        userContentBuilder.AppendLine("=====================================");
        userContentBuilder.AppendLine($"[CURRENT PLAYER INPUT]: \"{playerInput}\"");

        var requestBody = new
        {
            model    = ActiveModel,
            messages = new[]
            {
                new { role = "system", content = formattedSystemPrompt },
                new { role = "user",   content = userContentBuilder.ToString() }
            },
            max_tokens  = 600,
            temperature = 0.7
        };

        string jsonBody = JsonConvert.SerializeObject(requestBody);
        byte[] bodyRaw  = Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest request = new UnityWebRequest(ActiveUrl, "POST");
        request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type",  "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + ActiveKey);
        request.timeout = 30;

        yield return request.SendWebRequest();

        // Capture all data before Dispose invalidates the download handler.
        bool   succeeded    = request.result == UnityWebRequest.Result.Success;
        string responseText = request.downloadHandler.text;
        string requestError = request.error;
        request.Dispose();

        if (succeeded)
        {
            try
            {
                var response = JsonConvert.DeserializeObject<OpenAIResponse>(responseText);
                string content = response.choices[0].message.content;
                content = content.Replace("```json", "").Replace("```", "").Trim();
                Debug.Log("AI_REPLY:\n" + content);

                AIResponse aiResponse = JsonConvert.DeserializeObject<AIResponse>(content);
                if (aiResponse.triggerEvent == null)
                    aiResponse.triggerEvent = "none";

                if (!string.IsNullOrEmpty(aiResponse.historySummary))
                    historicalSummaries.Add($"[Round {gs.currentRound}] {aiResponse.historySummary}");

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
            Debug.LogError("API请求失败: " + requestError);
            Debug.LogError("响应: " + responseText);
            onComplete?.Invoke(null);
        }
    }
}

// ==================== 数据实体 ====================

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
    public int affinityChange;
    public string historySummary;
    public string triggerEvent = "none";
}

[System.Serializable]
public class OpenAIResponse { public Choice[] choices; }

[System.Serializable]
public class Choice { public Message message; }

[System.Serializable]
public class Message { public string content; }
