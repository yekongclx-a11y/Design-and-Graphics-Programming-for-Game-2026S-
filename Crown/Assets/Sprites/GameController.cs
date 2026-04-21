using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    void Start()
    {
        UIManager.Instance.sendButton.onClick.AddListener(OnSendClicked);
        UIManager.Instance.playerInput.onSubmit.AddListener(OnInputSubmit);
        DialogueSystem.Instance.StartRound(0);
        UIManager.Instance.UpdateResourceBars();
    }

    void OnSendClicked()
    {
        TMP_InputField inputField = UIManager.Instance.playerInput;
        string input = inputField.text.Trim();
        Debug.Log("Input text: " + input);
        if (!string.IsNullOrEmpty(input))
        {
            DialogueSystem.Instance.SubmitPlayerInput(input);
            inputField.text = "";
        }
    }

    void OnInputSubmit(string input)
    {
        Debug.Log("Submit text: " + input);
        if (!string.IsNullOrEmpty(input.Trim()))
        {
            DialogueSystem.Instance.SubmitPlayerInput(input.Trim());
            UIManager.Instance.playerInput.text = "";
        }
    }
}