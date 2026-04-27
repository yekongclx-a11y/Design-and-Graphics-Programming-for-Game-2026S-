using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
    public int maxEventsPerGame = 3;
    public int minRoundToTrigger = 4;
    public float earlyRoundChance = 0.25f;
    public float lateRoundChance = 0.15f;

    private int eventsTriggered = 0;
    private HashSet<string> triggeredEvents = new HashSet<string>();
    private System.Action onEventComplete;

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
    }

    private List<EventData> allEvents = new List<EventData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        InitializeEvents();
    }

    public void ResetEvents()
    {
        eventsTriggered = 0;
        triggeredEvents.Clear();
    }

    void InitializeEvents()
    {
        allEvents.Add(new EventData
        {
            eventId = "secret_orders",
            title = "Secret Orders",
            description = "The General sends a trusted aide with a private letter, inviting a secret meeting in the barracks after dark.",
            portraitKey = "general",
            hasChoices = true,
            choices = new EventChoice[]
            {
                new EventChoice { buttonText = "Accept the meeting", military = 15, suspicion = 20, affinityChange = 10, affinityTarget = "general" },
                new EventChoice { buttonText = "Decline", military = -5, affinityChange = -10, affinityTarget = "general" }
            }
        });

        allEvents.Add(new EventData
        {
            eventId = "corruption_evidence",
            title = "Evidence of Corruption",
            description = "You uncover proof that the Minister has been selling military grain. The evidence is damning.",
            portraitKey = "minister1",
            hasChoices = true,
            choices = new EventChoice[]
            {
                new EventChoice { buttonText = "Expose him publicly", popularity = 15, gold = -10, affinityChange = -20, affinityTarget = "minister" },
                new EventChoice { buttonText = "Use it as leverage", gold = 15, affinityChange = -10, affinityTarget = "minister", suspicion = 10 },
                new EventChoice { buttonText = "Ignore it", suspicion = -5, affinityChange = 5, affinityTarget = "minister" }
            }
        });

        allEvents.Add(new EventData
        {
            eventId = "midnight_confession",
            title = "Midnight Confession",
            description = "The Bishop holds a private blessing ceremony at midnight, hinting the Church can help conceal your private finances.",
            portraitKey = "bishop",
            hasChoices = true,
            choices = new EventChoice[]
            {
                new EventChoice { buttonText = "Accept the offer", church = 15, popularity = -10, gold = 10, affinityChange = 15, affinityTarget = "bishop" },
                new EventChoice { buttonText = "Decline gracefully", church = -5, popularity = 5, affinityChange = -5, affinityTarget = "bishop" }
            }
        });

        allEvents.Add(new EventData
        {
            eventId = "bloodstained_map",
            title = "The Bloodstained Map",
            description = "She bribes a guard to smuggle you a map of the palace's secret passages. The ink is still fresh.",
            portraitKey = "princess",
            hasChoices = true,
            choices = new EventChoice[]
            {
                new EventChoice { buttonText = "Accept the map", suspicion = 15, affinityChange = 10, affinityTarget = "princess" },
                new EventChoice { buttonText = "Burn it", suspicion = -5, affinityChange = -5, affinityTarget = "princess" }
            }
        });

        allEvents.Add(new EventData
        {
            eventId = "desperate_handmaid",
            title = "The Desperate Handmaid",
            description = "Your handmaid, whose family perished under your policies, slips a sedative into your wine. She is caught before you drink.",
            portraitKey = "handmaid",
            hasChoices = true,
            choices = new EventChoice[]
            {
                new EventChoice { buttonText = "Forgive her", popularity = 20, military = -5, affinityChange = 15, affinityTarget = "commoner" },
                new EventChoice { buttonText = "Punish her", popularity = -15, military = 5, church = -5, affinityChange = -10, affinityTarget = "commoner" }
            }
        });

        allEvents.Add(new EventData
        {
            eventId = "deserting_knight",
            title = "The Deserting Knight",
            description = "A knight who once served your father lays down his sword at your feet and refuses to serve further.",
            portraitKey = "knight",
            hasChoices = true,
            choices = new EventChoice[]
            {
                new EventChoice { buttonText = "Let him go", military = -10, popularity = 5, affinityChange = 5, affinityTarget = "general" },
                new EventChoice { buttonText = "Detain him", military = -5, popularity = -10, suspicion = 5, affinityChange = -15, affinityTarget = "general" }
            }
        });

        allEvents.Add(new EventData
        {
            eventId = "power_vacuum",
            title = "Power Vacuum",
            description = "The Regent falls gravely ill and is absent from court for three days. The throne room feels different without his shadow.",
            portraitKey = "",
            hasChoices = true,
            choices = new EventChoice[]
            {
                new EventChoice { buttonText = "Act boldly", suspicion = 15 },
                new EventChoice { buttonText = "Stay cautious", suspicion = -10 }
            }
        });

        allEvents.Add(new EventData
        {
            eventId = "deadly_compliment",
            title = "The Deadly Compliment",
            description = "The Regent publicly praises your wisdom and readiness to govern alone — in front of the entire court. Everyone is watching your reaction.",
            portraitKey = "regent",
            hasChoices = true,
            choices = new EventChoice[]
            {
                new EventChoice { buttonText = "Accept humbly", suspicion = -10, popularity = 5 },
                new EventChoice { buttonText = "Respond proudly", suspicion = 20, popularity = 10 }
            }
        });

        allEvents.Add(new EventData
        {
            eventId = "regents_gift",
            title = "The Regent's Gift",
            description = "The Regent sends you a songbird with its tongue cut out. A servant delivers it with a smile and no explanation.",
            portraitKey = "regent",
            hasChoices = true,
            choices = new EventChoice[]
            {
                new EventChoice { buttonText = "Send thanks", suspicion = 5, popularity = -5 },
                new EventChoice { buttonText = "Return it", suspicion = 20, popularity = 10 },
                new EventChoice { buttonText = "Say nothing", suspicion = 10 }
            }
        });

        allEvents.Add(new EventData
        {
            eventId = "empty_throne",
            title = "The Empty Throne",
            description = "The Regent skips today's session. His chair sits empty at the head of the council table. No one mentions it. No one looks at it directly.",
            portraitKey = "",
            hasChoices = false,
            directSuspicion = 10
        });

        allEvents.Add(new EventData
        {
            eventId = "blood_on_walls",
            title = "Blood on the Walls",
            description = "The words \"FALSE KING MUST DIE\" appear carved into a throne hall pillar. No one claims responsibility.",
            portraitKey = "",
            hasChoices = false,
            directPopularity = -10,
            directSuspicion = 10
        });

        allEvents.Add(new EventData
        {
            eventId = "spy_in_kitchen",
            title = "The Spy in the Kitchen",
            description = "The royal chef is revealed to be a foreign agent who has been embedded for ten years. He is arrested before he can flee.",
            portraitKey = "servant",
            hasChoices = true,
            choices = new EventChoice[]
            {
                new EventChoice { buttonText = "Execute him", military = 5, church = -10, popularity = -5 },
                new EventChoice { buttonText = "Interrogate him", suspicion = -5 }
            }
        });

        allEvents.Add(new EventData
        {
            eventId = "broken_string",
            title = "The Broken String",
            description = "A bard's string snaps mid-performance during a court gathering — considered a grave omen. The court falls silent.",
            portraitKey = "",
            hasChoices = false,
            directGold = Random.Range(-5, 6),
            directPopularity = Random.Range(-5, 6),
            directChurch = Random.Range(-5, 6),
            directMilitary = Random.Range(-5, 6)
        });

        allEvents.Add(new EventData
        {
            eventId = "midnight_footsteps",
            title = "Midnight Footsteps",
            description = "Strange footsteps echo through the corridor outside your chambers at night. By morning, nothing is found.",
            portraitKey = "",
            hasChoices = false,
            directSuspicion = 10
        });

        allEvents.Add(new EventData
        {
            eventId = "great_fire",
            title = "The Great Fire",
            description = "The slum district near the palace is burning. Thousands are displaced. The court awaits your decision.",
            portraitKey = "",
            hasChoices = true,
            choices = new EventChoice[]
            {
                new EventChoice { buttonText = "Send aid", popularity = 20, gold = -15, military = -5 },
                new EventChoice { buttonText = "Ignore it", popularity = -20, church = -10 },
                new EventChoice { buttonText = "Blame rebels", popularity = -10, military = 10, suspicion = 10 }
            }
        });

        allEvents.Add(new EventData
        {
            eventId = "foreign_envoy",
            title = "The Foreign Envoy",
            description = "A neighboring kingdom proposes a marriage alliance — actually a veiled annexation attempt. The envoy is charming and persistent.",
            portraitKey = "nobleFemale",
            hasChoices = true,
            choices = new EventChoice[]
            {
                new EventChoice { buttonText = "Accept the alliance", military = 15, popularity = -10, suspicion = 15 },
                new EventChoice { buttonText = "Refuse politely", popularity = 10, military = -5 },
                new EventChoice { buttonText = "Stall for time", suspicion = 5 }
            }
        });

        allEvents.Add(new EventData
        {
            eventId = "assassin",
            title = "The Assassin",
            description = "You are attacked in the corridor by a hooded figure. Your guards intervene, but not before the assassin delivers a message.",
            portraitKey = "assassin",
            hasChoices = true,
            choices = new EventChoice[]
            {
                new EventChoice { buttonText = "Investigate publicly", popularity = 10, suspicion = 15 },
                new EventChoice { buttonText = "Handle quietly", suspicion = 5, military = 5 }
            }
        });

        allEvents.Add(new EventData
        {
            eventId = "blizzard",
            title = "The Blizzard",
            description = "Supply lines are cut by an unexpected blizzard. Both soldiers and civilians begin fighting over the remaining food stores.",
            portraitKey = "",
            hasChoices = true,
            choices = new EventChoice[]
            {
                new EventChoice { buttonText = "Feed the army", military = 10, popularity = -15 },
                new EventChoice { buttonText = "Feed the people", popularity = 15, military = -10 }
            }
        });

        allEvents.Add(new EventData
        {
            eventId = "rumour",
            title = "The Rumour",
            description = "Word spreads through the court that you are not the true heir — perhaps the Regent's illegitimate son, placed conveniently on the throne.",
            portraitKey = "",
            hasChoices = false,
            directPopularity = -15,
            directSuspicion = 15
        });

        allEvents.Add(new EventData
        {
            eventId = "plague",
            title = "The Plague",
            description = "Disease spreads from the lower city. The court physicians disagree on the cause. The people are afraid.",
            portraitKey = "",
            hasChoices = true,
            choices = new EventChoice[]
            {
                new EventChoice { buttonText = "Quarantine the district", popularity = -20, church = 10, military = -5 },
                new EventChoice { buttonText = "Ignore it", popularity = -10, military = -10 },
                new EventChoice { buttonText = "Burn the district", popularity = -25, military = 5, church = -20 }
            }
        });

        allEvents.Add(new EventData
        {
            eventId = "blind_oracle",
            title = "The Blind Oracle",
            description = "A blind elder is brought before you, claiming to have foreseen your fate. The court watches nervously.",
            portraitKey = "oracle",
            hasChoices = true,
            choices = new EventChoice[]
            {
                new EventChoice { buttonText = "Listen to the prophecy", suspicion = 0 },
                new EventChoice { buttonText = "Dismiss him", popularity = -5, church = -10 }
            }
        });

        allEvents.Add(new EventData
        {
            eventId = "unpaid_wages",
            title = "Unpaid Wages",
            description = "Soldiers stationed outside the city threaten to mutiny if they are not paid within the day. The treasury is nearly empty.",
            portraitKey = "",
            hasChoices = true,
            choices = new EventChoice[]
            {
                new EventChoice { buttonText = "Pay immediately", gold = -20, military = 15 },
                new EventChoice { buttonText = "Promise to pay later", military = -10, suspicion = 10 }
            }
        });
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
            selectedEvent = SelectEvent();

        if (selectedEvent == null) return false;

        onEventComplete = onComplete;
        TriggerEvent(selectedEvent);
        Debug.Log($"Event triggered: {selectedEvent.eventId}");
        return true;
    }

    EventData SelectEvent()
    {
        GameStateManager gs = GameStateManager.Instance;
        List<EventData> available = new List<EventData>();
        List<float> weights = new List<float>();

        foreach (var evt in allEvents)
        {
            if (triggeredEvents.Contains(evt.eventId)) continue;
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
        eventTitle.text = evt.title;
        eventDescription.text = evt.description;

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
            case "general": gs.affinityGeneral = Mathf.Clamp(gs.affinityGeneral + change, 0, 100); break;
            case "bishop": gs.affinityBishop = Mathf.Clamp(gs.affinityBishop + change, 0, 100); break;
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
            eventPortrait.color = Color.clear;
        }
    }

    Sprite GetPortrait(string key)
    {
        switch (key)
        {
            case "minister1": return portraitMinister1;
            case "minister2": return portraitMinister2;
            case "general": return portraitGeneral;
            case "bishop": return portraitBishop;
            case "princess": return portraitPrincess;
            case "commoner": return portraitCommoner;
            case "regent": return portraitRegent;
            case "handmaid": return portraitHandmaid;
            case "knight": return portraitKnight;
            case "assassin": return portraitAssassin;
            case "oracle": return portraitOracle;
            case "nobleFemale": return portraitNobleFemale;
            case "servant": return portraitServant;
            default: return null;
        }
    }

    void CloseEvent()
    {
        eventPanel.SetActive(false);
        onEventComplete?.Invoke();
    }
}