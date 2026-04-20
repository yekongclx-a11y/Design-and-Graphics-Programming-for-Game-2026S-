using UnityEngine;

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

    public void UpdateResources(int goldChange, int popularityChange, 
                                 int churchChange, int militaryChange, 
                                 int suspicionChange)
    {
        gold = Mathf.Clamp(gold + goldChange, 0, 100);
        popularity = Mathf.Clamp(popularity + popularityChange, 0, 100);
        church = Mathf.Clamp(church + churchChange, 0, 100);
        military = Mathf.Clamp(military + militaryChange, 0, 100);
        suspicion = Mathf.Clamp(suspicion + suspicionChange, 0, 100);

        CheckDeathConditions();
    }

    void CheckDeathConditions()
    {
        if (gold <= 0) TriggerEnding("Treasury collapsed. The kingdom starves.");
        else if (gold >= 100) TriggerEnding("Your wealth threatens the Regent. Tonight, you do not wake.");
        else if (popularity <= 0) TriggerEnding("The people have had enough. The mob breaches the gates.");
        else if (popularity >= 100) TriggerEnding("You are too loved. The Regent cannot allow a king the people would die for.");
        else if (church <= 0) TriggerEnding("The Church declares you heretic. No king survives that.");
        else if (church >= 100) TriggerEnding("The Church owns you now. You are no longer a king — you are a puppet.");
        else if (military <= 0) TriggerEnding("The army dissolves. Without swords, a crown is just metal.");
        else if (military >= 100) TriggerEnding("The General bows to no one now. Not even you.");
        else if (suspicion >= 100) TriggerCoup();
    }

    void TriggerEnding(string message)
    {
        gameOver = true;
        Debug.Log("ENDING: " + message);
        // 后续接EndingManager
    }

    void TriggerCoup()
    {
        gameOver = true;
        Debug.Log("COUP: The Regent moves.");
        // 后续接EndingManager
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

    void CheckVictory()
    {
        if (gold > 20 && gold < 80 &&
            popularity > 20 && popularity < 80 &&
            church > 20 && church < 80 &&
            military > 20 && military < 80 &&
            suspicion < 50)
        {
            Debug.Log("VICTORY: You survived.");
        }
        else
        {
            TriggerEnding("You reached the end, but the damage was done.");
        }
    }
}