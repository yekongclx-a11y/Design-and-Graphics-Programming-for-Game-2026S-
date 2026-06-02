using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance;

    [System.Serializable]
    public class NPCData
    {
        public string npcName;       // NPC 识别标识
        public string titleName;     // UI 展现的阶级/头衔
        public string surfaceRequest;
        public string hiddenMotive;
        public Sprite portrait;
        public int maxTurns = 3;     // 单轮对话最大回合上限
    }

    [Header("NPC Roster Configuration")]
    public List<NPCData> npcRoster = new List<NPCData>();

    [Header("Scene Transition Configuration")]
    [SerializeField] private string endingSceneName = "EndingScene"; // 目标结算场景名称

    private NPCData currentNPC;
    private int currentRoundIndex = 0;
    private int currentTurnInRound = 1;
    private bool isFirstTurn = true;
    private bool isTransitioningRound = false; // 场景/回合状态机切换锁

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartRound(int roundIndex)
    {
        // 游戏首次启动或重置时，清理远端大模型历史上下文
        if (roundIndex == 0 && APIManager.Instance != null)
        {
            APIManager.Instance.ClearGameMemory();
        }
        StartRoundInternal(roundIndex);
    }

    private void StartRoundInternal(int roundIndex)
    {
        if (roundIndex >= npcRoster.Count)
        {
            Debug.LogError($"[DialogueSystem] Round index {roundIndex} out of range.");
            return;
        }

        currentRoundIndex = roundIndex;
        currentTurnInRound = 1;
        isFirstTurn = true;
        currentNPC = npcRoster[roundIndex];
        isTransitioningRound = false;

        if (currentNPC.portrait != null)
            UIManager.Instance.SetNPCPortrait(currentNPC.portrait);

        // 初始化输入框状态，杜绝测试占位文本延迟显示问题
        if (UIManager.Instance.playerInput != null)
        {
            UIManager.Instance.playerInput.text = string.Empty;
        }
        UIManager.Instance.SetInputLocked(true);
        UIManager.Instance.ShowLoading(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayNpcEnter();

        // 注册当前在场 NPC 数据至鼠标指针悬停交互拦截器
        NPCHoverHandler hoverHandler = FindObjectOfType<NPCHoverHandler>();
        if (hoverHandler != null) 
        {
            hoverHandler.SetCurrentNPC(currentNPC.npcName, currentNPC.titleName);
        }

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
        if (UIManager.Instance.playerInput != null) 
        {
            UIManager.Instance.playerInput.text = string.Empty;
        }

        if (response == null)
        {
            UIManager.Instance.DisplayNPCResponse(currentNPC.npcName, "", currentNPC.surfaceRequest);
        }
        else
        {
            UpdateNPCAffinity(currentNPC.npcName, response.affinityChange);
            UIManager.Instance.DisplayNPCResponse(currentNPC.npcName, response.action, response.dialogue);
        }
        UIManager.Instance.SetInputLocked(false);
    }

    public void SubmitPlayerInput(string playerInput)
    {
        if (isTransitioningRound || GameStateManager.Instance.gameOver) return;
        if (string.IsNullOrEmpty(playerInput)) return;

        UIManager.Instance.SetInputLocked(true);
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

    void OnAPIResponse(AIResponse response)
    {
        UIManager.Instance.ShowLoading(false);

        // 异步数据回调成功，即刻清空输入栏以防残留字符显示闪烁
        if (UIManager.Instance.playerInput != null)
        {
            UIManager.Instance.playerInput.text = string.Empty;
        }

        if (response == null)
        {
            UIManager.Instance.DisplayNPCResponse(currentNPC.npcName, "...", "Connection lost.");
            UIManager.Instance.SetInputLocked(false);
            return;
        }

        // 防止场景切换或已有协程运行期间重复响应
        if (isTransitioningRound || GameStateManager.Instance.gameOver) return;

        GameStateManager gs = GameStateManager.Instance;
        UpdateNPCAffinity(currentNPC.npcName, response.affinityChange);

        // ─────────────────────────────────────────────────────────
        // 核心渲染总线：优先渲染当前在场 NPC 的最终动作及文本，防止状态强切导致的内容吞噬
        // ─────────────────────────────────────────────────────────
        UIManager.Instance.DisplayNPCResponse(currentNPC.npcName, response.action, response.dialogue);
        int cappedSuspicion = Mathf.Clamp(response.suspicion, -8, 8);
        gs.UpdateResources(response.gold, response.popularity, response.church, response.military, cappedSuspicion);
        UIManager.Instance.UpdateResourceBars();

        // ─────────────────────────────────────────────────────────
        // 状态判定总线：将时序延迟推迟至打字机完结与玩家确认点击之后
        // ─────────────────────────────────────────────────────────
        
        // 1. 远端大模型显式触发的事件分支检测
        if (response.triggerEvent == "coup_attempt")
        {
            StartCoroutine(DelayedGameOverRoutine("coup_attempt"));
            return;
        }
        if (response.triggerEvent == "game_over")
        {
            StartCoroutine(DelayedGameOverRoutine("last_word"));
            return;
        }
        if (response.triggerEvent == "uncle_intervene")
        {
            StartCoroutine(DelayedUncleInterveneRoutine());
            return;
        }
        if (response.triggerEvent == "end_round")
        {
            StartCoroutine(DelayedEndRoundRoutine());
            return;
        }

        // 2. 本地数据边界拦截（数值爆表级强行触发危机）
        if (gs.suspicion > 65 && Random.Range(0, 100) < (gs.suspicion - 65) * 2)
        {
            StartCoroutine(DelayedRandomUncleInterveneRoutine());
            return;
        }

        // 3. 常规小回合周期迭代检查
        if (currentTurnInRound > currentNPC.maxTurns)
        {
            StartCoroutine(DelayedEndRoundRoutine());
        }
        else
        {
            if (!gs.gameOver) UIManager.Instance.SetInputLocked(false);
        }
    }

    // 💡 异步管线：处理远端显式干预事件。NPC 嘲讽文本播放完毕并经玩家确认点击后，强切危机角色进场
    IEnumerator DelayedUncleInterveneRoutine()
    {
        isTransitioningRound = true;
        UIManager.Instance.SetInputLocked(true);

        while (UIManager.Instance.isTyping) yield return null; 

        bool confirmedLeft = false;
        while (!confirmedLeft)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) confirmedLeft = true;
            yield return null;
        }

        // 执行本地覆盖渲染，展现截杀危机
        UIManager.Instance.ShowUncleOverride("Enough of this madness, child! Your words border on heresy. Guards, lead His Majesty back to his chambers—I shall handle the Court myself.");
        while (UIManager.Instance.isTyping) yield return null;

        bool confirmedRight = false;
        while (!confirmedRight)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) confirmedRight = true;
            yield return null;
        }

        isTransitioningRound = false;
        EndRound(); 
    }

    // 💡 异步管线：处理本地数值（怀疑度 >50）随机概率截杀
    IEnumerator DelayedRandomUncleInterveneRoutine()
    {
        isTransitioningRound = true;
        UIManager.Instance.SetInputLocked(true);

        while (UIManager.Instance.isTyping) yield return null; 

        bool confirmedLeft = false;
        while (!confirmedLeft)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) confirmedLeft = true;
            yield return null;
        }

        UIManager.Instance.ShowUncleOverride("His Majesty seems fatigued from the long court session. Allow me to respond on his behalf and dismiss the chamber.");
        GameStateManager.Instance.UpdateResources(-5, -5, -5, -5, 0);
        UIManager.Instance.UpdateResourceBars();
        
        while (UIManager.Instance.isTyping) yield return null;

        bool confirmedRight = false;
        while (!confirmedRight)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) confirmedRight = true;
            yield return null;
        }

        isTransitioningRound = false;
        EndRound();
    }

    // 💡 异步管线：本地硬上限死局。当前 NPC 吐出遗言，点击后切出篡位剧情，再次点击安全切换至软禁塔结局
    IEnumerator DelayedSuspicionGameOverRoutine()
    {
        isTransitioningRound = true;
        UIManager.Instance.SetInputLocked(true);

        while (UIManager.Instance.isTyping) yield return null; 

        bool confirmedLeft = false;
        while (!confirmedLeft)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) confirmedLeft = true;
            yield return null;
        }

        UIManager.Instance.ShowUncleOverride("You have nothing left to say, child. Your actions have betrayed the crown. The game is over.");
        while (UIManager.Instance.isTyping) yield return null;

        bool confirmedRight = false;
        while (!confirmedRight)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) confirmedRight = true;
            yield return null;
        }

        GameStateManager.Instance.gameOver = true;
        
        // 🌟 核心同步锁：持久化存储对接统一终结标识 "the_tower"
        PlayerPrefs.SetString("EndingType", "the_tower");
        PlayerPrefs.Save();
        SceneManager.LoadScene(endingSceneName);
    }

    // 💡 异步管线：常规小回合结束时的用户点击确认控制
    IEnumerator DelayedEndRoundRoutine()
    {
        isTransitioningRound = true;
        UIManager.Instance.SetInputLocked(true);

        while (UIManager.Instance.isTyping) yield return null; 

        bool confirmed = false;
        while (!confirmed)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) confirmed = true;
            yield return null;
        }

        isTransitioningRound = false;
        EndRound(); 
    }

    // 💡 异步管线：常规大模型远端主动中断引发的游戏终止结算跳转
    IEnumerator DelayedGameOverRoutine(string endingKey)
    {
        isTransitioningRound = true;
        UIManager.Instance.SetInputLocked(true);

        while (UIManager.Instance.isTyping) yield return null; 

        bool confirmed = false;
        while (!confirmed)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) confirmed = true;
            yield return null;
        }

        GameStateManager.Instance.gameOver = true;
        
        PlayerPrefs.SetString("EndingType", endingKey);
        PlayerPrefs.Save();
        SceneManager.LoadScene(endingSceneName); 
    }

    private void UpdateNPCAffinity(string npcName, int changeValue)
    {
        if (changeValue == 0) return;
        GameStateManager gs = GameStateManager.Instance;
        string lowerName = npcName.ToLower();
        if (lowerName.Contains("minister")) gs.affinityMinister = Mathf.Clamp(gs.affinityMinister + changeValue, 0, 100);
        else if (lowerName.Contains("general")) gs.affinityGeneral = Mathf.Clamp(gs.affinityGeneral + changeValue, 0, 100);
        else if (lowerName.Contains("bishop")) gs.affinityBishop = Mathf.Clamp(gs.affinityBishop + changeValue, 0, 100);
        else if (lowerName.Contains("princess")) gs.affinityPrincess = Mathf.Clamp(gs.affinityPrincess + changeValue, 0, 100);
        else if (lowerName.Contains("commoner")) gs.affinityCommoner = Mathf.Clamp(gs.affinityCommoner + changeValue, 0, 100);
    }
// 使用 Switch 表达式或精简的逻辑，提升可读性
    void EndRound()
    {
        currentTurnInRound = 1;
        isFirstTurn = true;
        UIManager.Instance.SetInputLocked(true);

        if (GameStateManager.Instance.gameOver) return;

        // 每轮结束后suspicion自然衰减，给玩家喘息空间
        GameStateManager gs = GameStateManager.Instance;
        gs.suspicion = Mathf.Clamp(gs.suspicion - 3, 0, 100);
        UIManager.Instance.UpdateResourceBars();

        int nextIndex = currentRoundIndex + 1;
        bool eventTriggered = EventManager.Instance.TryTriggerEvent(
            GameStateManager.Instance.currentRound,
            () => ProceedToNextRound(nextIndex)
        );

        if (!eventTriggered) ProceedToNextRound(nextIndex);
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