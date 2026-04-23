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

    [Header("Settings")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    void Start()
    {
        ShowMain();
    }

    public void OnStartClicked()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OnSettingsClicked()
    {
        logo.SetActive(false);
        buttonGroup.SetActive(false);
        settingsPanel.SetActive(true);
        aboutPanel.SetActive(false);
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
        if (logo) logo.SetActive(true);
        if (buttonGroup) buttonGroup.SetActive(true);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (aboutPanel) aboutPanel.SetActive(false);
    }

    public void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void OnSfxVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("SfxVolume", value);
    }
}