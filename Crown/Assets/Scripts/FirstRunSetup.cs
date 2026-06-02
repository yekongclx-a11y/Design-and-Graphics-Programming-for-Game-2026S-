using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class FirstRunSetup : MonoBehaviour
{
    // ── Presets ──────────────────────────────────────────────────────────────
    struct Preset { public string Label, Url, Model, KeyPageUrl, KeyPageLabel; }

    static readonly Preset[] Presets =
    {
        new Preset {
            Label        = "Gemini",
            Url          = "https://generativelanguage.googleapis.com/v1beta/openai/chat/completions",
            Model        = "gemini-2.0-flash",
            KeyPageUrl   = "https://aistudio.google.com/app/apikey",
            KeyPageLabel = "→ Get free Gemini key"
        },
        new Preset {
            Label        = "OpenAI",
            Url          = "https://api.openai.com/v1/chat/completions",
            Model        = "gpt-4o-mini",
            KeyPageUrl   = "https://platform.openai.com/api-keys",
            KeyPageLabel = "→ Get OpenAI key"
        },
        new Preset {
            Label        = "Custom",
            Url          = "",
            Model        = "",
            KeyPageUrl   = "",
            KeyPageLabel = ""
        },
    };

    // ── State ────────────────────────────────────────────────────────────────
    private static Action s_onComplete;
    private static bool   s_isFirstRun;

    private TMP_InputField  _urlField, _keyField, _modelField;
    private Image[]         _presetBtnImages;
    private TextMeshProUGUI _getKeyLabel;
    private TextMeshProUGUI _testStatus;
    private TextMeshProUGUI _showHideLbl;
    private TextMeshProUGUI _currentStatusLbl;
    private Button          _testBtn;
    private bool            _isTesting;
    private bool            _keyVisible;
    private int             _selectedPreset;

    // ── Colors ───────────────────────────────────────────────────────────────
    static readonly Color ColPanel       = new Color(0.10f, 0.11f, 0.14f, 1f);
    static readonly Color ColTitleBar    = new Color(0.07f, 0.08f, 0.11f, 1f);
    static readonly Color ColStatusBg    = new Color(0.08f, 0.09f, 0.13f, 1f);
    static readonly Color ColFieldBg     = new Color(0.06f, 0.07f, 0.09f, 1f);
    static readonly Color ColFieldBorder = new Color(0.22f, 0.25f, 0.31f, 1f);
    static readonly Color ColSep         = new Color(0.18f, 0.21f, 0.27f, 1f);
    static readonly Color ColSelected    = new Color(0.14f, 0.38f, 0.68f, 1f);
    static readonly Color ColUnselected  = new Color(0.16f, 0.18f, 0.23f, 1f);
    static readonly Color ColTextMain    = new Color(0.80f, 0.84f, 0.90f, 1f);
    static readonly Color ColTextDim     = new Color(0.44f, 0.49f, 0.56f, 1f);
    static readonly Color ColTestBtn     = new Color(0.16f, 0.26f, 0.40f, 1f);
    static readonly Color ColAccent      = new Color(0.30f, 0.65f, 1.00f, 1f);
    static readonly Color ColGreen       = new Color(0.25f, 0.78f, 0.45f, 1f);
    static readonly Color ColOrange      = new Color(0.90f, 0.58f, 0.15f, 1f);
    static readonly Color ColRed         = new Color(0.90f, 0.32f, 0.28f, 1f);

    // ── Public API ───────────────────────────────────────────────────────────
    public static void Show(Action onComplete, bool isFirstRun = false)
    {
        s_onComplete = onComplete;
        s_isFirstRun = isFirstRun;
        var go = new GameObject("[FirstRunSetup]");
        DontDestroyOnLoad(go);
        go.AddComponent<FirstRunSetup>();
    }

    void Start()
    {
        GameConfig.Instance?.Load();
        BuildOverlay();

        // Detect preset from saved URL (hostname comparison, not fragile substring)
        string existingUrl    = GameConfig.Instance?.Config.apiUrl ?? "";
        int    detectedPreset = Presets.Length - 1;
        for (int i = 0; i < Presets.Length - 1; i++)
        {
            if (string.IsNullOrEmpty(Presets[i].Url)) continue;
            string host = ExtractHost(Presets[i].Url);
            if (!string.IsNullOrEmpty(host) && existingUrl.Contains(host))
            { detectedPreset = i; break; }
        }
        HighlightPreset(detectedPreset);
        UpdateGetKeyLink(detectedPreset);
        RefreshCurrentStatus();

        // Pre-fill fields from saved config — always wins over any preset default
        if (_urlField   != null) _urlField.text   = existingUrl;
        if (_modelField != null) _modelField.text = GameConfig.Instance?.Config.model  ?? "";
        if (_keyField   != null) _keyField.text   = GameConfig.Instance?.Config.apiKey ?? "";
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Proceed(false);
    }

    // ── Overlay ──────────────────────────────────────────────────────────────
    void BuildOverlay()
    {
        var cgo = new GameObject("_Canvas");
        cgo.transform.SetParent(transform, false);
        var cv = cgo.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 9999;
        var sc = cgo.AddComponent<CanvasScaler>();
        sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);
        sc.matchWidthOrHeight  = 0.5f;
        cgo.AddComponent<GraphicRaycaster>();

        // Scrim — click outside panel to close (only for non-first-run)
        var scrimGO  = new GameObject("_Scrim");
        scrimGO.transform.SetParent(cgo.transform, false);
        var scrimImg = scrimGO.AddComponent<Image>();
        scrimImg.color = new Color(0f, 0f, 0f, 0.86f);
        Fill(scrimGO.GetComponent<RectTransform>(), V(0, 0), V(1, 1));
        if (!s_isFirstRun)
        {
            var scrimBtn = scrimGO.AddComponent<Button>();
            scrimBtn.transition = Selectable.Transition.None;
            scrimBtn.onClick.AddListener(() => Proceed(false));
        }

        // Panel  860 × 680
        var panel = new GameObject("_Panel");
        panel.transform.SetParent(cgo.transform, false);
        panel.AddComponent<Image>().color = ColPanel;
        var pRT = panel.GetComponent<RectTransform>();
        pRT.anchorMin = pRT.anchorMax = pRT.pivot = V(0.5f, 0.5f);
        pRT.sizeDelta        = new Vector2(860, 680);
        pRT.anchoredPosition = Vector2.zero;
        Transform p = panel.transform;

        // ── Title bar ─────────────────────────────────────────────────────
        Img(p, "_TitleBar", V(0f, 0.906f), V(1f, 1f), ColTitleBar);
        Lbl(p, "  ⚙   API Configuration",
            V(0f, 0.906f), V(0.84f, 1f),
            19, Color.white, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);

        // ✕ close button — Image bg (transparent→red on hover) + child TMP
        var xGO  = new GameObject("_CloseBtn");
        xGO.transform.SetParent(p, false);
        var xImg = xGO.AddComponent<Image>();
        xImg.color = new Color(0f, 0f, 0f, 0f);
        var xBtn = xGO.AddComponent<Button>();
        xBtn.targetGraphic = xImg;
        var xC = xBtn.colors;
        xC.normalColor      = new Color(0f, 0f, 0f, 0f);
        xC.highlightedColor = new Color(0.55f, 0.15f, 0.12f, 0.9f);
        xC.pressedColor     = new Color(0.38f, 0.09f, 0.07f, 1f);
        xC.colorMultiplier  = 1f;
        xBtn.colors = xC;
        xBtn.onClick.AddListener(() => Proceed(false));
        Fill(xGO.GetComponent<RectTransform>(), V(0.86f, 0.910f), V(0.99f, 0.996f));
        var xLblGO  = new GameObject("_L");
        xLblGO.transform.SetParent(xGO.transform, false);
        var xLbl = xLblGO.AddComponent<TextMeshProUGUI>();
        xLbl.text           = "X";
        xLbl.fontSize       = 17;
        xLbl.color          = new Color(0.58f, 0.61f, 0.66f, 1f);
        xLbl.alignment      = TextAlignmentOptions.Center;
        xLbl.raycastTarget  = false;
        Fill(xLblGO.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);

        Img(p, "_S0", V(0.02f, 0.901f), V(0.98f, 0.906f), ColSep);

        // ── Current config status row ─────────────────────────────────────
        Img(p, "_StatusBg", V(0f, 0.853f), V(1f, 0.898f), ColStatusBg);
        _currentStatusLbl = Lbl(p, "",
            V(0.04f, 0.853f), V(0.96f, 0.898f),
            12, ColTextDim, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
        Img(p, "_S1", V(0f, 0.848f), V(1f, 0.853f), ColSep);

        // ── Provider row ──────────────────────────────────────────────────
        Lbl(p, "Provider",
            V(0.04f, 0.806f), V(0.21f, 0.838f),
            11, ColTextDim, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);

        _presetBtnImages = new Image[Presets.Length];
        float[] px = { 0.22f, 0.44f, 0.66f };
        float[] pw = { 0.19f, 0.19f, 0.16f };
        for (int i = 0; i < Presets.Length; i++)
        {
            int idx = i;
            _presetBtnImages[i] = Btn(p, Presets[i].Label, ColUnselected,
                V(px[i], 0.793f), V(px[i] + pw[i], 0.838f), 12,
                () => OnPresetButtonClicked(idx));
        }
        Img(p, "_S2", V(0.02f, 0.786f), V(0.98f, 0.791f), ColSep);

        // ── API Endpoint ──────────────────────────────────────────────────
        Lbl(p, "API Endpoint",
            V(0.04f, 0.748f), V(0.55f, 0.778f),
            12, ColTextDim, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
        Lbl(p, "any OpenAI-compatible URL",
            V(0.04f, 0.748f), V(0.96f, 0.778f),
            11, new Color(0.32f, 0.36f, 0.42f), FontStyles.Italic,
            TextAlignmentOptions.MidlineRight);
        _urlField = Field(p, "https://…", V(0.04f, 0.650f), V(0.96f, 0.742f));

        // ── API Key ───────────────────────────────────────────────────────
        Lbl(p, "API Key",
            V(0.04f, 0.607f), V(0.36f, 0.638f),
            12, ColTextDim, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);

        // [show/hide] toggle — Image root guarantees RectTransform; TMP in child for text
        var shGO  = new GameObject("_ShowHide");
        shGO.transform.SetParent(p, false);
        var shImg = shGO.AddComponent<Image>();
        shImg.color = new Color(0f, 0f, 0f, 0f);
        var shBtn = shGO.AddComponent<Button>();
        shBtn.targetGraphic = shImg;
        shBtn.transition = Selectable.Transition.None;
        shBtn.onClick.AddListener(ToggleKeyVisibility);
        Fill(shGO.GetComponent<RectTransform>(), V(0.37f, 0.607f), V(0.52f, 0.638f));
        var shLblGO = new GameObject("_L");
        shLblGO.transform.SetParent(shGO.transform, false);
        _showHideLbl               = shLblGO.AddComponent<TextMeshProUGUI>();
        _showHideLbl.text          = "[ show ]";
        _showHideLbl.fontSize      = 11;
        _showHideLbl.color         = ColAccent;
        _showHideLbl.alignment     = TextAlignmentOptions.MidlineLeft;
        _showHideLbl.raycastTarget = false;
        Fill(shLblGO.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);

        // Get-key link — same pattern
        var gkGO  = new GameObject("_GetKey");
        gkGO.transform.SetParent(p, false);
        var gkImg = gkGO.AddComponent<Image>();
        gkImg.color = new Color(0f, 0f, 0f, 0f);
        var gkBtn = gkGO.AddComponent<Button>();
        gkBtn.targetGraphic = gkImg;
        gkBtn.transition = Selectable.Transition.None;
        gkBtn.onClick.AddListener(OpenKeyPage);
        Fill(gkGO.GetComponent<RectTransform>(), V(0.54f, 0.607f), V(0.96f, 0.638f));
        var gkLblGO = new GameObject("_L");
        gkLblGO.transform.SetParent(gkGO.transform, false);
        _getKeyLabel               = gkLblGO.AddComponent<TextMeshProUGUI>();
        _getKeyLabel.fontSize      = 12;
        _getKeyLabel.fontStyle     = FontStyles.Underline;
        _getKeyLabel.color         = ColAccent;
        _getKeyLabel.alignment     = TextAlignmentOptions.MidlineRight;
        _getKeyLabel.raycastTarget = false;
        Fill(gkLblGO.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);

        // Key field — password-masked by default
        _keyField = Field(p, "Paste your API key here…", V(0.04f, 0.500f), V(0.96f, 0.601f));
        _keyField.contentType = TMP_InputField.ContentType.Password;
        _keyField.ForceLabelUpdate();

        // ── Model ─────────────────────────────────────────────────────────
        Lbl(p, "Model",
            V(0.04f, 0.456f), V(0.52f, 0.487f),
            12, ColTextDim, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);

        // Test Connection button
        var tGO  = new GameObject("_TestBtn");
        tGO.transform.SetParent(p, false);
        var tImg = tGO.AddComponent<Image>();
        tImg.color = ColTestBtn;
        _testBtn   = tGO.AddComponent<Button>();
        _testBtn.targetGraphic = tImg;
        var tC = _testBtn.colors;
        tC.normalColor      = ColTestBtn;
        tC.highlightedColor = ColTestBtn * 1.35f;
        tC.pressedColor     = ColTestBtn * 0.65f;
        tC.colorMultiplier  = 1f;
        _testBtn.colors = tC;
        _testBtn.onClick.AddListener(() => StartCoroutine(TestConnection()));
        Fill(tGO.GetComponent<RectTransform>(), V(0.55f, 0.456f), V(0.96f, 0.487f));
        var tlGO = new GameObject("_L");
        tlGO.transform.SetParent(tGO.transform, false);
        var tlT  = tlGO.AddComponent<TextMeshProUGUI>();
        tlT.text = "Test Connection"; tlT.fontSize = 12;
        tlT.fontStyle = FontStyles.Bold; tlT.color = Color.white;
        tlT.alignment = TextAlignmentOptions.Center; tlT.raycastTarget = false;
        Fill(tlGO.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);

        _modelField = Field(p, "e.g.  gemini-2.0-flash  /  gpt-4o-mini",
            V(0.04f, 0.350f), V(0.96f, 0.450f));

        // ── Test status ───────────────────────────────────────────────────
        _testStatus = Lbl(p, "", V(0.04f, 0.305f), V(0.96f, 0.342f), 12, Color.white);

        // ── Footer ────────────────────────────────────────────────────────
        Lbl(p,
            "<color=#3A4050>Config file: <color=#4A8EA8>" + GameConfig.ConfigPath +
            "</color>  —  you can edit it directly in any text editor.</color>",
            V(0.04f, 0.190f), V(0.96f, 0.298f), 11, ColTextDim);

        Img(p, "_S3", V(0.02f, 0.183f), V(0.98f, 0.188f), ColSep);

        // ── Action buttons ────────────────────────────────────────────────
        string dismissLabel = s_isFirstRun ? "Quit Game" : "Close";
        string saveLabel    = s_isFirstRun ? "Save & Launch" : "Save & Apply";

        Btn(p, dismissLabel, new Color(0.17f, 0.19f, 0.23f, 1f),
            V(0.04f, 0.068f), V(0.30f, 0.172f), 14, () => Proceed(false));

        Btn(p, saveLabel, new Color(0.11f, 0.40f, 0.25f, 1f),
            V(0.34f, 0.068f), V(0.96f, 0.172f), 15, () => Proceed(true));
    }

    // ── Preset ───────────────────────────────────────────────────────────────

    // Called by button clicks only — always writes to fields (Custom clears them).
    void OnPresetButtonClicked(int idx)
    {
        var preset = Presets[idx];
        if (_urlField   != null) _urlField.text   = preset.Url;
        if (_modelField != null) _modelField.text = preset.Model;
        HighlightPreset(idx);
        UpdateGetKeyLink(idx);
        if (_testStatus != null) _testStatus.text = "";
    }

    void HighlightPreset(int idx)
    {
        _selectedPreset = idx;
        for (int i = 0; i < _presetBtnImages.Length; i++)
            if (_presetBtnImages[i] != null)
                _presetBtnImages[i].color = (i == idx) ? ColSelected : ColUnselected;
    }

    void UpdateGetKeyLink(int idx)
    {
        if (_getKeyLabel == null) return;
        var p = Presets[idx];
        _getKeyLabel.text = p.KeyPageLabel;
        _getKeyLabel.gameObject.SetActive(!string.IsNullOrEmpty(p.KeyPageUrl));
    }

    void OpenKeyPage()
    {
        string url = Presets[_selectedPreset].KeyPageUrl;
        if (!string.IsNullOrEmpty(url)) Application.OpenURL(url);
    }

    // ── Status row ───────────────────────────────────────────────────────────
    void RefreshCurrentStatus()
    {
        if (_currentStatusLbl == null) return;

        bool   hasKey = GameConfig.Instance != null &&
                        !string.IsNullOrEmpty(GameConfig.Instance.Config.apiKey);
        string model  = GameConfig.Instance?.Config.model  ?? "";
        string url    = GameConfig.Instance?.Config.apiUrl ?? "";

        if (hasKey && !string.IsNullOrEmpty(model))
        {
            string host = ExtractHost(url);
            string via  = string.IsNullOrEmpty(host) ? "" : $"  ·  {host}";
            _currentStatusLbl.text  = $"<color=#{ToHex(ColGreen)}>●</color>  Active: {model}{via}";
            _currentStatusLbl.color = ColTextMain;
        }
        else if (hasKey)
        {
            _currentStatusLbl.text  = $"<color=#{ToHex(ColOrange)}>●</color>  API key set — model field is empty";
            _currentStatusLbl.color = ColTextDim;
        }
        else
        {
            _currentStatusLbl.text  = $"<color=#{ToHex(ColOrange)}>●</color>  Not configured — fill in the fields below and save";
            _currentStatusLbl.color = ColTextDim;
        }
    }

    // ── Show / hide key ───────────────────────────────────────────────────────
    void ToggleKeyVisibility()
    {
        _keyVisible = !_keyVisible;
        if (_keyField != null)
        {
            _keyField.contentType = _keyVisible
                ? TMP_InputField.ContentType.Standard
                : TMP_InputField.ContentType.Password;
            _keyField.ForceLabelUpdate();
        }
        if (_showHideLbl != null)
            _showHideLbl.text = _keyVisible ? "[ hide ]" : "[ show ]";
    }

    // ── Test Connection ───────────────────────────────────────────────────────
    IEnumerator TestConnection()
    {
        if (_isTesting) yield break;
        _isTesting = true;
        if (_testBtn != null) _testBtn.interactable = false;

        string url   = _urlField?.text?.Trim()   ?? "";
        string key   = _keyField?.text?.Trim()   ?? "";
        string model = _modelField?.text?.Trim() ?? "";

        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(model))
        {
            SetTestStatus("⚠  Fill in all three fields first.", ColOrange);
            Done(); yield break;
        }

        SetTestStatus("⏳  Connecting…", new Color(0.65f, 0.68f, 0.72f));

        var body = new
        {
            model    = model,
            messages = new[] { new { role = "user", content = "Reply with only the word: OK" } },
            max_tokens = 8
        };
        byte[] raw = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));

        var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(raw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type",  "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + key);
        req.timeout = 20;

        yield return req.SendWebRequest();

        // Capture result data before Dispose invalidates the download handler.
        bool   ok     = req.result == UnityWebRequest.Result.Success;
        string errMsg = ok ? null : ExtractApiError(req);
        req.Dispose();

        if (ok)
        {
            SetTestStatus("✓  Connected — API responded successfully.", ColGreen);
            // Reflect the tested (not-yet-saved) state in the status row
            if (_currentStatusLbl != null)
            {
                string m    = _modelField?.text?.Trim() ?? "";
                string host = ExtractHost(_urlField?.text?.Trim() ?? "");
                string via  = string.IsNullOrEmpty(host) ? "" : $"  ·  {host}";
                _currentStatusLbl.text  = $"<color=#{ToHex(ColGreen)}>●</color>  Tested OK: {m}{via}  (not saved yet)";
                _currentStatusLbl.color = ColTextMain;
            }
        }
        else
        {
            SetTestStatus("✗  " + errMsg, ColRed);
            if (_currentStatusLbl != null)
            {
                _currentStatusLbl.text  = $"<color=#{ToHex(ColRed)}>●</color>  Connection failed — check key / endpoint / model";
                _currentStatusLbl.color = ColTextDim;
            }
        }

        Done();
        void Done() { _isTesting = false; if (_testBtn != null) _testBtn.interactable = true; }
    }

    // Parses the API provider's own error message when present, falls back to req.error.
    static string ExtractApiError(UnityWebRequest req)
    {
        string body = req.downloadHandler?.text ?? "";
        if (!string.IsNullOrEmpty(body))
        {
            try
            {
                string msg = JObject.Parse(body)["error"]?["message"]?.ToString();
                if (!string.IsNullOrEmpty(msg))
                    return msg.Length > 120 ? msg.Substring(0, 118) + "…" : msg;
            }
            catch { }
        }
        string err = req.error ?? "Unknown error";
        return err.Length > 120 ? err.Substring(0, 118) + "…" : err;
    }

    void SetTestStatus(string text, Color color)
    {
        if (_testStatus == null) return;
        _testStatus.text  = text;
        _testStatus.color = color;
    }

    // ── Save & proceed ────────────────────────────────────────────────────────
    void Proceed(bool save)
    {
        if (save && GameConfig.Instance != null)
            GameConfig.Instance.SaveAll(
                _urlField?.text   ?? "",
                _keyField?.text   ?? "",
                _modelField?.text ?? "");

        bool quitOnDismiss = s_isFirstRun && !save;
        var  cb            = s_onComplete;
        s_onComplete = null;
        Destroy(gameObject);

        if (quitOnDismiss)
        {
            // First-run: no API key configured → can't play → exit.
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            return;
        }

        cb?.Invoke();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static string ExtractHost(string url)
    {
        try
        {
            string s = url;
            if (s.StartsWith("https://"))      s = s.Substring(8);
            else if (s.StartsWith("http://"))  s = s.Substring(7);
            int slash = s.IndexOf('/');
            return slash < 0 ? s : s.Substring(0, slash);
        }
        catch { return ""; }
    }

    static string ToHex(Color c) => ColorUtility.ToHtmlStringRGB(c);

    // ── UI primitives ─────────────────────────────────────────────────────────
    static Vector2 V(float x, float y) => new Vector2(x, y);

    static void Fill(RectTransform rt, Vector2 min, Vector2 max)
    {
        rt.anchorMin = min; rt.anchorMax = max;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void Img(Transform parent, string name, Vector2 min, Vector2 max, Color col)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = col;
        Fill(go.GetComponent<RectTransform>(), min, max);
    }

    static TextMeshProUGUI Lbl(Transform parent, string text,
        Vector2 min, Vector2 max, float size, Color color,
        FontStyles style = FontStyles.Normal,
        TextAlignmentOptions align = TextAlignmentOptions.TopLeft)
    {
        var go = new GameObject("_Lbl");
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size; t.color = color;
        t.fontStyle = style; t.alignment = align;
        t.enableWordWrapping = true; t.raycastTarget = false;
        Fill(go.GetComponent<RectTransform>(), min, max);
        return t;
    }

    // Input field with border outline.
    static TMP_InputField Field(Transform parent, string placeholder, Vector2 min, Vector2 max)
    {
        var border = new GameObject("_FieldBorder");
        border.transform.SetParent(parent, false);
        border.AddComponent<Image>().color = ColFieldBorder;
        var bRT = border.GetComponent<RectTransform>();
        bRT.anchorMin = min; bRT.anchorMax = max;
        bRT.offsetMin = new Vector2(-1, -1); bRT.offsetMax = new Vector2(1, 1);

        var root  = new GameObject("_Field");
        root.transform.SetParent(parent, false);
        root.AddComponent<Image>().color = ColFieldBg;
        var field = root.AddComponent<TMP_InputField>();
        Fill(root.GetComponent<RectTransform>(), min, max);

        var area = new GameObject("Text Area");
        area.transform.SetParent(root.transform, false);
        area.AddComponent<RectMask2D>();
        var aRT = area.GetComponent<RectTransform>();
        aRT.anchorMin = Vector2.zero; aRT.anchorMax = Vector2.one;
        aRT.offsetMin = new Vector2(10, 4); aRT.offsetMax = new Vector2(-10, -4);
        field.textViewport = aRT;

        var ph  = new GameObject("Placeholder");
        ph.transform.SetParent(area.transform, false);
        var phT = ph.AddComponent<TextMeshProUGUI>();
        phT.text = placeholder; phT.fontSize = 13; phT.fontStyle = FontStyles.Italic;
        phT.color = new Color(0.33f, 0.36f, 0.42f, 1f); phT.enableWordWrapping = false;
        Fill(ph.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        field.placeholder = phT;

        var txt  = new GameObject("Text");
        txt.transform.SetParent(area.transform, false);
        var txtT = txt.AddComponent<TextMeshProUGUI>();
        txtT.color = ColTextMain; txtT.fontSize = 13; txtT.enableWordWrapping = false;
        Fill(txt.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        field.textComponent = txtT;

        return field;
    }

    // Button with label child (correct Image→child TMP pattern).
    static Image Btn(Transform parent, string label, Color bg,
        Vector2 min, Vector2 max, float fontSize, Action onClick)
    {
        var go  = new GameObject("_Btn");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = bg;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var c = btn.colors;
        c.normalColor      = bg;
        c.highlightedColor = bg * 1.30f;
        c.pressedColor     = bg * 0.65f;
        c.colorMultiplier  = 1f;
        btn.colors = c;
        btn.onClick.AddListener(() => onClick());
        Fill(go.GetComponent<RectTransform>(), min, max);

        var lGO = new GameObject("_L");
        lGO.transform.SetParent(go.transform, false);
        var lT  = lGO.AddComponent<TextMeshProUGUI>();
        lT.text = label; lT.fontSize = fontSize; lT.fontStyle = FontStyles.Bold;
        lT.alignment = TextAlignmentOptions.Center; lT.color = Color.white;
        lT.raycastTarget = false;
        Fill(lGO.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        return img;
    }
}
