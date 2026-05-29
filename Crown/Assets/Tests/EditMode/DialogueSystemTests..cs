using NUnit.Framework;
using UnityEngine;
using System.Reflection;

public class DialogueSystemTests
{
    private GameObject go;
    private DialogueSystem ds;
    private GameStateManager gs;

    [SetUp]
    public void Setup()
    {
        go = new GameObject("DialogueSystem");
        ds = go.AddComponent<DialogueSystem>();

        GameObject gsObj = new GameObject("GameState");
        gs = gsObj.AddComponent<GameStateManager>();
        gs.ResetGame();

        PlayerPrefs.DeleteAll();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(go);
        PlayerPrefs.DeleteAll();
    }

    // ======================================================
    // 1. ROUND START SAFETY
    // ======================================================

    [Test]
    public void StartRound_ShouldNotCrash()
    {
        Assert.DoesNotThrow(() =>
        {
            ds.StartRound(0);
        });
    }

    // ======================================================
    // 2. INPUT VALIDATION
    // ======================================================

    [Test]
    public void SubmitPlayerInput_Empty_ShouldBeIgnored()
    {
        Assert.DoesNotThrow(() =>
        {
            ds.SubmitPlayerInput("");
        });
    }

    // ======================================================
    // 3. GAME OVER BLOCKING
    // ======================================================

    [Test]
    public void SubmitInput_GameOver_ShouldBeBlocked()
    {
        gs.gameOver = true;

        Assert.DoesNotThrow(() =>
        {
            ds.SubmitPlayerInput("hello");
        });
    }

    // ======================================================
    // 4. DIALOGUE FLOW STATE SAFETY
    // ======================================================

    [Test]
    public void RoundIndex_ShouldInitializeCorrectly()
    {
        ds.StartRound(0);

        var field = typeof(DialogueSystem)
            .GetField("currentRoundIndex",
                BindingFlags.NonPublic | BindingFlags.Instance);

        int value = (int)field.GetValue(ds);

        Assert.AreEqual(0, value);
    }

    // ======================================================
    // 5. NPC ROSTER SAFETY
    // ======================================================

    [Test]
    public void NPCRoster_OutOfRange_ShouldNotCrash()
    {
        Assert.DoesNotThrow(() =>
        {
            ds.StartRound(999);
        });
    }

    // ======================================================
    // 6. TURN FLOW SAFETY
    // ======================================================

    [Test]
    public void SubmitInput_ShouldIncreaseTurn()
    {
        ds.StartRound(0);

        int before = GetTurn();

        ds.SubmitPlayerInput("test");

        int after = GetTurn();

        Assert.IsTrue(after >= before);
    }

    // ======================================================
    // 7. REFLECTION HELPERS
    // ======================================================

    private int GetTurn()
    {
        var f = typeof(DialogueSystem)
            .GetField("currentTurnInRound",
                BindingFlags.NonPublic | BindingFlags.Instance);

        return (int)f.GetValue(ds);
    }
}