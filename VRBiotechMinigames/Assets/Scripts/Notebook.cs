using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// The notebook object that the player will have at all times.
/// Display updates to tasks and mistakes made as well as hold
/// setting functionality.
/// </summary>
/// <remarks>
/// Author: Jason Nguyen
/// Since: 2025-2-18
/// </remarks> 
public class Notebook : MonoBehaviour
{

    /// <summary>
    /// Controller button action to open and close the Notebook
    /// </summary>
    [SerializeField]
    private InputActionProperty menuButtonAction;

    /// <summary>
    /// The list of pages in the notebook
    /// </summary>
    [SerializeField]
    private GameObject[] pages;

    /// <summary>
    /// The list of buttons used to switch between pages
    /// </summary>
    [SerializeField]
    private Image[] pageButtons;

    /// <summary>
    /// The CanvasGroup used for adjusting the transparency when opening/closing the notebook
    /// </summary>
    [SerializeField]
    private CanvasGroup canvasGroup;

    /// <summary>
    /// Coroutine for opening/closing the notebook
    /// </summary>
    private Coroutine fadeRoutine;

    /// <summary>
    /// The index that is currently displayed from the tasks list
    /// </summary>
    private int displayedTaskIdx;

    /// <summary>
    /// The Text GameObject used to display the name of the task
    /// </summary>
    [SerializeField]
    private TMP_Text taskNameDisplay;

    /// <summary>
    /// The Text GameObject used to display the description of the task
    /// </summary>
    [SerializeField]
    private TMP_Text taskDescriptionDisplay;

    /// <summary>
    /// Displays a checkmark if the viewed task is complete
    /// </summary>
    [SerializeField]
    private Image taskCheckmark;

    /* MISTAKE FIELDS */
    /// <summary>
    /// Child Object of the Mistakes Page which contains all mistakes performed as children.
    /// </summary>
    [SerializeField] private GameObject mistakesContent;

    /// <summary>
    /// Holds all mistakes performed so far to filter out repeats
    /// </summary>
    private HashSet<int> mistakesKeys;


    /// <summary>
    /// On Start, set the Notebook to not be visible and connect the controller input
    /// </summary>
    public void Start()
    {
        gameObject.SetActive(false);
        canvasGroup.alpha = 0.0f;
        mistakesKeys = new HashSet<int>();

        menuButtonAction.action.performed += ToggleNotebook;
        if (pages.Length > 0)
        {
            SwitchToTab(0);
        }

        // Set the displayedTaskIdx to the start
        displayedTaskIdx = 0;
        DisplayTask(0);

    }

    public void OnDestroy()
    {
        menuButtonAction.action.performed -= ToggleNotebook;
    }

    /// <summary>
    /// Enable and Disable the Notebook's visibility
    /// </summary>
    /// <param name="callback">The data of the input performed (Not Used)</param>
    public void ToggleNotebook(InputAction.CallbackContext callback)
    {

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        if (gameObject.activeSelf == false)
        {
            SwitchToTab(0);
            gameObject.SetActive(true);

            fadeRoutine = StartCoroutine(FadeInNotebook());
        }
        else
        {
            fadeRoutine = StartCoroutine(FadeOutNotebook());
        }

    }

    public void SetVolume(Slider slider)
    {
        AudioListener.volume = slider.value;
    }

    public void PlayTutorial()
    {
        GameManager.getInstance().getTransitionmanager().GoToScene("TutorialRoom");
    }

    public void RestartLevel()
    {
        GameManager.getInstance().getTransitionmanager().GoToScene(SceneManager.GetActiveScene().name);
    }

    public void LeaveGame()
    {
        string selectionRoomName = "SelectionRoom";
        if (SceneManager.GetActiveScene().name.Equals(selectionRoomName))
        {

            // Does not work in Unity Editor; this is intended by Unity. As such, must be tested in an actual build
            Application.Quit();

        }
        else
        {

            GameManager.getInstance().getTransitionmanager().GoToScene(selectionRoomName);

        }
    }

