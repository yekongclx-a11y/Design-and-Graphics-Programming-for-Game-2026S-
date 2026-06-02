using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingsPanel;
    public GameObject aboutPanel;

    [Header("Buttons")]
    public GameObject logo;
    public GameObject buttonGroup;

    [Header("Settings UI elements")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    private TextMeshProUGUI musicVolText;
    private TextMeshProUGUI sfxVolText;

    void Start()
    {
        ShowMain();
        CreateFixedLabel(musicVolumeSlider, ref musicVolText);
        CreateFixedLabel(sfxVolumeSlider,   ref sfxVolText);

        Debug.Log("AudioManager存在吗: " + (AudioManager.Instance != null));
        Debug.Log("bgmTheme存在吗: " + (AudioManager.Instance?.bgmTheme != null));

        SynchronizeSliders();
        Invoke("PlayTheme", 0.1f);
    }

    void PlayTheme()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayThemeMusic();
    }

    public void OnStartClicked()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopMusic();

        // Session skip: if prologue was already completed this session, go straight to the game.
        if (SessionState.ProloguePlayed)
            SceneManager.LoadScene("SampleScene");
        else
            SceneManager.LoadScene("OpeningCG");
    }

    public void OnSettingsClicked()
    {
        logo.SetActive(false);
        buttonGroup.SetActive(false);
        settingsPanel.SetActive(true);
        aboutPanel.SetActive(false);
        SynchronizeSliders();
    }

    public void OnAboutClicked()
    {
        logo.SetActive(false);
        buttonGroup.SetActive(false);
        settingsPanel.SetActive(false);
        aboutPanel.SetActive(true);
    }

    public void OnQuitClicked()
    {
        Application.Quit();
        Debug.Log("Quit");
    }

    public void OnBackClicked()
    {
        ShowMain();
    }

    void ShowMain()
    {
        if (logo)          logo.SetActive(true);
        if (buttonGroup)   buttonGroup.SetActive(true);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (aboutPanel)    aboutPanel.SetActive(false);
    }

    public void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);
        UpdateLabelText(musicVolText, value);
    }

    public void OnSfxVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
        UpdateLabelText(sfxVolText, value);
    }

    private void SynchronizeSliders()
    {
        if (AudioManager.Instance != null)
        {
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = AudioManager.Instance.musicVolume;
                UpdateLabelText(musicVolText, musicVolumeSlider.value);
            }
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = AudioManager.Instance.sfxVolume;
                UpdateLabelText(sfxVolText, sfxVolumeSlider.value);
            }
        }
    }

    private void CreateFixedLabel(Slider slider, ref TextMeshProUGUI targetText)
    {
        if (slider == null) return;

        GameObject textObj = new GameObject("Fixed_Vol_Label");
        textObj.transform.SetParent(slider.transform, false);

        targetText = textObj.AddComponent<TextMeshProUGUI>();
        targetText.fontSize  = 16;
        targetText.alignment = TextAlignmentOptions.Left;
        targetText.color     = Color.white;
        targetText.raycastTarget = false;

        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin       = new Vector2(1f, 0.5f);
        rect.anchorMax       = new Vector2(1f, 0.5f);
        rect.pivot           = new Vector2(0f, 0.5f);
        rect.anchoredPosition = new Vector2(15f, 0f);
    }

    private void UpdateLabelText(TextMeshProUGUI textComponent, float value)
    {
        if (textComponent != null)
            textComponent.text = Mathf.RoundToInt(value * 100) + "%";
    }
}
