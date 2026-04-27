using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    [Header("Resources")]
    public int gold = 50;
    public int popularity = 50;
    public int church = 50;
    public int military = 50;

    [Header("Uncle")]
    public int suspicion = 0;

    [Header("Game Progress")]
    public int currentRound = 1;
    public int maxRounds = 12;
    public bool gameOver = false;

    [Header("NPC Affinity")]
    public int affinityMinister = 50;
    public int affinityGeneral = 50;
    public int affinityBishop = 50;
    public int affinityPrincess = 50;
    public int affinityCommoner = 50;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetGame()
    {
        gold = 50;
        popularity = 50;
        church = 50;
        military = 50;
        suspicion = 0;
        currentRound = 1;
        gameOver = false;
        affinityMinister = 50;
        affinityGeneral = 50;
        affinityBishop = 50;
        affinityPrincess = 50;
        affinityCommoner = 50;
    }

    public void UpdateResources(int goldChange, int popularityChange,
                                 int churchChange, int militaryChange,
                                 int suspicionChange)
    {
        gold = Mathf.Clamp(gold + goldChange, 0, 100);
        popularity = Mathf.Clamp(popularity + popularityChange, 0, 100);
        church = Mathf.Clamp(church + churchChange, 0, 100);
        military = Mathf.Clamp(military + militaryChange, 0, 100);
        suspicion = Mathf.Clamp(suspicion + suspicionChange, 0, 100);

        int totalChange = goldChange + popularityChange + churchChange + militaryChange;
        if (totalChange > 0) AudioManager.Instance.PlayValueUp();
        else if (totalChange < 0) AudioManager.Instance.PlayValueDown();

        CheckDeathConditions();
    }

    void CheckDeathConditions()
    {
        if (gameOver) return;
        if (gold <= 0) TriggerEnding("unpaid_guard");
        else if (gold >= 100) TriggerEnding("golden_target");
        else if (popularity <= 0) TriggerEnding("mob_verdict");
        else if (popularity >= 100) TriggerEnding("poisoned_cup");
        else if (church <= 0) TriggerEnding("heretic_pyre");
        else if (church >= 100) TriggerEnding("living_saint");
        else if (military <= 0) TriggerEnding("fallen_gates");
        else if (military >= 100) TriggerEnding("generals_crown");
        else if (suspicion >= 100) TriggerCoup();
    }

    void TriggerEnding(string endingId)
    {
        if (gameOver) return;
        gameOver = true;
        AudioManager.Instance.PlayGameOver();
        Debug.Log("ENDING: " + endingId);
        PlayerPrefs.SetString("EndingType", endingId);
        SceneManager.LoadScene("EndingScene");
    }

    void TriggerCoup()
    {
        if (gameOver) return;
        gameOver = true;
        AudioManager.Instance.PlayGameOver();
        Debug.Log("COUP: The Regent moves.");
        PlayerPrefs.SetString("EndingType", "the_tower");
        SceneManager.LoadScene("EndingScene");
    }

    public void NextRound()
    {
        if (currentRound >= maxRounds)
        {
            CheckVictory();
            return;
        }
        currentRound++;
    }

    public void CheckVictory()
    {
        if (gold > 20 && gold < 80 &&
            popularity > 20 && popularity < 80 &&
            church > 20 && church < 80 &&
            military > 20 && military < 80 &&
            suspicion < 50)
        {
            PlayerPrefs.SetString("EndingType", "true_coronation");
            SceneManager.LoadScene("EndingScene");
        }
        else
        {
            TriggerEnding("last_word");
        }
    }
}