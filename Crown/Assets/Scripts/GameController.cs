using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    void Start()
    {
        Debug.Log("GameController Start called");
        
        if (UIManager.Instance == null || GameStateManager.Instance == null)
        {
            Debug.LogError("Manager not found!");
            return;
        }
        
        Debug.Log("UIManager: " + UIManager.Instance.name);
        Debug.Log("AudioManager: " + AudioManager.Instance.name);
        
        GameStateManager.Instance.ResetGame();
        EventManager.Instance.ResetEvents();

        // 核心修正：仅保留玩家文本输入和发送按钮的事件绑定
        UIManager.Instance.sendButton.onClick.RemoveAllListeners();
        UIManager.Instance.playerInput.onSubmit.RemoveAllListeners();

        UIManager.Instance.sendButton.onClick.AddListener(OnSendClicked);
        UIManager.Instance.playerInput.onSubmit.AddListener(OnInputSubmit);

        // 🌟 干净利落：彻底删除了关于已废弃的 dismissButton 的所有报错代码

        Debug.Log("Starting audio...");
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMainMusic();
        }
        
        Debug.Log("Starting round...");
        if (DialogueSystem.Instance != null)
        {
            DialogueSystem.Instance.StartRound(0);
        }
        
        UIManager.Instance.UpdateResourceBars();
        Debug.Log("GameController Start complete");
    }

    void OnSendClicked()
    {
        string input = UIManager.Instance.playerInput.text.Trim();
        if (!string.IsNullOrEmpty(input))
        {
            DialogueSystem.Instance.SubmitPlayerInput(input);
            UIManager.Instance.playerInput.text = "";
        }
    }

    void OnInputSubmit(string input)
    {
        if (!string.IsNullOrEmpty(input.Trim()))
        {
            DialogueSystem.Instance.SubmitPlayerInput(input.Trim());
            UIManager.Instance.playerInput.text = "";
        }
    }

    // 🌟 干净利落：彻底删除了已经无家可归的 OnDismissClicked() 回调方法
}