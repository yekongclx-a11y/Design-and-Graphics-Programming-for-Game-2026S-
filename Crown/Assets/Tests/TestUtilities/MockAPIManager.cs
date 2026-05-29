using System;
using UnityEngine;

public class MockAPIManager : MonoBehaviour
{
    public static MockAPIManager Instance;

    public Action<string> onRequestCaptured;

    void Awake()
    {
        Instance = this;
    }

    public void SendMessage(string npcName, string surfaceRequest,
        string hiddenMotive, string playerInput,
        int currentTurn, int maxTurns,
        bool isDismiss,
        System.Action<AIResponse> onComplete)
    {
        onRequestCaptured?.Invoke(playerInput);

        // deterministic fake response
        AIResponse mock = new AIResponse
        {
            action = "mock_action",
            dialogue = "mock_dialogue_response",
            gold = 0,
            popularity = 0,
            church = 0,
            military = 0,
            suspicion = 0,
            triggerEvent = "none"
        };

        onComplete?.Invoke(mock);
    }
}