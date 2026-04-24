using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PrologueController : MonoBehaviour
{
    [Header("UI Elements")]
    public Image princessPortrait;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;
    public Button continueButton;

    [Header("Audio")]
    public AudioClip[] voiceLines;

    private AudioSource audioSource;
    private int currentLine = 0;

    private string[] speakers = {
        "Princess",
        "Princess",
        "Princess"
    };

    private string[] lines = {
        "You were dreaming again. About that night, weren't you?",
        "The court is already gathering. Your uncle has been... busy this morning. Remember — your words are your only weapon in that hall. Every sentence matters.",
        "It is time. They are waiting for you, Your Majesty. Do not let them see you afraid."
    };

    private string[] actions = {
        "She sits beside your bed, her voice barely above a whisper.",
        "She glances toward the door, her brow furrowed.",
        "She rises, smoothing her dress, eyes filled with quiet urgency."
    };

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        continueButton.onClick.AddListener(OnContinueClicked);
        ShowLine(0);
    }

    void ShowLine(int index)
    {
        if (index >= lines.Length)
        {
            SceneManager.LoadScene("SampleScene");
            return;
        }

        speakerText.text = speakers[index];
        dialogueText.text = "<i>(" + actions[index] + ")</i>\n" + lines[index];

        if (voiceLines != null && index < voiceLines.Length && voiceLines[index] != null)
        {
            audioSource.PlayOneShot(voiceLines[index]);
        }
    }

    void OnContinueClicked()
    {
        currentLine++;
        ShowLine(currentLine);
    }
}