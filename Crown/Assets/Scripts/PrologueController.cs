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

    [Header("Audio Configurations")]
    public AudioClip[] voiceLines;
    public AudioClip bedroomBGM;

    [Header("Skip")]
    // Create a Button in PrologueScene, assign it here.
    // It is automatically hidden for first-time players and shown for returning players.
    public Button skipButton;

    private AudioSource audioSource;
    private int currentLine = 0;
    private bool isTransitioning = false;

    private string[] speakers = {
        "Princess Elara", "Princess Elara", "Princess Elara",
        "Princess Elara", "Princess Elara", "Princess Elara"
    };

    private string[] lines = {
        "You were dreaming again. About that night, weren't you? Wake up, my love. The court is already gathering.",
        "Watch the lords closely. Gaze into their eyes to read their shifting stances. Push any faction too far, and they will overthrow you.",
        "Time is fleeting. You only have three chances to speak with each lord before they force your hand. Every choice counts.",
        "The hall is a viper's nest. Sudden crises and unexpected outcries will erupt from the crowd without warning. Stay sharp.",
        "Beware your Uncle. His paranoia is near the absolute brink. One final misstep, and his spies will drag you to the Tower.",
        "It is time. They are waiting for you, Your Majesty. Go, and show them who rules this kingdom."
    };

    private string[] actions = {
        "She sits beside your bed, her voice barely above a whisper.",
        "She hands you your signet ring, looking deep into your eyes.",
        "She adjusts your royal collar, her fingers trembling slightly.",
        "She holds your hand tightly, a cold sweat on her palm.",
        "She glances toward the heavy door, her voice dropping lower.",
        "She rises, smoothing her dress, eyes filled with quiet urgency."
    };

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        if (AudioManager.Instance != null && bedroomBGM != null)
            AudioManager.Instance.PlayMusic(bedroomBGM);

        // Skip button: only for players who have BOTH seen the prologue before
        // AND already configured their API key. This is the true "returning player" state.
        bool hasPlayed = PlayerPrefs.GetInt("hasEverPlayed", 0) == 1;
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(hasPlayed);
            skipButton.onClick.AddListener(OnSkipClicked);
        }

        ShowLine(0);
    }

    void Update()
    {
        if (isTransitioning) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            OnContinueClicked();
    }

    public void OnSkipClicked()
    {
        if (isTransitioning) return;
        isTransitioning = true;
        if (audioSource != null) audioSource.Stop();
        ProceedToGame();
    }

    void ShowLine(int index)
    {
        if (index >= lines.Length)
        {
            if (isTransitioning) return;
            isTransitioning = true;
            ProceedToGame();
            return;
        }

        speakerText.text   = speakers[index];
        dialogueText.text  = "<i>(" + actions[index] + ")</i>\n" + lines[index];

        if (voiceLines != null && index < voiceLines.Length && voiceLines[index] != null)
        {
            audioSource.Stop();
            if (AudioManager.Instance != null)
                audioSource.volume = AudioManager.Instance.sfxVolume;
            audioSource.clip = voiceLines[index];
            audioSource.Play();
        }
    }

    void OnContinueClicked()
    {
        if (isTransitioning) return;
        currentLine++;
        ShowLine(currentLine);
    }

    // Single exit point for all prologue completion paths.
    // Triggers API setup overlay whenever the key is missing (not just on first-ever play).
    void ProceedToGame()
    {
        bool apiKeyMissing = GameConfig.Instance == null ||
                             string.IsNullOrEmpty(GameConfig.Instance.Config.apiKey);
        MarkPrologueComplete();

        if (apiKeyMissing && !SessionState.ApiSetupShown)
        {
            SessionState.ApiSetupShown = true;
            FirstRunSetup.Show(() => SceneManager.LoadScene("SampleScene"), isFirstRun: true);
        }
        else
        {
            SceneManager.LoadScene("SampleScene");
        }
    }

    static void MarkPrologueComplete()
    {
        SessionState.ProloguePlayed = true;
        PlayerPrefs.SetInt("hasEverPlayed", 1);
        PlayerPrefs.Save();
    }
}
