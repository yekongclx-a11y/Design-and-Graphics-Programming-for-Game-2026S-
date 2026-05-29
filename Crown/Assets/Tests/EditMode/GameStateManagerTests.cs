using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManagerTests
{
    private GameObject go;
    private GameStateManager gs;

    [SetUp]
    public void Setup()
    {
        go = new GameObject("GameStateManager");
        gs = go.AddComponent<GameStateManager>();
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
    // 1. INITIAL STATE
    // ======================================================

    [Test]
    public void ResetGame_ShouldRestoreDefaults()
    {
        Assert.AreEqual(50, gs.gold);
        Assert.AreEqual(50, gs.popularity);
        Assert.AreEqual(50, gs.church);
        Assert.AreEqual(50, gs.military);

        Assert.AreEqual(0, gs.suspicion);
        Assert.AreEqual(1, gs.currentRound);
        Assert.IsFalse(gs.gameOver);
    }

    // ======================================================
    // 2. RESOURCE CLAMPING (CRITICAL SYSTEM)
    // ======================================================

    [Test]
    public void UpdateResources_ShouldClamp_MinBoundary()
    {
        gs.UpdateResources(-999, -999, -999, -999, -999);

        Assert.AreEqual(0, gs.gold);
        Assert.AreEqual(0, gs.popularity);
        Assert.AreEqual(0, gs.church);
        Assert.AreEqual(0, gs.military);
        Assert.AreEqual(0, gs.suspicion);
    }

    [Test]
    public void UpdateResources_ShouldClamp_MaxBoundary()
    {
        gs.UpdateResources(999, 999, 999, 999, 999);

        Assert.AreEqual(100, gs.gold);
        Assert.AreEqual(100, gs.popularity);
        Assert.AreEqual(100, gs.church);
        Assert.AreEqual(100, gs.military);
        Assert.AreEqual(100, gs.suspicion);
    }

    [Test]
    public void UpdateResources_ShouldClamp_MixedInput()
    {
        gs.gold = 50;
        gs.UpdateResources(60, -200, 30, -400, 120);

        Assert.IsTrue(gs.gold <= 100 && gs.gold >= 0);
        Assert.IsTrue(gs.popularity <= 100 && gs.popularity >= 0);
        Assert.IsTrue(gs.church <= 100 && gs.church >= 0);
        Assert.IsTrue(gs.military <= 100 && gs.military >= 0);
        Assert.IsTrue(gs.suspicion <= 100 && gs.suspicion >= 0);
    }

    // ======================================================
    // 3. ENDING TRIGGERS (DEATH CONDITIONS)
    // ======================================================

    [Test]
    public void GoldZero_ShouldTrigger_UnpaidGuardEnding()
    {
        gs.gold = 1;

        gs.UpdateResources(-10, 0, 0, 0, 0);

        Assert.IsTrue(gs.gameOver);
        Assert.AreEqual("unpaid_guard", PlayerPrefs.GetString("EndingType"));
    }

    [Test]
    public void GoldHundred_ShouldTrigger_GoldenTargetEnding()
    {
        gs.gold = 95;

        gs.UpdateResources(10, 0, 0, 0, 0);

        Assert.IsTrue(gs.gameOver);
        Assert.AreEqual("golden_target", PlayerPrefs.GetString("EndingType"));
    }

    [Test]
    public void PopularityZero_ShouldTrigger_MobVerdict()
    {
        gs.popularity = 5;

        gs.UpdateResources(0, -10, 0, 0, 0);

        Assert.IsTrue(gs.gameOver);
        Assert.AreEqual("mob_verdict", PlayerPrefs.GetString("EndingType"));
    }

    [Test]
    public void PopularityHundred_ShouldTrigger_PoisonedCup()
    {
        gs.popularity = 95;

        gs.UpdateResources(0, 10, 0, 0, 0);

        Assert.IsTrue(gs.gameOver);
        Assert.AreEqual("poisoned_cup", PlayerPrefs.GetString("EndingType"));
    }

    [Test]
    public void ChurchZero_ShouldTrigger_HereticPyre()
    {
        gs.church = 1;

        gs.UpdateResources(0, 0, -10, 0, 0);

        Assert.IsTrue(gs.gameOver);
        Assert.AreEqual("heretic_pyre", PlayerPrefs.GetString("EndingType"));
    }

    [Test]
    public void MilitaryZero_ShouldTrigger_FallenGates()
    {
        gs.military = 1;

        gs.UpdateResources(0, 0, 0, -10, 0);

        Assert.IsTrue(gs.gameOver);
        Assert.AreEqual("fallen_gates", PlayerPrefs.GetString("EndingType"));
    }

    // ======================================================
    // 4. SUSPICION SYSTEM (COUP LOGIC)
    // ======================================================

    [Test]
    public void SuspicionMax_ShouldTrigger_TowerEnding()
    {
        gs.suspicion = 95;

        gs.UpdateResources(0, 0, 0, 0, 10);

        Assert.IsTrue(gs.gameOver);
        Assert.AreEqual("the_tower", PlayerPrefs.GetString("EndingType"));
    }

    // ======================================================
    // 5. ROUND SYSTEM
    // ======================================================

    [Test]
    public void NextRound_ShouldIncreaseRound()
    {
        int before = gs.currentRound;

        gs.NextRound();

        Assert.AreEqual(before + 1, gs.currentRound);
    }

    [Test]
    public void MaxRound_ShouldTriggerVictoryCheck()
    {
        gs.currentRound = gs.maxRounds;

        gs.gold = 50;
        gs.popularity = 50;
        gs.church = 50;
        gs.military = 50;
        gs.suspicion = 10;

        gs.NextRound();

        Assert.IsNotNull(PlayerPrefs.GetString("EndingType"));
    }

    // ======================================================
    // 6. VICTORY CONDITIONS
    // ======================================================

    [Test]
    public void BalancedState_ShouldTriggerTrueCoronation()
    {
        gs.gold = 60;
        gs.popularity = 60;
        gs.church = 60;
        gs.military = 60;
        gs.suspicion = 10;

        gs.CheckVictory();

        Assert.AreEqual("true_coronation", PlayerPrefs.GetString("EndingType"));
    }

    [Test]
    public void ExtremeState_ShouldFailVictory()
    {
        gs.gold = 95;
        gs.popularity = 50;
        gs.church = 50;
        gs.military = 50;

        gs.CheckVictory();

        Assert.AreEqual("last_word", PlayerPrefs.GetString("EndingType"));
    }

    [Test]
    public void HighSuspicion_ShouldFailVictory()
    {
        gs.gold = 60;
        gs.popularity = 60;
        gs.church = 60;
        gs.military = 60;
        gs.suspicion = 80;

        gs.CheckVictory();

        Assert.AreEqual("last_word", PlayerPrefs.GetString("EndingType"));
    }

    // ======================================================
    // 7. REGRESSION SAFETY
    // ======================================================

    [Test]
    public void GameOver_ShouldBeIdempotent()
    {
        gs.gold = 1;

        gs.UpdateResources(-10, 0, 0, 0, 0);

        string first = PlayerPrefs.GetString("EndingType");

        gs.UpdateResources(-10, -10, -10, -10, -10);

        string second = PlayerPrefs.GetString("EndingType");

        Assert.AreEqual(first, second);
    }

    [Test]
    public void NoStateChangeAfterGameOver()
    {
        gs.gold = 1;

        gs.UpdateResources(-10, 0, 0, 0, 0);

        int goldAfter = gs.gold;

        gs.UpdateResources(50, 50, 50, 50, 50);

        Assert.AreEqual(goldAfter, gs.gold);
    }

    // ======================================================
    // 8. STRESS TEST (ENGINE SAFETY)
    // ======================================================

    [Test]
    public void ResourceSystem_ShouldRemainStableUnderStress()
    {
        for (int i = 0; i < 5000; i++)
        {
            gs.UpdateResources(
                Random.Range(-10, 10),
                Random.Range(-10, 10),
                Random.Range(-10, 10),
                Random.Range(-10, 10),
                Random.Range(-5, 5)
            );
        }

        Assert.IsTrue(gs.gold >= 0 && gs.gold <= 100);
        Assert.IsTrue(gs.popularity >= 0 && gs.popularity <= 100);
        Assert.IsTrue(gs.church >= 0 && gs.church <= 100);
        Assert.IsTrue(gs.military >= 0 && gs.military <= 100);
        Assert.IsTrue(gs.suspicion >= 0 && gs.suspicion <= 100);
    }

}