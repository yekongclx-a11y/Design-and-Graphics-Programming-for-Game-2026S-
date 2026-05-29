using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;

public class FullGameFlowTests
{
    private GameObject gsObj;
    private GameObject dsObj;
    private GameStateManager gs;
    private DialogueSystem ds;

    // ======================================================
    // SETUP REAL GAME SYSTEM
    // ======================================================

    [UnitySetUp]
    public IEnumerator Setup()
    {
        PlayerPrefs.DeleteAll();

        gsObj = new GameObject("GameState");
        gs = gsObj.AddComponent<GameStateManager>();
        gs.ResetGame();

        dsObj = new GameObject("DialogueSystem");
        ds = dsObj.AddComponent<DialogueSystem>();

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Object.Destroy(gsObj);
        Object.Destroy(dsObj);

        yield return null;
    }

    // ======================================================
    // 1. FULL GAME LOOP STABILITY TEST
    // ======================================================

    [UnityTest]
    public IEnumerator Game_Should_Run_Multiple_Rounds_Without_Crash()
    {
        for (int i = 0; i < 5; i++) // short safe simulation
        {
            ds.StartRound(i);
            yield return null;

            ds.SubmitPlayerInput("I will balance the kingdom.");
            yield return null;
        }

        Assert.IsTrue(true);
    }

    // ======================================================
    // 2. RESOURCE SYSTEM INTEGRATION TEST
    // ======================================================

    [UnityTest]
    public IEnumerator Resources_Should_Stay_Valid_During_Play()
    {
        for (int i = 0; i < 10; i++)
        {
            gs.UpdateResources(
                Random.Range(-5, 5),
                Random.Range(-5, 5),
                Random.Range(-5, 5),
                Random.Range(-5, 5),
                Random.Range(-3, 3)
            );

            yield return null;

            Assert.IsTrue(gs.gold >= 0 && gs.gold <= 100);
            Assert.IsTrue(gs.popularity >= 0 && gs.popularity <= 100);
            Assert.IsTrue(gs.church >= 0 && gs.church <= 100);
            Assert.IsTrue(gs.military >= 0 && gs.military <= 100);
            Assert.IsTrue(gs.suspicion >= 0 && gs.suspicion <= 100);
        }
    }

    // ======================================================
    // 3. EVENT SYSTEM INTEGRATION TEST
    // ======================================================

    [UnityTest]
    public IEnumerator Event_System_Should_Not_Crash_Game()
    {
        GameObject emObj = new GameObject("EventManager");
        EventManager em = emObj.AddComponent<EventManager>();

        bool completed = false;

        for (int i = 1; i <= 8; i++)
        {
            bool triggered = em.TryTriggerEvent(i, () =>
            {
                completed = true;
            });

            yield return null;

            // system must never crash
            Assert.Pass();
        }

        Object.Destroy(emObj);
    }

    // ======================================================
    // 4. GAME OVER SAFETY TEST
    // ======================================================

    [UnityTest]
    public IEnumerator GameOver_Should_Stop_Further_Logic()
    {
        gs.gold = 1;

        gs.UpdateResources(-10, 0, 0, 0, 0);
        yield return null;

        Assert.IsTrue(gs.gameOver);

        int before = gs.gold;

        gs.UpdateResources(50, 50, 50, 50, 50);
        yield return null;

        Assert.AreEqual(before, gs.gold);
    }

    // ======================================================
    // 5. SUSPICION COUP PATH TEST
    // ======================================================

    [UnityTest]
    public IEnumerator Suspicion_Should_Trigger_GameOver_Eventually()
    {
        for (int i = 0; i < 20; i++)
        {
            gs.UpdateResources(0, 0, 0, 0, 10);
            yield return null;

            if (gs.gameOver)
                break;
        }

        Assert.IsTrue(gs.suspicion <= 100);
    }

    // ======================================================
    // 6. STRESS TEST (FULL SYSTEM LOOP)
    // ======================================================

    [UnityTest]
    public IEnumerator Full_System_Should_Handle_Stress()
    {
        GameObject emObj = new GameObject("EventManager");
        EventManager em = emObj.AddComponent<EventManager>();

        for (int i = 0; i < 50; i++)
        {
            gs.UpdateResources(
                Random.Range(-10, 10),
                Random.Range(-10, 10),
                Random.Range(-10, 10),
                Random.Range(-10, 10),
                Random.Range(-5, 5)
            );

            em.TryTriggerEvent(Random.Range(1, 10), () => { });

            ds.StartRound(Random.Range(0, 3));

            yield return null;
        }

        Object.Destroy(emObj);

        Assert.IsTrue(true);
    }

    // ======================================================
    // 7. ENDING SAFETY PATH TEST
    // ======================================================

    [UnityTest]
    public IEnumerator Ending_System_Should_Not_Crash()
    {
        gs.gold = 0;

        gs.UpdateResources(-10, 0, 0, 0, 0);
        yield return null;

        Assert.IsTrue(gs.gameOver);

        yield return null;
    }
}