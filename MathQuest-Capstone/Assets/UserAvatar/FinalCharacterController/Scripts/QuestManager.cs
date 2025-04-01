using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    // References to UI elements
    public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI questDescText;
    public TextMeshProUGUI questObjTitleText;
    public TextMeshProUGUI questObjContentText;
    public TextMeshProUGUI questRewTitleText;
    public TextMeshProUGUI questRewContentText;
    public TextMeshProUGUI questLossesTitleText;
    public TextMeshProUGUI questLossesContentText;

    // Button references
    public Button nextButton;
    public Button previousButton;

    // Quest data array
    public Quest[] quests;

    private int currentQuestIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Prevents destruction when changing scenes
        }
        else
        {
            Destroy(gameObject);  // Ensures only one instance of QuestManager exists
        }
    }

    private void Start()
    {
        // Reset the quest index when the scene is loaded (in case it's not the first time)
        if (SceneManager.GetActiveScene().name == "QuestListScene")
        {
            currentQuestIndex = 0;
        }

        // Set the UI to display the first quest
        ShowQuest(currentQuestIndex);

        // Set up button listeners
        nextButton.onClick.AddListener(ShowNextQuest);
        previousButton.onClick.AddListener(ShowPreviousQuest);
    }

    private void ShowQuest(int index)
    {
        if (index < 0 || index >= quests.Length)
            return;

        Quest currentQuest = quests[index];

        questTitleText.text = currentQuest.questName;
        questDescText.text = currentQuest.description;
        questObjTitleText.text = "Objectives";
        questObjContentText.text = currentQuest.objectives;
        questRewTitleText.text = "Rewards";
        questRewContentText.text = currentQuest.rewards;
        questLossesTitleText.text = "Lives Lost";
        questLossesContentText.text = currentQuest.livesLost;
    }
    private void ShowNextQuest()
    {
        // Move to the next quest if possible
        if (currentQuestIndex < quests.Length - 1)
        {
            currentQuestIndex++;
            ShowQuest(currentQuestIndex);
        }
    }

    private void ShowPreviousQuest()
    {
        // Move to the previous quest if possible
        if (currentQuestIndex > 0)
        {
            currentQuestIndex--;
            ShowQuest(currentQuestIndex);
        }

    }
}