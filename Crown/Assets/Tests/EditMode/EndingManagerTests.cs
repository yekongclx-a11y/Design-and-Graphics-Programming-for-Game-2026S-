using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using System.Collections;

public class EndingManagerTests
{
    [SetUp]
    public void Setup()
    {
        PlayerPrefs.DeleteAll();
    }

    [Test]
    public void EndingManager_LoadsCorrectEnding_Default()
    {
        PlayerPrefs.SetString("EndingType", "last_word");

        GameObject go = new GameObject();
        EndingManager em = go.AddComponent<EndingManager>();

        Assert.DoesNotThrow(() =>
        {
            em.SendMessage("Start");
        });
    }

    [Test]
    public void EndingManager_CanHandleAllEndingIds()
    {
        string[] endings =
        {
            "unpaid_guard",
            "mob_verdict",
            "heretic_pyre",
            "fallen_gates",
            "golden_target",
            "poisoned_cup",
            "living_saint",
            "generals_crown",
            "the_tower",
            "last_word",
            "true_coronation"
        };

        foreach (var id in endings)
        {
            PlayerPrefs.SetString("EndingType", id);

            GameObject go = new GameObject();
            EndingManager em = go.AddComponent<EndingManager>();

            Assert.DoesNotThrow(() =>
            {
                em.SendMessage("Start");
            });

            Object.DestroyImmediate(go);
        }
    }
}