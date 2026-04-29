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
        public int maxTurns = 3;
    }

    [Header("NPC Roster")]
    public List<NPCData> npcRoster = new List<NPCData>();

    private NPCData currentNPC;
    private int currentRoundIndex = 0;
    private int currentTurnInRound = 1;
    private bool isFirstTurn = true;

    void Awake()
    {
        Instance = this;
    }

    public void StartRound(int roundIndex)
    {
        // 随机事件在EndRound后触发，这里直接开始
        StartRoundInternal(roundIndex);
    }

    void StartRoundInternal(int roundIndex)
    {
        if (roundIndex >= npcRoster.Count)
        {
            Debug.LogError("Round index out of range.");
            return;
        }

        currentRoundIndex = roundIndex;
        currentTurnInRound = 1;
        isFirstTurn = true;
        currentNPC = npcRoster[roundIndex];

        if (currentNPC.portrait != null)
            UIManager.Instance.SetNPCPortrait(currentNPC.portrait);

        // 显示NPC名字，对话框显示等待状态
        UIManager.Instance.DisplayNPCResponse(currentNPC.npcName, "", "...");
        UIManager.Instance.SetInputLocked(true);
        UIManager.Instance.ShowLoading(true);
        UIManager.Instance.ShowDismissButton(false);

        AudioManager.Instance.PlayNpcEnter();

        Debug.Log($"Round {roundIndex + 1} started: {currentNPC.npcName}");

        // AI生成NPC开场白
        APIManager.Instance.SendMessage(
            currentNPC.npcName,
            currentNPC.surfaceRequest,
            currentNPC.hiddenMotive,
            "[SCENE_START]",
            0,
            currentNPC.maxTurns,
            false,
            OnOpeningResponse
        );
    }

    void OnOpeningResponse(AIResponse response)
    {
        UIManager.Instance.ShowLoading(false);

        if (response == null)
        {
            UIManager.Instance.DisplayNPCResponse(
                currentNPC.npcName,
                "",
                currentNPC.surfaceRequest
            );
        }
        else
        {
            UIManager.Instance.DisplayNPCResponse(
                currentNPC.npcName,
                response.action,
                response.dialogue
            );
        }

        // 开场白显示完毕，解锁输入框
        UIManager.Instance.SetInputLocked(false);
        UIManager.Instance.ShowDismissButton(true);
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
        UIManager.Instance.ShowDismissButton(false);
        UIManager.Instance.ShowLoading(true);

        APIManager.Instance.SendMessage(
            currentNPC.npcName,
            currentNPC.surfaceRequest,
            currentNPC.hiddenMotive,
            playerInput,
            currentTurnInRound,
            currentNPC.maxTurns,
            false,
            OnAPIResponse
        );

        currentTurnInRound++;
        isFirstTurn = false;
    }

    public void SubmitDismiss()
    {
        if (GameStateManager.Instance.gameOver) return;

        UIManager.Instance.SetInputLocked(true);
        UIManager.Instance.ShowDismissButton(false);
        UIManager.Instance.ShowLoading(true);

        string dismissInput = "The King says nothing and waves his hand in dismissal. Give your parting action, a final line, and settle the values.";

        APIManager.Instance.SendMessage(
            currentNPC.npcName,
            currentNPC.surfaceRequest,
            currentNPC.hiddenMotive,
            dismissInput,
            currentTurnInRound,
            currentNPC.maxTurns,
            true,
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
            UIManager.Instance.ShowDismissButton(true);
            return;
        }

        GameStateManager gs = GameStateManager.Instance;

        switch (response.triggerEvent)
        {
            case "coup_attempt":
                Debug.Log("TRIGGER: coup_attempt");
                gs.UpdateResources(0, 0, 0, 0, 20);
                UIManager.Instance.DisplayNPCResponse(
                    currentNPC.npcName,
                    response.action,
                    response.dialogue
                );
                UIManager.Instance.UpdateResourceBars();
                UIManager.Instance.SetInputLocked(false);
                UIManager.Instance.ShowDismissButton(true);
                return;

            case "game_over":
                Debug.Log("TRIGGER: game_over");
                UIManager.Instance.DisplayNPCResponse(
                    currentNPC.npcName,
                    response.action,
                    response.dialogue
                );
                gs.UpdateResources(response.gold, response.popularity,
                                   response.church, response.military,
                                   response.suspicion);
                UIManager.Instance.UpdateResourceBars();
                gs.gameOver = true;
                return;

            case "uncle_intervene":
                Debug.Log("TRIGGER: uncle_intervene");
                UIManager.Instance.ShowUncleOverride(
                    "His Majesty seems fatigued. Allow me to respond on his behalf."
                );
                gs.UpdateResources(-5, -5, -5, -5, 20);
                UIManager.Instance.UpdateResourceBars();
                EndRound();
                return;

            case "end_round":
                Debug.Log("TRIGGER: end_round");
                UIManager.Instance.DisplayNPCResponse(
                    currentNPC.npcName,
                    response.action,
                    response.dialogue
                );
                gs.UpdateResources(response.gold, response.popularity,
                                   response.church, response.military,
                                   response.suspicion);
                UIManager.Instance.UpdateResourceBars();
                EndRound();
                return;
        }

        if (gs.suspicion >= 80)
        {
            UIManager.Instance.ShowUncleOverride(
                "You have nothing left to say, child. The game is over."
            );
            gs.gameOver = true;
            return;
        }
        else if (gs.suspicion > 50 && Random.Range(0, 100) < (gs.suspicion - 50) * 2)
        {
            UIManager.Instance.ShowUncleOverride(
                "His Majesty seems fatigued. Allow me to respond on his behalf."
            );
            gs.UpdateResources(-5, -5, -5, -5, 5);
            UIManager.Instance.UpdateResourceBars();
            EndRound();
            return;
        }

        UIManager.Instance.DisplayNPCResponse(
            currentNPC.npcName,
            response.action,
            response.dialogue
        );
        gs.UpdateResources(response.gold, response.popularity,
                           response.church, response.military,
                           response.suspicion);
        UIManager.Instance.UpdateResourceBars();

        if (currentTurnInRound > currentNPC.maxTurns)
        {
            EndRound();
        }
        else
        {
            if (!gs.gameOver)
            {
                UIManager.Instance.SetInputLocked(false);
                UIManager.Instance.ShowDismissButton(true);
            }
        }
    }

    void EndRound()
    {
        currentTurnInRound = 1;
        isFirstTurn = true;
        UIManager.Instance.ShowDismissButton(false);
        UIManager.Instance.SetInputLocked(true);

        if (GameStateManager.Instance.gameOver) return;

        int nextIndex = currentRoundIndex + 1;

        // 随机事件在NPC离场后触发
        bool eventTriggered = EventManager.Instance.TryTriggerEvent(
            GameStateManager.Instance.currentRound,
            () => ProceedToNextRound(nextIndex)
        );

        if (!eventTriggered)
        {
            ProceedToNextRound(nextIndex);
        }
    }

    void ProceedToNextRound(int nextIndex)
    {
        if (nextIndex >= npcRoster.Count)
        {
            GameStateManager.Instance.CheckVictory();
            return;
        }

        GameStateManager.Instance.NextRound();
        StartRoundInternal(nextIndex);
    }
}