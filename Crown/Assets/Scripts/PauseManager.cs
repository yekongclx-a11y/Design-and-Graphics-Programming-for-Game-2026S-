using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("UI Panels & General Buttons")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button mainMenuButton;
    public Button quitButton;

    [Header("In-Game Volume Settings (Newly Extracted)")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    private bool isPaused = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        pausePanel.SetActive(false);
        resumeButton.onClick.AddListener(Resume);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
        quitButton.onClick.AddListener(QuitGame);

        // ─────────────────────────────────────────────────────────
        // 🔌 【自查注入】全自动连线总线：免去在引擎面板中人肉拉事件的风险
        // ─────────────────────────────────────────────────────────
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    void Pause()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f; // 截断物理与游戏帧时序

        // 【核心同步】：当玩家在朝堂上按 ESC 时，立刻同步滑块到最新的音量
        if (AudioManager.Instance != null)
        {
            if (musicVolumeSlider != null) musicVolumeSlider.value = AudioManager.Instance.musicVolume;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = AudioManager.Instance.sfxVolume;
        }
    }

    public void Resume()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f; // 恢复时序
    }

    void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);
    }

    void OnSfxVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
    }

    void GoToMainMenu()
    {
        isPaused = false;
        Time.timeScale = 1f; // 极其重要：切回主菜单前必须将物理时间尺度恢复为 1，否则主界面会假死
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopMusic();
            
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}