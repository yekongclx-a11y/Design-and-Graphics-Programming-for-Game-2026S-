using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UILayout : MonoBehaviour
{
    [Header("Panels")]
    public RectTransform resourcePanel;
    public RectTransform dialoguePanel;
    public RectTransform nameBox;
    public RectTransform dialogueBox;

    [Header("Input Area")]
    public RectTransform playerInput;
    public RectTransform sendButton;

    [Header("Settings")]
    public float resourcePanelHeight = 80f;
    public float dialoguePanelHeight = 280f;
    public float nameBoxHeight = 45f;
    public float inputBarHeight = 60f;
    public float padding = 16f;

    void Start()
    {
        ApplyLayout();
    }

    public void ApplyLayout()
    {
        // 资源栏 顶部
        SetAnchors(resourcePanel, 0, 1, 1, 1);
        resourcePanel.sizeDelta = new Vector2(0, resourcePanelHeight);
        resourcePanel.anchoredPosition = Vector2.zero;

        // 对话框 底部
        SetAnchors(dialoguePanel, 0, 0, 1, 0);
        dialoguePanel.sizeDelta = new Vector2(0, dialoguePanelHeight);
        dialoguePanel.anchoredPosition = Vector2.zero;

        // 名字框 对话框左上角
        SetAnchors(nameBox, 0, 1, 0, 1);
        nameBox.sizeDelta = new Vector2(200f, nameBoxHeight);
        nameBox.anchoredPosition = new Vector2(padding, 0);

        // 对话内容框
        SetAnchors(dialogueBox, 0, 1, 1, 1);
        float dialogueBoxTop = -nameBoxHeight;
        float dialogueBoxBottom = inputBarHeight + padding;
        dialogueBox.offsetMin = new Vector2(padding, dialogueBoxBottom);
        dialogueBox.offsetMax = new Vector2(-padding, dialogueBoxTop);

        // 输入框 底部左边
        SetAnchors(playerInput, 0, 0, 1, 0);
        playerInput.sizeDelta = new Vector2(-(160f + padding * 2), inputBarHeight);
        playerInput.anchoredPosition = new Vector2(padding, padding);

        // 发送按钮 底部右边
        SetAnchors(sendButton, 1, 0, 1, 0);
        sendButton.sizeDelta = new Vector2(160f, inputBarHeight);
        sendButton.anchoredPosition = new Vector2(-padding, padding);
    }

    void SetAnchors(RectTransform rt, float minX, float minY, float maxX, float maxY)
    {
        rt.anchorMin = new Vector2(minX, minY);
        rt.anchorMax = new Vector2(maxX, maxY);
        rt.pivot = new Vector2(minX, minY);
    }
}