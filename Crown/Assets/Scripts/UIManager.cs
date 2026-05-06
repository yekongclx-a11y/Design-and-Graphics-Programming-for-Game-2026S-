using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Crown.UI;  // ← 新增：ResourceBarUI 在这个命名空间

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Resource Bars")]
    public ResourceBarUI goldBar;
    public ResourceBarUI popularityBar;
    public ResourceBarUI churchBar;
    public ResourceBarUI militaryBar;

    // ─────────────────────────────────────────────
    // ↑ 已删除：旧的 4 个 Slider 字段
    // ↑ 已删除：旧的 4 个 TextMeshProUGUI Label 字段
    // ─────────────────────────────────────────────

    [Header("Dialogue")]
    public TextMeshProUGUI npcNameText;
    public TextMeshProUGUI actionText;
    public TextMeshProUGUI dialogueText;

    [Header("Input")]
    public TMP_InputField playerInput;
    public Button sendButton;
    public Button dismissButton;

    [Header("NPC Portrait")]
    public Image npcPortrait;

    [Header("Special")]
    public Sprite regentPortrait;

    [Header("Round Info")]
    public TextMeshProUGUI roundText;

    [Header("Loading")]
    public GameObject loadingIndicator;

    void Awake()
    {
        Instance = this;
    }

    public void UpdateResourceBars()
    {
        GameStateManager gs = GameStateManager.Instance;

        if (goldBar)       goldBar.SetValue(gs.gold);
        if (popularityBar) popularityBar.SetValue(gs.popularity);
        if (churchBar)     churchBar.SetValue(gs.church);
        if (militaryBar)   militaryBar.SetValue(gs.military);

        if (roundText) roundText.text = "Round " + gs.currentRound + " / " + gs.maxRounds;
    }

    /// <summary>
    /// 重置游戏时调用。瞬切到目标值，无动画。
    /// </summary>
    public void ResetResourceBars()
    {
        GameStateManager gs = GameStateManager.Instance;

        if (goldBar)       goldBar.SetValueImmediate(gs.gold);
        if (popularityBar) popularityBar.SetValueImmediate(gs.popularity);
        if (churchBar)     churchBar.SetValueImmediate(gs.church);
        if (militaryBar)   militaryBar.SetValueImmediate(gs.military);

        if (roundText) roundText.text = "Round " + gs.currentRound + " / " + gs.maxRounds;
    }

    public void DisplayNPCResponse(string npcName, string action, string dialogue)
    {
        if (npcNameText) npcNameText.text = npcName;
        if (actionText) actionText.text = "(" + action + ")";
        if (dialogueText)
        {
            if (string.IsNullOrEmpty(action))
                dialogueText.text = dialogue;
            else
                dialogueText.text = "<i>(" + action + ")</i>\n" + dialogue;
        }
    }

    public void SetNPCPortrait(Sprite portrait)
    {
        Debug.Log("SetNPCPortrait called: " + (portrait != null ? portrait.name : "NULL"));
        if (npcPortrait)
        {
            npcPortrait.sprite = portrait;
            npcPortrait.color = portrait != null ? Color.white : Color.clear;
        }
    }

    public void SetInputLocked(bool locked)
    {
        if (playerInput) playerInput.interactable = !locked;
        if (sendButton) sendButton.interactable = !locked;
    }

    public void ShowLoading(bool show)
    {
        if (loadingIndicator) loadingIndicator.SetActive(show);
    }

    public void ShowDismissButton(bool show)
    {
        if (dismissButton) dismissButton.gameObject.SetActive(show);
    }

    public void ShowUncleOverride(string uncleMessage)
    {
        if (npcNameText) npcNameText.text = "The Regent";
        if (dialogueText) dialogueText.text = "<i>(He steps forward, silencing the room.)</i>\n" + uncleMessage;
        if (regentPortrait != null) SetNPCPortrait(regentPortrait);
        SetInputLocked(true);
        ShowDismissButton(false);
    }

    // ─────────────────────────────────────────────
    // 测试用：按 F1 随机刷新数值，验证动画 + 闪烁效果
    // 上线前删掉这个方法
    // ─────────────────────────────────────────────
 
    
}