using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Prefabs and Container")]
    // A prefab for a quest panel. This prefab should be set up with child objects named:
    // "QuestTitle", "QuestDesc", "QuestObjectives", "QuestRewards", "QuestLivesLost".
    public GameObject questPanelPrefab;
    // Parent container in the Canvas where quest panels will be instantiated.
    public Transform panelContainer;

    [Header("Transition Component")]
    public Quest3DTransition quest3DTransition;

    [Header("Navigation Buttons")]
    public Button nextButton;
    public Button previousButton;

    [Header("Quest Data")]
    public Quest[] quests;

    // List of instantiated quest panels (their RectTransforms)
    private List<RectTransform> questPanels = new List<RectTransform>();

    // The index of the currently visible quest panel
    private int currentQuestIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes.
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Instantiate a quest panel for each Quest in the dataset.
        // (Make sure your questPanelPrefab is not active in the scene if it's used as a template.)
        foreach (Quest quest in quests)
        {
            GameObject panelObj = Instantiate(questPanelPrefab, panelContainer);
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();

            // For the first panel, set it on-screen; for others, place them off-screen.
            if (questPanels.Count == 0)
            {
                panelRect.localPosition = new Vector3(0f, -37f, 0f); // on-screen position
            }
            else
            {
                // Set other panels to the default incoming position for Next transitions.
                panelRect.localPosition = new Vector3(400f, -37f, 200f);
            }

            panelObj.SetActive(true); // Ensure they are visible.
            questPanels.Add(panelRect);

            // Update panel UI with quest data.
            UpdatePanelUI(panelRect, quest);
        }

        // All panels remain active now. You can adjust their sibling order if needed.
        // Optionally, bring the current panel to front:
        questPanels[currentQuestIndex].SetAsLastSibling();

        // Set up button listeners.
        nextButton.onClick.AddListener(AnimateToNextQuest);
        previousButton.onClick.AddListener(AnimateToPreviousQuest);
    }

    // Updates the UI elements on a given panel with data from a Quest.
    private void UpdatePanelUI(RectTransform panel, Quest quest)
    {
        TextMeshProUGUI title = panel.Find("QuestTitle").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI desc = panel.Find("QuestDesc").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI objectives = panel.Find("QuestObjectives").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI rewards = panel.Find("QuestRewards").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI livesLost = panel.Find("Losses").GetComponent<TextMeshProUGUI>();

        title.text = quest.questName;
        desc.text = quest.description;
        objectives.text = quest.objectives;
        rewards.text = quest.rewards;
        livesLost.text = quest.livesLost;
    }

    // Called when the Next button is clicked.
    private void AnimateToNextQuest()
    {
        if (currentQuestIndex < questPanels.Count - 1)
        {
            int targetIndex = currentQuestIndex + 1;

            // Ensure the target panel is active (it should be, since we instantiated all).
            // Bring the target panel to front (optional: adjust sibling order for layering).
            questPanels[targetIndex].SetAsLastSibling();

            // Animate from the current panel to the target panel using the "Next" settings.
            quest3DTransition.TransitionBetweenPanels(questPanels[currentQuestIndex], questPanels[targetIndex], true, () =>
            {
                // After the transition, update currentQuestIndex.
                currentQuestIndex = targetIndex;
                // Optionally, adjust the sibling order to bring current to the front.
                questPanels[currentQuestIndex].SetAsLastSibling();
            });
        }
    }

    // Called when the Previous button is clicked.
    private void AnimateToPreviousQuest()
    {
        if (currentQuestIndex > 0)
        {
            int targetIndex = currentQuestIndex - 1;

            // Bring the target panel to front.
            questPanels[targetIndex].SetAsLastSibling();

            // Animate from the current panel to the target panel using the "Previous" settings.
            quest3DTransition.TransitionBetweenPanels(questPanels[currentQuestIndex], questPanels[targetIndex], false, () =>
            {
                currentQuestIndex = targetIndex;
                questPanels[currentQuestIndex].SetAsLastSibling();
            });
        }
    }
}
