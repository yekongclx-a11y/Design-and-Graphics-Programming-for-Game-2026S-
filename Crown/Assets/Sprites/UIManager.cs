using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Resource Bars")]
    public Slider goldBar;
    public Slider popularityBar;
    public Slider churchBar;
    public Slider militaryBar;

    [Header("Resource Labels")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI popularityText;
    public TextMeshProUGUI churchText;
    public TextMeshProUGUI militaryText;

    [Header("Dialogue")]
    public TextMeshProUGUI npcNameText;
    public TextMeshProUGUI actionText;
    public TextMeshProUGUI dialogueText;

    [Header("Input")]
    public TMP_InputField playerInput;
    public Button sendButton;

    [Header("NPC Portrait")]
    public Image npcPortrait;

    [Header("Round Info")]
    public TextMeshProUGUI roundText;

    [Header("Loading")]
    public GameObject loadingIndicator;

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

    public void UpdateResourceBars()
    {
        GameStateManager gs = GameStateManager.Instance;

        // ===== 往下是植入的“窃听器” =====
        Debug.Log($"【UI更新测试】UpdateResourceBars 被成功调用！此时底层数据 金币:{gs.gold}, 民心:{gs.popularity}");
        if (goldBar == null) 
        {
            Debug.LogError("【致命错误】虽然你第一步检查了Inspector有东西，但代码运行到这里时，goldBar 居然是空的！引用丢失了！");
        }
        // ===== 往上是植入的“窃听器” =====

        if (goldBar) goldBar.value = gs.gold;
        if (popularityBar) popularityBar.value = gs.popularity;
        if (churchBar) churchBar.value = gs.church;
        if (militaryBar) militaryBar.value = gs.military;

        if (goldText) goldText.text = gs.gold.ToString();
        if (popularityText) popularityText.text = gs.popularity.ToString();
        if (churchText) churchText.text = gs.church.ToString();
        if (militaryText) militaryText.text = gs.military.ToString();

        if (roundText) roundText.text = "Round " + gs.currentRound + " / " + gs.maxRounds;
    }

    public void DisplayNPCResponse(string npcName, string action, string dialogue)
    {
        if (npcNameText) npcNameText.text = npcName;
        if (actionText) actionText.text = "(" + action + ")";
        if (dialogueText) dialogueText.text = dialogue;
    }

    public void SetNPCPortrait(Sprite portrait)
    {
        if (npcPortrait) npcPortrait.sprite = portrait;
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

    public void ShowUncleOverride(string uncleMessage)
    {
        if (npcNameText) npcNameText.text = "The Regent";
        if (actionText) actionText.text = "(He steps forward, silencing the room.)";
        if (dialogueText) dialogueText.text = uncleMessage;
        SetInputLocked(true);
    }
}