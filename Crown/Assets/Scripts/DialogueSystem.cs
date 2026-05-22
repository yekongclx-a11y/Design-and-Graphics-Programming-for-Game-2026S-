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
        // 【后期优化核心】：如果玩家是从头重新玩（Round 1），必须强行洗掉大模型的过往记忆，防止世界线污染
        if (roundIndex == 0 && APIManager.Instance != null)
        {
            APIManager.Instance.ClearGameMemory();
        }

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

        if (AudioManager.Instance != null)
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
            // 【后期优化】：即使是开场白，AI也可能会根据历史政治摘要产生好感度基础波动，直接更新
            UpdateNPCAffinity(currentNPC.npcName, response.affinityChange);

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

        // 【核心机制：动态分发、更新对应当前NPC的个人好感度（Affinity）矩阵】
        UpdateNPCAffinity(currentNPC.npcName, response.affinityChange);

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

    // 【新增后期核心函数：好感度路由转换矩阵】
    // 将大模型返回的动态变量映射更新到 GameStateManager 对应的长期持久化变量中
    private void UpdateNPCAffinity(string npcName, int changeValue)
    {
        if (changeValue == 0) return;

        GameStateManager gs = GameStateManager.Instance;
        if (gs == null) return;

        // 根据名称模糊匹配或精确判断，动态将好感涨跌克扣进对应的变量里
        string lowerName = npcName.ToLower();
        if (lowerName.Contains("minister"))
        {
            gs.affinityMinister = Mathf.Clamp(gs.affinityMinister + changeValue, 0, 100);
            Debug.Log($"[AffinitySystem] 大臣好感度变动 {changeValue}，当前总值: {gs.affinityMinister}");
        }
        else if (lowerName.Contains("general"))
        {
            gs.affinityGeneral = Mathf.Clamp(gs.affinityGeneral + changeValue, 0, 100);
            Debug.Log($"[AffinitySystem] 将军好感度变动 {changeValue}，当前总值: {gs.affinityGeneral}");
        }
        else if (lowerName.Contains("bishop"))
        {
            gs.affinityBishop = Mathf.Clamp(gs.affinityBishop + changeValue, 0, 100);
            Debug.Log($"[AffinitySystem] 主教好感度变动 {changeValue}，当前总值: {gs.affinityBishop}");
        }
        else if (lowerName.Contains("princess"))
        {
            gs.affinityPrincess = Mathf.Clamp(gs.affinityPrincess + changeValue, 0, 100);
            Debug.Log($"[AffinitySystem] 公主好感度变动 {changeValue}，当前总值: {gs.affinityPrincess}");
        }
        else if (lowerName.Contains("commoner") || lowerName.Contains("peasant"))
        {
            gs.affinityCommoner = Mathf.Clamp(gs.affinityCommoner + changeValue, 0, 100);
            Debug.Log($"[AffinitySystem] 平民好感度变动 {changeValue}，当前总值: {gs.affinityCommoner}");
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