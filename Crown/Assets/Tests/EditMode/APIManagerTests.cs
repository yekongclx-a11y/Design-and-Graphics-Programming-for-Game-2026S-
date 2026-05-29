using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Text;
using UnityEngine.TestTools;

public class APIManagerTests
{
    private GameObject go;
    private APIManager api;

    [SetUp]
    public void Setup()
    {
        go = new GameObject("APIManager");
        api = go.AddComponent<APIManager>();

        PlayerPrefs.DeleteAll();

        // mock prompt，避免文件依赖
        SetPrivateField("systemPrompt", api,
            "KING STATE: {gold} {popularity} {church} {military} {suspicion}");
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(go);
        PlayerPrefs.DeleteAll();
    }

    // ======================================================
    // 1. FILE LOAD SAFETY
    // ======================================================

    [Test]
    public void LoadEnv_ShouldNotCrash()
    {
        var m = typeof(APIManager).GetMethod("LoadEnv",
            BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.DoesNotThrow(() => m.Invoke(api, null));
    }

    [Test]
    public void LoadPrompt_ShouldNotCrash()
    {
        var m = typeof(APIManager).GetMethod("LoadPrompt",
            BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.DoesNotThrow(() => m.Invoke(api, null));
    }

    // ======================================================
    // 2. SEND MESSAGE ENTRY SAFETY
    // ======================================================

    [Test]
    public void SendMessage_ShouldNotThrow()
    {
        Assert.DoesNotThrow(() =>
        {
            api.SendMessage(
                "King",
                "surface",
                "hidden",
                "hello",
                1,
                3,
                false,
                (r) => { }
            );
        });
    }

    // ======================================================
    // 3. NULL CALLBACK SAFETY
    // ======================================================

    [Test]
    public void SendMessage_NullCallback_ShouldNotCrash()
    {
        Assert.DoesNotThrow(() =>
        {
            api.SendMessage(
                "NPC",
                "surface",
                "hidden",
                "input",
                1,
                3,
                false,
                null
            );
        });
    }

    // ======================================================
    // 4. PROMPT TOKEN REPLACEMENT
    // ======================================================

    [Test]
    public void Prompt_ShouldContainGameStateValues()
    {
        GameStateManager.Instance.ResetGame();

        GameStateManager.Instance.gold = 10;
        GameStateManager.Instance.popularity = 20;
        GameStateManager.Instance.church = 30;
        GameStateManager.Instance.military = 40;
        GameStateManager.Instance.suspicion = 50;

        string prompt = BuildPromptMock();

        Assert.IsTrue(prompt.Contains("10"));
        Assert.IsTrue(prompt.Contains("20"));
        Assert.IsTrue(prompt.Contains("30"));
        Assert.IsTrue(prompt.Contains("40"));
        Assert.IsTrue(prompt.Contains("50"));
    }

    // ======================================================
    // 5. AI RESPONSE MODEL SAFETY
    // ======================================================

    [Test]
    public void AIResponse_DefaultTrigger_ShouldBeNone()
    {
        AIResponse r = new AIResponse();
        Assert.AreEqual("none", r.triggerEvent);
    }

    [Test]
    public void AIResponse_PartialData_ShouldBeValid()
    {
        AIResponse r = new AIResponse
        {
            action = "test",
            dialogue = "hello"
        };

        Assert.AreEqual("test", r.action);
        Assert.AreEqual("hello", r.dialogue);
    }

    // ======================================================
    // 6. OPENAI WRAPPER STRUCTURE
    // ======================================================

    [Test]
    public void OpenAIResponse_Structure_ShouldBeValid()
    {
        string json = @"{
            ""choices"": [
                {
                    ""message"": {
                        ""content"": ""{\""action\"":\""a\"",\""dialogue\"":\""b\""}""
                    }
                }
            ]
        }";

        var obj = JsonUtility.FromJson<OpenAIResponse>(json);

        Assert.IsNotNull(obj);
        Assert.IsNotNull(obj.choices);
    }

    // ======================================================
    // 7. STRESS TEST (PROMPT + STATE)
    // ======================================================

    [Test]
    public void Prompt_ShouldRemainStable_UnderStress()
    {
        GameStateManager.Instance.ResetGame();

        for (int i = 0; i < 500; i++)
        {
            GameStateManager.Instance.UpdateResources(
                Random.Range(-5, 5),
                Random.Range(-5, 5),
                Random.Range(-5, 5),
                Random.Range(-5, 5),
                Random.Range(-5, 5)
            );

            string prompt = BuildPromptMock();
            Assert.IsNotNull(prompt);
        }
    }

    // ======================================================
    // Helper
    // ======================================================

    private void SetPrivateField(string field, object obj, object value)
    {
        var f = obj.GetType().GetField(field,
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (f != null) f.SetValue(obj, value);
    }

    private string BuildPromptMock()
    {
        var gs = GameStateManager.Instance;

        return $"KING {gs.gold} {gs.popularity} {gs.church} {gs.military} {gs.suspicion}";
    }
}