using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Persistent system-level API status indicator.
/// Always visible in the bottom-right corner (except during cinematics).
/// Click or press Ctrl+Shift+A to open the API configuration panel at any time.
/// Zero Editor setup — entirely self-contained.
/// </summary>
public class APIStatusIndicator : MonoBehaviour
{
    private static APIStatusIndicator s_instance;

    private GameObject     _canvasRoot;
    private Image          _bg;
    private TextMeshProUGUI _label;

    static readonly Color ColBgNormal  = new Color(0.07f, 0.08f, 0.11f, 0.82f);
    static readonly Color ColBgWarning = new Color(0.30f, 0.12f, 0.04f, 0.90f);
    static readonly Color ColTextOK    = new Color(0.42f, 0.80f, 0.52f, 1f);
    static readonly Color ColTextWarn  = new Color(1.00f, 0.58f, 0.18f, 1f);

    // Scenes where the indicator is hidden (cinematics / narration)
    static readonly string[] HideInScenes = { "OpeningCG", "PrologueScene" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoCreate()
    {
        if (s_instance != null) return;
        var go = new GameObject("[APIStatusIndicator]");
        DontDestroyOnLoad(go);
        s_instance = go.AddComponent<APIStatusIndicator>();
    }

    void Awake()
    {
        if (s_instance != null && s_instance != this) { Destroy(gameObject); return; }
        s_instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        BuildIndicator();
        Refresh();
    }

    void Update()
    {
        bool ctrl  = Input.GetKey(KeyCode.LeftControl)  || Input.GetKey(KeyCode.RightControl);
        bool shift = Input.GetKey(KeyCode.LeftShift)    || Input.GetKey(KeyCode.RightShift);
        if (ctrl && shift && Input.GetKeyDown(KeyCode.A))
            OpenPanel();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool hide = System.Array.IndexOf(HideInScenes, scene.name) >= 0;
        if (_canvasRoot != null) _canvasRoot.SetActive(!hide);
        Refresh();
    }

    // ── Build ────────────────────────────────────────────────────────────────

    void BuildIndicator()
    {
        _canvasRoot = new GameObject("_Canvas");
        _canvasRoot.transform.SetParent(transform, false);

        var canvas = _canvasRoot.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9998;                         // just below FirstRunSetup (9999)

        var scaler = _canvasRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        _canvasRoot.AddComponent<GraphicRaycaster>();

        // Pill button — bottom-right corner
        var pill = new GameObject("_Pill");
        pill.transform.SetParent(_canvasRoot.transform, false);

        _bg       = pill.AddComponent<Image>();
        _bg.color = ColBgNormal;

        var btn = pill.AddComponent<Button>();
        btn.targetGraphic = _bg;
        var cols = btn.colors;
        cols.normalColor      = ColBgNormal;
        cols.highlightedColor = new Color(0.15f, 0.17f, 0.23f, 0.95f);
        cols.pressedColor     = new Color(0.04f, 0.05f, 0.07f, 1f);
        cols.colorMultiplier  = 1f;
        btn.colors = cols;
        btn.onClick.AddListener(OpenPanel);

        var rt = pill.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(1f, 0f);   // bottom-right anchor
        rt.anchorMax        = new Vector2(1f, 0f);
        rt.pivot            = new Vector2(1f, 0f);
        rt.sizeDelta        = new Vector2(270f, 30f);
        rt.anchoredPosition = new Vector2(-12f, 12f);

        // Label
        var labelGO = new GameObject("_Label");
        labelGO.transform.SetParent(pill.transform, false);
        _label                    = labelGO.AddComponent<TextMeshProUGUI>();
        _label.fontSize           = 11;
        _label.alignment          = TextAlignmentOptions.MidlineLeft;
        _label.enableWordWrapping = false;
        _label.raycastTarget      = false;
        var labelRT = labelGO.GetComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = new Vector2(8f, 0f);
        labelRT.offsetMax = new Vector2(-6f, 0f);
    }

    // ── Display ──────────────────────────────────────────────────────────────

    public void Refresh()
    {
        if (_label == null || _bg == null) return;

        bool   hasKey = GameConfig.Instance != null &&
                        !string.IsNullOrEmpty(GameConfig.Instance.Config.apiKey);
        string model  = GameConfig.Instance?.Config.model ?? "";

        if (hasKey)
        {
            string display = string.IsNullOrEmpty(model) ? "—" : Truncate(model, 26);
            _label.text  = "⚙  " + display;
            _label.color = ColTextOK;
            _bg.color    = ColBgNormal;
        }
        else
        {
            _label.text  = "⚠  No API Key  —  click to configure";
            _label.color = ColTextWarn;
            _bg.color    = ColBgWarning;
        }
    }

    // ── Panel ────────────────────────────────────────────────────────────────

    void OpenPanel()
    {
        if (FindObjectOfType<FirstRunSetup>() != null) return;   // already open
        FirstRunSetup.Show(() => Refresh(), isFirstRun: false);
    }

    static string Truncate(string s, int max) =>
        s.Length <= max ? s : s.Substring(0, max - 1) + "…";
}
