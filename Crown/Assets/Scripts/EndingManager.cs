using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class EndingManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Image background;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI endingText;
    public Button returnButton;

    [Header("Ending Backgrounds")]
    public Sprite bgMob;
    public Sprite bgPyre;
    public Sprite bgSiege;
    public Sprite bgPoison;
    public Sprite bgSaint;
    public Sprite bgGeneral;
    public Sprite bgTower;
    public Sprite bgVictory;
    public Sprite bgDefault;

    void Start()
    {
        returnButton.onClick.AddListener(OnReturnClicked);
        string endingId = PlayerPrefs.GetString("EndingType", "last_word");
        ShowEnding(endingId);
        AudioManager.Instance.PlayEndingMusic();
    }

    void ShowEnding(string endingId)
    {
        switch (endingId)
        {
            case "unpaid_guard":
                titleText.text = "The Unpaid Guard";
                endingText.text = "The treasury runs dry. The guards have not been paid in weeks.\nTonight, they remember they are men before they are soldiers.\nThe throne room falls silent by dawn.";
                SetBackground(bgDefault);
                break;

            case "mob_verdict":
                titleText.text = "The Mob's Verdict";
                endingText.text = "They did not hate you. They were simply hungry.\nAnd hunger, left unanswered long enough, becomes something else entirely.\nThe palace gates did not hold.";
                SetBackground(bgMob);
                break;

            case "heretic_pyre":
                titleText.text = "The Heretic's Pyre";
                endingText.text = "The Church does not need evidence. It needs a symbol.\nYou were convenient.\nThe smoke rose for three days. The Regent wept publicly.";
                SetBackground(bgPyre);
                break;

            case "fallen_gates":
                titleText.text = "The Fallen Gates";
                endingText.text = "The border held for three hundred years.\nIt held for three hundred years, and then it did not.\nThe enemy king was not cruel. He was simply thorough.";
                SetBackground(bgSiege);
                break;

            case "golden_target":
                titleText.text = "The Golden Target";
                endingText.text = "Word of your treasury reached every court in the continent.\nThe neighboring king sent his warmest regards.\nAnd then he sent his army.";
                SetBackground(bgDefault);
                break;

            case "poisoned_cup":
                titleText.text = "The Poisoned Cup";
                endingText.text = "You were loved. Genuinely, dangerously loved.\nThe nobility could not allow that.\nThe wine at the banquet was excellent. You said so yourself, just before.";
                SetBackground(bgPoison);
                break;

            case "living_saint":
                titleText.text = "The Living Saint";
                endingText.text = "They called it an honor.\nThe finest cell in the monastery. Silk sheets. Three meals a day.\nThe Archbishop visited every Sunday. He always smiled.\nYou never left.";
                SetBackground(bgSaint);
                break;

            case "generals_crown":
                titleText.text = "The General's Crown";
                endingText.text = "He never wanted the throne, he said.\nHe only wanted order.\nThe crown fit him well, in the end.\nYou were not there to see it.";
                SetBackground(bgGeneral);
                break;

            case "the_tower":
                titleText.text = "The Tower";
                endingText.text = "He came in the night, as you knew he would.\n\"For your own safety,\" he said. He was always so considerate.\nThe tower has a fine view of the city.\nYou have had years to memorize it.";
                SetBackground(bgTower);
                break;

            case "last_word":
                titleText.text = "The Last Word";
                endingText.text = "Some words cannot be unsaid.\nYou learned this the hard way.\nThe court has a very long memory.";
                SetBackground(bgDefault);
                break;

            case "true_coronation":
                titleText.text = "The True Coronation";
                endingText.text = "You survived.\nNot through strength. Not through cunning alone.\nBut because you understood, at last, what the throne truly was:\nnot a seat of power, but a cage — and you had learned to make the cage work for you.\nThe Regent bowed. Slowly. For the first time, without a smile.";
                SetBackground(bgVictory);
                break;

            default:
                titleText.text = "The End";
                endingText.text = "Your reign has come to an end.";
                SetBackground(bgDefault);
                break;
        }
    }

    void SetBackground(Sprite sprite)
    {
        if (background && sprite != null)
            background.sprite = sprite;
    }

    void OnReturnClicked()
    {
        PlayerPrefs.DeleteKey("EndingType");
        SceneManager.LoadScene("MainMenu");
    }
}