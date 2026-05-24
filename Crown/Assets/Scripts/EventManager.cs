using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    [Header("Event Panel")]
    public GameObject eventPanel;
    public Image eventPortrait;
    public TextMeshProUGUI eventTitle;
    public TextMeshProUGUI eventDescription;
    public Button choiceButton1;
    public Button choiceButton2;
    public Button choiceButton3;
    public Button continueButton;
    public TextMeshProUGUI choiceText1;
    public TextMeshProUGUI choiceText2;
    public TextMeshProUGUI choiceText3;

    [Header("NPC Portraits")]
    public Sprite portraitMinister1;
    public Sprite portraitMinister2;
    public Sprite portraitGeneral;
    public Sprite portraitBishop;
    public Sprite portraitPrincess;
    public Sprite portraitCommoner;
    public Sprite portraitRegent;
    public Sprite portraitHandmaid;
    public Sprite portraitKnight;
    public Sprite portraitAssassin;
    public Sprite portraitOracle;
    public Sprite portraitNobleFemale;
    public Sprite portraitServant;

    [Header("Trigger Settings")]
    public int maxEventsPerGame = 8;
    public int minRoundToTrigger = 1;
    public float earlyRoundChance = 0.6f;
    public float lateRoundChance = 0.9f;

    private int eventsTriggered = 0;
    private HashSet<string> triggeredEvents = new HashSet<string>();
    private System.Action onEventComplete;

    [System.Serializable]
    public class EventConditions
    {
        public int minSuspicion = 0;
    }

    [System.Serializable]
    public class EventChoice
    {
        public string buttonText;
        public int gold;
        public int popularity;
        public int church;
        public int military;
        public int suspicion;
        public int affinityChange;
        public string affinityTarget;
    }

    [System.Serializable]
    public class EventData
    {
        public string eventId;
        public string title;
        public string description;
        public string portraitKey;
        public bool hasChoices;
        public EventChoice[] choices;
        public int directGold;
        public int directPopularity;
        public int directChurch;
        public int directMilitary;
        public int directSuspicion;
        public EventConditions conditions;
        public bool randomizeDirect;
    }

    private List<EventData> allEvents = new List<EventData>();

    private GameObject eventContent;

    void Awake()
    {
        Instance = this;
        LoadEventsFromJson();
        if (eventPanel != null)
        {
            Transform t = eventPanel.transform.Find("EventContent");
            if (t != null) eventContent = t.gameObject;
        }
        AutoWireButtonTexts();
    }

    void AutoWireButtonTexts()
    {
        TryWire(choiceButton1, ref choiceText1);
        TryWire(choiceButton2, ref choiceText2);
        TryWire(choiceButton3, ref choiceText3);
        AutoWireEventPanelTexts();
    }

    void TryWire(Button btn, ref TextMeshProUGUI label)
    {
        if (btn == null) return;
        // includeInactive:true — panel may be disabled at Awake time
        if (label == null)
            label = btn.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label == null) return;

        label.enableAutoSizing = true;
        label.fontSizeMin = 10;
        label.fontSizeMax = 24;
        label.enableWordWrapping = true;
    }

    void AutoWireEventPanelTexts()
    {
        if (eventPanel == null) return;
        if (eventTitle != null && eventDescription != null) return;

        // Collect all TMP texts in the panel, excluding button labels already wired
        var buttonLabels = new HashSet<TextMeshProUGUI> { choiceText1, choiceText2, choiceText3 };
        var panelTexts = new List<TextMeshProUGUI>();
        foreach (var t in eventPanel.GetComponentsInChildren<TextMeshProUGUI>(true))
            if (!buttonLabels.Contains(t)) panelTexts.Add(t);

        if (eventTitle == null && panelTexts.Count > 0)
        {
            eventTitle = panelTexts[0];
            Debug.Log("[EventManager] Auto-wired eventTitle: " + eventTitle.gameObject.name);
        }
        if (eventDescription == null && panelTexts.Count > 1)
        {
            eventDescription = panelTexts[1];
            Debug.Log("[EventManager] Auto-wired eventDescription: " + eventDescription.gameObject.name);
        }
    }

    void LoadEventsFromJson()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "events.json");
        if (!File.Exists(path))
        {
            Debug.LogError("[EventManager] events.json not found: " + path);
            return;
        }
        string json = File.ReadAllText(path, Encoding.UTF8);
        allEvents = JsonConvert.DeserializeObject<List<EventData>>(json);
        Debug.Log($"[EventManager] Loaded {allEvents.Count} events from events.json.");
    }

    public void ResetEvents()
    {
        eventsTriggered = 0;
        triggeredEvents.Clear();
    }

    public bool TryTriggerEvent(int currentRound, System.Action onComplete)
    {
        if (eventsTriggered >= maxEventsPerGame) return false;

        bool forceTrigger = (currentRound == 4 || currentRound == 8);
        bool suspicionTrigger = GameStateManager.Instance.suspicion >= 60
            && !triggeredEvents.Contains("deadly_compliment");

        if (!forceTrigger && !suspicionTrigger)
        {
            if (currentRound < minRoundToTrigger) return false;
            float chance = currentRound <= 8 ? earlyRoundChance : lateRoundChance;
            if (Random.value > chance) return false;
        }

        EventData selectedEvent = null;

        if (suspicionTrigger)
        {
            selectedEvent = allEvents.Find(e => e.eventId == "deadly_compliment"
                && !triggeredEvents.Contains(e.eventId));
            if (selectedEvent == null)
                selectedEvent = allEvents.Find(e => e.eventId == "regents_gift"
                    && !triggeredEvents.Contains(e.eventId));
        }
        else if (forceTrigger && currentRound == 4)
        {
            selectedEvent = allEvents.Find(e => e.eventId == "power_vacuum"
                && !triggeredEvents.Contains(e.eventId));
        }
        else if (forceTrigger && currentRound == 8)
        {
            selectedEvent = allEvents.Find(e => e.eventId == "blizzard"
                && !triggeredEvents.Contains(e.eventId));
        }

        if (selectedEvent == null)
            selectedEvent = SelectEvent(currentRound);

        if (selectedEvent == null) return false;

        onEventComplete = onComplete;
        TriggerEvent(selectedEvent);
        Debug.Log($"[EventManager] Event triggered: {selectedEvent.eventId}");
        return true;
    }

    EventData SelectEvent(int currentRound)
    {
        GameStateManager gs = GameStateManager.Instance;
        List<EventData> available = new List<EventData>();
        List<float> weights = new List<float>();

        foreach (var evt in allEvents)
        {
            if (triggeredEvents.Contains(evt.eventId)) continue;

            if (evt.conditions != null && gs.suspicion < evt.conditions.minSuspicion)
                continue;

            available.Add(evt);

            float weight = 1f;

            bool dangerZone =
                gs.gold <= 20 || gs.gold >= 80 ||
                gs.popularity <= 20 || gs.popularity >= 80 ||
                gs.church <= 20 || gs.church >= 80 ||
                gs.military <= 20 || gs.military >= 80;

            if (dangerZone) weight *= 1.5f;

            bool affinityExtreme =
                gs.affinityMinister <= 30 || gs.affinityMinister >= 80 ||
                gs.affinityGeneral <= 30 || gs.affinityGeneral >= 80 ||
                gs.affinityBishop <= 30 || gs.affinityBishop >= 80 ||
                gs.affinityPrincess <= 30 || gs.affinityPrincess >= 80 ||
                gs.affinityCommoner <= 30 || gs.affinityCommoner >= 80;

            if (affinityExtreme) weight *= 1.5f;

            weights.Add(weight);
        }

        if (available.Count == 0) return null;

        float totalWeight = 0f;
        foreach (var w in weights) totalWeight += w;

        float roll = Random.value * totalWeight;
        float cumulative = 0f;
        for (int i = 0; i < available.Count; i++)
        {
            cumulative += weights[i];
            if (roll <= cumulative) return available[i];
        }

        return available[available.Count - 1];
    }

    void TriggerEvent(EventData evt)
    {
        triggeredEvents.Add(evt.eventId);
        eventsTriggered++;

        eventPanel.SetActive(true);
        if (eventContent != null) eventContent.SetActive(true);
        if (eventTitle != null) eventTitle.text = evt.title;
        else Debug.LogError("[EventManager] eventTitle is null — connect it in Inspector or check panel hierarchy.");
        if (eventDescription != null) eventDescription.text = evt.description;
        else Debug.LogError("[EventManager] eventDescription is null — connect it in Inspector or check panel hierarchy.");

        SetPortrait(evt.portraitKey);

        choiceButton1.gameObject.SetActive(false);
        choiceButton2.gameObject.SetActive(false);
        choiceButton3.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);

        if (evt.hasChoices)
        {
            SetupChoiceButton(choiceButton1, choiceText1, evt.choices[0]);
            choiceButton1.gameObject.SetActive(true);

            if (evt.choices.Length > 1)
            {
                SetupChoiceButton(choiceButton2, choiceText2, evt.choices[1]);
                choiceButton2.gameObject.SetActive(true);
            }

            if (evt.choices.Length > 2)
            {
                SetupChoiceButton(choiceButton3, choiceText3, evt.choices[2]);
                choiceButton3.gameObject.SetActive(true);
            }
        }
        else
        {
            if (evt.randomizeDirect)
            {
                evt.directGold = Random.Range(-5, 6);
                evt.directPopularity = Random.Range(-5, 6);
                evt.directChurch = Random.Range(-5, 6);
                evt.directMilitary = Random.Range(-5, 6);
            }

            GameStateManager.Instance.UpdateResources(
                evt.directGold, evt.directPopularity,
                evt.directChurch, evt.directMilitary,
                evt.directSuspicion
            );
            UIManager.Instance.UpdateResourceBars();

            continueButton.gameObject.SetActive(true);
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => CloseEvent());
        }
    }

    void SetupChoiceButton(Button btn, TextMeshProUGUI label, EventChoice choice)
    {
        label.text = choice.buttonText;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => OnChoiceSelected(choice));
    }

    void OnChoiceSelected(EventChoice choice)
    {
        GameStateManager gs = GameStateManager.Instance;
        gs.UpdateResources(choice.gold, choice.popularity,
                           choice.church, choice.military,
                           choice.suspicion);

        UpdateAffinity(choice.affinityTarget, choice.affinityChange);
        UIManager.Instance.UpdateResourceBars();
        CloseEvent();
    }

    void UpdateAffinity(string target, int change)
    {
        if (string.IsNullOrEmpty(target) || change == 0) return;
        GameStateManager gs = GameStateManager.Instance;
        switch (target)
        {
            case "minister": gs.affinityMinister = Mathf.Clamp(gs.affinityMinister + change, 0, 100); break;
            case "general":  gs.affinityGeneral  = Mathf.Clamp(gs.affinityGeneral  + change, 0, 100); break;
            case "bishop":   gs.affinityBishop   = Mathf.Clamp(gs.affinityBishop   + change, 0, 100); break;
            case "princess": gs.affinityPrincess = Mathf.Clamp(gs.affinityPrincess + change, 0, 100); break;
            case "commoner": gs.affinityCommoner = Mathf.Clamp(gs.affinityCommoner + change, 0, 100); break;
        }
    }

    void SetPortrait(string key)
    {
        if (eventPortrait == null) return;
        Sprite sprite = GetPortrait(key);
        if (sprite != null)
        {
            eventPortrait.sprite = sprite;
            eventPortrait.color = Color.white;
        }
        else
        {
            eventPortrait.sprite = null;
            eventPortrait.color = Color.clear;
        }
    }

    Sprite GetPortrait(string key)
    {
        switch (key)
        {
            case "minister1":   return portraitMinister1;
            case "minister2":   return portraitMinister2;
            case "general":     return portraitGeneral;
            case "bishop":      return portraitBishop;
            case "princess":    return portraitPrincess;
            case "commoner":    return portraitCommoner;
            case "regent":      return portraitRegent;
            case "handmaid":    return portraitHandmaid;
            case "knight":      return portraitKnight;
            case "assassin":    return portraitAssassin;
            case "oracle":      return portraitOracle;
            case "nobleFemale": return portraitNobleFemale;
            case "servant":     return portraitServant;
            default:            return null;
        }
    }

    void CloseEvent()
    {
        if (eventContent != null) eventContent.SetActive(false);
        eventPanel.SetActive(false);
        onEventComplete?.Invoke();
    }
}
