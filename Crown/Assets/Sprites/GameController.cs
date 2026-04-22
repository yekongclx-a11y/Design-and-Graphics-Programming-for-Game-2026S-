using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    void Start()
    {
        UIManager.Instance.sendButton.onClick.AddListener(OnSendClicked);
        UIManager.Instance.playerInput.onSubmit.AddListener(OnInputSubmit);

        if (UIManager.Instance.dismissButton != null)
            UIManager.Instance.dismissButton.onClick.AddListener(OnDismissClicked);

        AudioManager.Instance.PlayMainMusic();
        DialogueSystem.Instance.StartRound(0);
        UIManager.Instance.UpdateResourceBars();
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