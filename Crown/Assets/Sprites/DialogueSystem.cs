using UnityEngine;
using System.Collections.Generic;

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance;

    [System.Serializable]
    public class NPCData
    {
        public string npcName;
        public string surfaceRequest;
        public string hiddenMotive;
        public Sprite portrait;
    }

    [Header("NPC Roster")]
    public List<NPCData> npcRoster = new List<NPCData>();

    private NPCData currentNPC;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartRound(int roundIndex)
    {
        if (roundIndex >= npcRoster.Count)
        {
            Debug.LogError("Round index out of range.");
            return;
        }

        currentNPC = npcRoster[roundIndex];
        UIManager.Instance.SetNPCPortrait(currentNPC.portrait);
        UIManager.Instance.DisplayNPCResponse(
            currentNPC.npcName,
            "",
            "..."
        );
        UIManager.Instance.SetInputLocked(false);
        UIManager.Instance.ShowLoading(false);

        Debug.Log("Round started: " + currentNPC.npcName);
    }

    public void SubmitPlayerInput(string playerInput)
    {
        if (string.IsNullOrEmpty(playerInput))
        {
            Debug.LogWarning("Player input is empty.");
            return;
        }

        if (GameStateManager.Instance.gameOver)
        {
            Debug.LogWarning("Game is over.");
            return;
        }

        UIManager.Instance.SetInputLocked(true);
        UIManager.Instance.ShowLoading(true);

        APIManager.Instance.SendMessage(
            currentNPC.npcName,
            currentNPC.surfaceRequest,
            currentNPC.hiddenMotive,
            playerInput,
            OnAPIResponse
        );
    }

    void OnAPIResponse(AIResponse response)
    {
        UIManager.Instance.ShowLoading(false);

        if (response == null)
        {
            UIManager.Instance.DisplayNPCResponse(
                currentNPC.npcName,
                "...",
                "Something is wrong. The court falls silent."
            );
            UIManager.Instance.SetInputLocked(false);
            return;
        }

        GameStateManager gs = GameStateManager.Instance;

        // 硬性锁死：疑心值超过80
        if (gs.suspicion >= 80)
        {
            UIManager.Instance.ShowUncleOverride(
                "You have nothing left to say, child. The game is over."
            );
            gs.gameOver = true;
            return;
        }
        // 概率性盖声：疑心值50-80之间
        else if (gs.suspicion > 50 && Random.Range(0, 100) < (gs.suspicion - 50) * 2)
        {
            UIManager.Instance.ShowUncleOverride(
                "His Majesty seems fatigued. Allow me to respond on his behalf."
            );
            gs.UpdateResources(-5, -5, -5, -5, 5);
        }
        else
        {
            UIManager.Instance.DisplayNPCResponse(
                currentNPC.npcName,
                response.action,
                response.dialogue
            );
            gs.UpdateResources(
                response.gold,
                response.popularity,
                response.church,
                response.military,
                response.suspicion
            );
        }

        UIManager.Instance.UpdateResourceBars();

        if (!gs.gameOver)
        {
            gs.NextRound();
            UIManager.Instance.SetInputLocked(false);
        }
    }
}