    /// <summary>
    /// Switch the current Tab based on the index of the tab
    /// </summary>
    /// <param name="TabID">The index of the tab</param>
    public void SwitchToTab(int TabID)
    {
        foreach (GameObject page in pages)
        {
            page.SetActive(false);
        }

        //Update Tasks
        if (TabID == 0)
        {
            GameManager gameManager = GameManager.getInstance();
            TaskManager taskManager = gameManager.getTaskManager();
            TaskList taskList = taskManager.taskList;


            List<Task> tasks = taskList.getTasks();
            displayedTaskIdx = taskManager.GetCurrentTaskIdx();
            DisplayTask(displayedTaskIdx);
        }

        if (pages.Length > 0)
        {
            pages[TabID].SetActive(true);
        }

    }

    /// <summary>
    /// Update Notebook to display the previous task
    /// </summary>
    public void ViewPreviousTask()
    {
        if (displayedTaskIdx <= 0)
            return;
        displayedTaskIdx--;
        DisplayTask(displayedTaskIdx);


    }

    /// <summary>
    /// Update Notebook to display the next task
    /// </summary>
    public void ViewNextTask()
    {

        GameManager gameManager = GameManager.getInstance();
        TaskManager taskManager = gameManager.getTaskManager();
        TaskList taskList = taskManager.taskList;

        List<Task> tasks = taskList.getTasks();

        // Check that the index isn't larger than the length of the task list
        if (displayedTaskIdx >= tasks.Count)
            return;

        displayedTaskIdx++;
        DisplayTask(displayedTaskIdx);
    }

    // TODO Implement method when Taskmanager is implemented
    public void DisplayTask(int taskIndex)
    {
        if (taskIndex < 0)
            return;

        // Retrieve the taskList from the TaskManager
        GameManager gameManager = GameManager.getInstance();
        TaskManager taskManager = gameManager.getTaskManager();
        TaskList taskList = taskManager.taskList;

        List<Task> tasks = taskList.getTasks();

        string taskName = "";
        string taskDescription = "";

        // Task index is higher than number of tasks + Complete page, return
        if (taskIndex > tasks.Count)
            return;
        // Task index is equal to the size of the tasks (index is one higher than bounds), open unique complete minigame page
        else if (taskIndex > tasks.Count - 1)
        {
            // It will only display if we make it to the end of the minigame
            if (taskManager.GetCurrentTaskIdx() == taskIndex)
            {
                taskName = "Congratulations";
                taskDescription = "You've completed the minigame! Open the settings page in the notebook and click 'Leave Game' to return back to the Selection Room.";
            }
            else
            {
                return;
            }
        }
        // Task index is an appropriate index, get the information from the tasks list
        else
        {
            Task selectedTask = tasks[taskIndex];
            taskName = selectedTask.getName();
            taskDescription = selectedTask.getDescription();
        }

        displayedTaskIdx = taskIndex;
        taskNameDisplay.text = taskName;
        taskDescriptionDisplay.text = taskDescription;

        if (taskIndex < taskManager.GetCurrentTaskIdx())
        {
            taskCheckmark.enabled = true;
        }
        else
        {
            // Edge case, at the Minigame Complete Page in the Task Page
            if (taskIndex == tasks.Count)
            {
                taskCheckmark.enabled = true;
            }
            else
            {
                taskCheckmark.enabled = false;
            }
        }


    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mistakeArgs"></param>
    public void AddMistake(MistakeEventArgs mistakeArgs)
    {
        // If the mistake already happened, do not add
        if (mistakesKeys.Contains(mistakeArgs.getMessageKey()))
        {
            return;
        }
        mistakesKeys.Add(mistakeArgs.getMessageKey());
        GameObject textObject = new GameObject($"Alert {mistakesContent.transform.childCount}");
        TextMeshProUGUI tmpText = textObject.AddComponent<TextMeshProUGUI>();

        tmpText.text = MistakeEvent.getMistakeDescription(mistakeArgs.getMessageKey());

        tmpText.fontSize = 20;
        tmpText.transform.SetParent(mistakesContent.transform, false);

    }

    /// Fades in the alert UI.
    private IEnumerator FadeInNotebook()
    {
        gameObject.SetActive(true);
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.deltaTime * 4f;
            yield return null;
        }
    }


    /// Fades out the alert UI and hides it.
    private IEnumerator FadeOutNotebook()
    {
        if (gameObject == null) yield break;


        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.deltaTime * 4f;
            yield return null;
        }
        gameObject.SetActive(false);
    }

}
