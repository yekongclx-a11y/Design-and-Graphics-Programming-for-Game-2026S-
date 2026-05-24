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

    // 静态文字组件引用
    private TextMeshProUGUI musicVolText;
    private TextMeshProUGUI sfxVolText;

    void Start()
    {
        ShowMain();
        
        // 【全新逻辑】：全自动在滑块的右侧（后面）创建固定标签
        CreateFixedLabel(musicVolumeSlider, ref musicVolText);
        CreateFixedLabel(sfxVolumeSlider, ref sfxVolText);

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
        if (logo) logo.SetActive(true);
        if (buttonGroup) buttonGroup.SetActive(true);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (aboutPanel) aboutPanel.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────
    // 🎛️ 音量动态映射与数值随动更新
    // ─────────────────────────────────────────────────────────
    
    public void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
        UpdateLabelText(musicVolText, value);
    }

    public void OnSfxVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
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

    // ─────────────────────────────────────────────────────────
    // 🛠️ 纯代码自动化：在滑条右侧边缘固定生成数字
    // ─────────────────────────────────────────────────────────
    
    private void CreateFixedLabel(Slider slider, ref TextMeshProUGUI targetText)
    {
        // 核心解耦：直接挂载在 Slider 身上，不再看 Handle 的脸色
        if (slider == null) return;

        GameObject textObj = new GameObject("Fixed_Vol_Label");
        textObj.transform.SetParent(slider.transform, false);

        targetText = textObj.AddComponent<TextMeshProUGUI>();
        targetText.fontSize = 16; // 侧边文字稍微大一点点，看起来更清晰
        targetText.alignment = TextAlignmentOptions.Left; // 左对齐，让百分比向右延展
        targetText.color = Color.white;
        targetText.raycastTarget = false;

        // 🎯 UI 强行锚定在物理滑条的【正右侧边缘】
        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0.5f);  // 锚点对齐父物体（滑条）的右边缘、垂直居中
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);      // 字体的中心点设在自己的左边缘
        rect.anchoredPosition = new Vector2(15f, 0f); // 向右物理偏移 15 个像素，保证绝不重叠
    }

    private void UpdateLabelText(TextMeshProUGUI textComponent, float value)
    {
        if (textComponent != null)
        {
            textComponent.text = Mathf.RoundToInt(value * 100) + "%";
        }
    }
}