using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("UI")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button mainMenuButton;
    public Button quitButton;

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
        Time.timeScale = 0f;
    }

    void Resume()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void GoToMainMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
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