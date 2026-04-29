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

    UIManager.Instance.sendButton.onClick.RemoveAllListeners();
    UIManager.Instance.playerInput.onSubmit.RemoveAllListeners();

    UIManager.Instance.sendButton.onClick.AddListener(OnSendClicked);
    UIManager.Instance.playerInput.onSubmit.AddListener(OnInputSubmit);

    if (UIManager.Instance.dismissButton != null)
    {
        UIManager.Instance.dismissButton.onClick.RemoveAllListeners();
        UIManager.Instance.dismissButton.onClick.AddListener(OnDismissClicked);
    }

    Debug.Log("Starting audio...");
    AudioManager.Instance.PlayMainMusic();
    
    Debug.Log("Starting round...");
    DialogueSystem.Instance.StartRound(0);
    
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

    void OnDismissClicked()
    {
        DialogueSystem.Instance.SubmitDismiss();
    }
}