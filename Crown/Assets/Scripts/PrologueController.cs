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
    public Button continueButton; // 注：现在充当视觉提示，逻辑已完全由全局点击总线托管

    [Header("Audio Configurations")]
    public AudioClip[] voiceLines;       // 槽位调整至 6，请在面板中依次拖入 01_prologue 至 06_prologue
    public AudioClip bedroomBGM;         // 专属扩容槽位：把你的舒缓卧室背景音乐拖进这里

    private AudioSource audioSource;
    private int currentLine = 0;

    // ─────────────────────────────────────────────────────────
    // 📜 沉浸式宫廷玩法教学文本总线（严控字数，防止爆框）
    // ─────────────────────────────────────────────────────────
    
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
        // 运行时动态挂载大喇叭，负责单独播放未婚妻的配音
        audioSource = gameObject.AddComponent<AudioSource>();
        
        // 【自查注入】：一开局立刻统一调度背景音乐
        if (AudioManager.Instance != null && bedroomBGM != null)
        {
            AudioManager.Instance.PlayMusic(bedroomBGM);
        }

        // 展现开盘第一句话
        ShowLine(0);
    }

    void Update()
    {
        // ─────────────────────────────────────────────────────────
        // 🖱️ 全局任意交互总线：释放玩家手腕，盲按即可推进剧情
        // ─────────────────────────────────────────────────────────
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            OnContinueClicked();
        }
    }

    void ShowLine(int index)
    {
        // 终点检查：如果 6 句台词全部交割完毕，平稳切入上朝场景正片
        if (index >= lines.Length)
        {
            SceneManager.LoadScene("SampleScene");
            return;
        }

        // 渲染文本框 UI
        speakerText.text = speakers[index];
        dialogueText.text = "<i>(" + actions[index] + ")</i>\n" + lines[index];

        // ─────────────────────────────────────────────────────────
        // 🎙️ 未婚妻同步语音调度中心（防音轨叠加穿帮）
        // ─────────────────────────────────────────────────────────
        if (voiceLines != null && index < voiceLines.Length && voiceLines[index] != null)
        {
            // 【核心安全自查】：掐断上一句残存的配音音频，绝不重叠打架
            audioSource.Stop(); 
            
            // 实时动态同步中央司令部的 SFX 音量，保证音量滑块对其有绝对控制权
            if (AudioManager.Instance != null)
            {
                audioSource.volume = AudioManager.Instance.sfxVolume;
            }
            
            audioSource.clip = voiceLines[index];
            audioSource.Play();
        }
    }

    void OnContinueClicked()
    {
        currentLine++;
        ShowLine(currentLine);
    }
}