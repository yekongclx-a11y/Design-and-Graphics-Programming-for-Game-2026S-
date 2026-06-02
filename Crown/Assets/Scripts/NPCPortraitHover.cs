using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NPCHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [System.Serializable]
    public class CharacterProfile
    {
        public string backendId;       // 对应大模型的 minister, bishop, general 等识别码
        public string nameEN;          // 英文官方名称
        public string factionEN;       // 英文阵营名称
    }

    [Header("UI Component Bindings")]
    [SerializeField] private GameObject tooltipPanel; // 提示框根节点物体
    [SerializeField] private TextMeshProUGUI realNameText;
    [SerializeField] private TextMeshProUGUI factionText;
    [SerializeField] private TextMeshProUGUI relationshipText;

    [Header("Database Configuration (Auto-Populated)")]
    [SerializeField] private CharacterProfile[] characterDatabase;

    private string currentActiveNpcId = string.Empty;

    private void Awake()
    {
        // 🌟 运行时自动化：彻底锁死数据填充，无视 Inspector 面板上的任何干扰
        InitializeDatabase();
    }

    private void Reset()
    {
        // 🌟 编辑器自动化：在挂载或右键重置脚本时，自动在面板上刷出这 5 个格子
        InitializeDatabase();
    }

    /// <summary>
    /// 工业级硬编码：彻底免除人工录入的全自动人设配置
    /// </summary>
    private void InitializeDatabase()
    {
        characterDatabase = new CharacterProfile[5];
        characterDatabase[0] = new CharacterProfile { backendId = "minister", nameEN = "Count August von Wald", factionEN = "Old Aristocracy" };
        characterDatabase[1] = new CharacterProfile { backendId = "bishop", nameEN = "Cardinal Malakai Vane", factionEN = "Holy Order" };
        characterDatabase[2] = new CharacterProfile { backendId = "general", nameEN = "Commander Varian Draven", factionEN = "Iron Vanguard" };
        characterDatabase[3] = new CharacterProfile { backendId = "commoner", nameEN = "Thomas Miller", factionEN = "Sown Districts" };
        characterDatabase[4] = new CharacterProfile { backendId = "princess", nameEN = "Lady Elara von Rosenburg", factionEN = "Imperial Fiancée" };
    }

    void Start()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false); // 默认关闭状态栏
        }
    }

    /// <summary>
    /// 由 DialogueSystem 在关卡初始化或切人时显式调用，同步当前在场 NPC 的底层 ID
    /// </summary>
    public void SetCurrentNPC(string npcName, string titleName)
    {
        if (string.IsNullOrEmpty(npcName)) return;

        string lowerName = npcName.ToLower();
        if (lowerName.Contains("minister")) currentActiveNpcId = "minister";
        else if (lowerName.Contains("bishop")) currentActiveNpcId = "bishop";
        else if (lowerName.Contains("general")) currentActiveNpcId = "general";
        else if (lowerName.Contains("commoner") || lowerName.Contains("petitioner")) currentActiveNpcId = "commoner";
        else if (lowerName.Contains("princess")) currentActiveNpcId = "princess";
        else currentActiveNpcId = string.Empty;
    }

    /// <summary>
    /// 触发鼠标移入事件
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(currentActiveNpcId) || tooltipPanel == null) return;

        CharacterProfile profile = FindProfile(currentActiveNpcId);
        if (profile == null) return;

        RenderTooltip(profile);
        tooltipPanel.SetActive(true);
    }

    /// <summary>
    /// 触发鼠标移出事件
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    private CharacterProfile FindProfile(string id)
    {
        if (characterDatabase == null) return null;
        for (int i = 0; i < characterDatabase.Length; i++)
        {
            if (characterDatabase[i] != null && characterDatabase[i].backendId == id) return characterDatabase[i];
        }
        return null;
    }

    private void RenderTooltip(CharacterProfile profile)
    {
        GameStateManager gs = GameStateManager.Instance;
        int affinityValue = 50; // 默认中性数值兜底

        // 从全局状态机实时动态抓取隐藏好感度
        switch (profile.backendId)
        {
            case "minister": affinityValue = gs != null ? gs.affinityMinister : 50; break;
            case "bishop":   affinityValue = gs != null ? gs.affinityBishop : 50; break;
            case "general":  affinityValue = gs != null ? gs.affinityGeneral : 50; break;
            case "commoner": affinityValue = gs != null ? gs.affinityCommoner : 50; break;
            case "princess": affinityValue = gs != null ? gs.affinityPrincess : 50; break;
        }

        // 纯英原生 HUD 动态组装渲染
        realNameText.text = profile.nameEN;
        factionText.text = $"Faction: {profile.factionEN}";
        relationshipText.text = $"Stance: {EvaluateRelationshipEN(affinityValue)} ({affinityValue}/100)";
    }

    private string EvaluateRelationshipEN(int value)
    {
        if (value <= 20) return "<color=#FF0000>Antagonistic</color>";
        if (value <= 50) return "<color=#FFA500>Estranged</color>";
        if (value <= 80) return "<color=#FFFF00>Cooperative</color>";
        return "<color=#00FF00>Loyal</color>";
    }
}