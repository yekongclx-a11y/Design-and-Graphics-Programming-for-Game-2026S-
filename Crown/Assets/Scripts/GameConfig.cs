using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

[Serializable]
public class GameConfigData
{
    public string apiUrl = "";
    public string apiKey = "";
    public string model  = "";
}

public class GameConfig : MonoBehaviour
{
    public static GameConfig Instance { get; private set; }
    public GameConfigData Config { get; private set; } = new GameConfigData();

    // In standalone: [GameFolder]/config.json, right next to the .exe.
    // In Editor:     [ProjectRoot]/config.json  (add to .gitignore).
    public static string ConfigPath =>
        Path.GetFullPath(Path.Combine(Application.dataPath, "../config.json"));

    // Auto-creates itself before any scene loads — no scene setup needed.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInit()
    {
        if (Instance != null) return;
        var go = new GameObject("[GameConfig]");
        go.AddComponent<GameConfig>();
        DontDestroyOnLoad(go);
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public void Load()
    {
        if (!File.Exists(ConfigPath))
        {
            WriteTemplate();
            Debug.LogWarning(
                "[GameConfig] config.json not found — a template has been created.\n" +
                "Fill in your API key at:\n" + ConfigPath);
            return;
        }

        try
        {
            string raw = File.ReadAllText(ConfigPath);
            GameConfigData parsed = JsonConvert.DeserializeObject<GameConfigData>(raw);
            if (parsed != null) Config = parsed;

            if (string.IsNullOrEmpty(Config.apiKey))
                Debug.LogWarning("[GameConfig] apiKey is empty. Edit config.json at:\n" + ConfigPath);
            else
                Debug.Log($"[GameConfig] Loaded — url: {Config.apiUrl} | model: {Config.model}");
        }
        catch (Exception e)
        {
            Debug.LogError("[GameConfig] Failed to parse config.json: " + e.Message);
        }
    }

    // Saves all three fields to config.json and updates the in-memory Config immediately.
    // Called by FirstRunSetup after the player fills in the in-game configuration panel.
    public void SaveAll(string url, string key, string model)
    {
        Config.apiUrl = url.Trim();
        Config.apiKey = key.Trim();
        Config.model  = model.Trim();
        try
        {
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Config, Formatting.Indented));
            Debug.Log($"[GameConfig] Saved — url:{Config.apiUrl} model:{Config.model}");
        }
        catch (Exception e)
        {
            Debug.LogError("[GameConfig] Save failed: " + e.Message);
        }
    }

    // Writes a template with embedded usage instructions.
    // Unknown _-prefixed fields are silently ignored on deserialization.
    static void WriteTemplate()
    {
        const string template =
            "{\n" +
            "  \"_readme\": \"Edit this file in any text editor. Save, then restart the game.\",\n" +
            "  \"_howToGetKey\": \"Gemini key: https://aistudio.google.com/app/apikey  |  OpenAI key: https://platform.openai.com/api-keys\",\n" +
            "  \"_examples\": {\n" +
            "    \"gemini\": \"apiUrl=https://generativelanguage.googleapis.com/v1beta/openai/chat/completions  model=gemini-2.0-flash\",\n" +
            "    \"openai\": \"apiUrl=https://api.openai.com/v1/chat/completions  model=gpt-4o-mini\",\n" +
            "    \"custom\": \"apiUrl=https://your-proxy-or-local-endpoint/v1/chat/completions  model=your-model-name\"\n" +
            "  },\n" +
            "  \"apiUrl\": \"https://generativelanguage.googleapis.com/v1beta/openai/chat/completions\",\n" +
            "  \"apiKey\": \"\",\n" +
            "  \"model\" : \"gemini-2.0-flash\"\n" +
            "}\n";

        try   { File.WriteAllText(ConfigPath, template); }
        catch (Exception e) { Debug.LogError("[GameConfig] Could not write template: " + e.Message); }
    }
}
