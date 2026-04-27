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
}