using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Crown.UI; // 💡 确保这行没有被任何奇奇怪怪的注释和格式截断
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Resource Bars")]
    public ResourceBarUI goldBar;
    public ResourceBarUI popularityBar;
    public ResourceBarUI churchBar;
    public ResourceBarUI militaryBar;

    [Header("Dialogue")]
    public TextMeshProUGUI npcNameText;
    public TextMeshProUGUI actionText;
    public TextMeshProUGUI dialogueText;

    [Header("Input")]
    public TMP_InputField playerInput;
    public Button sendButton;

    [Header("NPC Portrait")]
    public Image npcPortrait;

    [Header("Special")]
    public Sprite regentPortrait;

    [Header("Round Info")]
    public TextMeshProUGUI roundText;

    [Header("Loading")]
    public GameObject loadingIndicator;

    private Coroutine typingCoroutine;
    [HideInInspector] public bool isTyping = false;

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
        if (roundText)     roundText.text = "Round " + gs.currentRound + " / " + gs.maxRounds;
    }

    public void ResetResourceBars()
    {
        GameStateManager gs = GameStateManager.Instance;
        if (goldBar)       goldBar.SetValueImmediate(gs.gold);
        if (popularityBar) popularityBar.SetValueImmediate(gs.popularity);
        if (churchBar)     churchBar.SetValueImmediate(gs.church);
        if (militaryBar)   militaryBar.SetValueImmediate(gs.military);
        if (roundText)     roundText.text = "Round " + gs.currentRound + " / " + gs.maxRounds;
    }

    public void DisplayNPCResponse(string npcName, string action, string dialogue)
    {
        if (npcNameText) npcNameText.text = npcName;

        if (dialogueText)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            
            string fullText = string.IsNullOrEmpty(action) ? dialogue : $"<i>({action})</i>\n{dialogue}";
            typingCoroutine = StartCoroutine(TypewriterRoutine(fullText));
        }
    }

    IEnumerator TypewriterRoutine(string targetText)
    {
        isTyping = true;
        dialogueText.text = "";
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int i = 0; i < targetText.Length; i++)
        {
            if (targetText[i] == '<')
            {
                int endTagStr = targetText.IndexOf('>', i);
                if (endTagStr != -1)
                {
                    sb.Append(targetText.Substring(i, endTagStr - i + 1));
                    i = endTagStr;
                    continue;
                }
            }
            sb.Append(targetText[i]);
            dialogueText.text = sb.ToString();
            yield return new WaitForSeconds(0.02f);
        }
        dialogueText.text = targetText;
        isTyping = false;
    }

    public void SetNPCPortrait(Sprite portrait)
    {
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

    public void ShowUncleOverride(string uncleMessage)
    {
        if (npcNameText) npcNameText.text = "The Regent";
        if (regentPortrait != null) SetNPCPortrait(regentPortrait);
        
        SetInputLocked(true);
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        
        string fullText = $"<i>(He steps forward, silencing the room.)</i>\n{uncleMessage}";
        typingCoroutine = StartCoroutine(TypewriterRoutine(fullText));
    }
}