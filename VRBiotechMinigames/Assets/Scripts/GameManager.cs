using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Scene = UnityEngine.SceneManagement.Scene;
using Random = System.Random;
using Int32 = System.Int32;

/**
 * 
 * @author Colby Cress
 * 
 * Singleton object that provides access to managers of functionality within the game.
 * 
 */
public class GameManager : MonoBehaviour
{

	// Singleton instance of GameManager
	private static GameManager instance;
	private TaskManager taskManager;
	private AlertManager alertManager;
	private HandManager handManager;
	private TransitionManager transitionManager;

	[SerializeField]
	private InputActionAsset inputActions;

	[SerializeField]
	private TaskList taskList;

	private void initiateChildManagers(Scene scene, LoadSceneMode mode) {

		// Reset the children managers
		ResetManagers();

		GameObject taskManagerObject = new GameObject();
		taskManagerObject.transform.parent = this.transform;
		taskManagerObject.name = "TaskManager";
		taskManagerObject.AddComponent<TaskManager>();
		taskManager = taskManagerObject.GetComponent<TaskManager>();
		taskManager.taskList = taskList;

		GameObject alertManagerObject = new GameObject();
		alertManagerObject.transform.parent = this.transform;
		alertManagerObject.name = "AlertManager";
		alertManagerObject.AddComponent<AlertManager>();
		alertManager = alertManagerObject.GetComponent<AlertManager>();
		Notebook notebook = Object.FindObjectOfType<Notebook>();
		alertManager.notebook = notebook;

		GameObject handManagerObject = new GameObject();
		handManagerObject.transform.parent = this.transform;
		handManagerObject.name = "HandManager";
		handManagerObject.AddComponent<HandManager>();
		handManager = handManagerObject.GetComponent<HandManager>();
		handManager.setInputActions(inputActions);
		handManager.setup();

        GameObject transitionManagerObject = new GameObject();
		transitionManagerObject.transform.parent = this.transform;
		transitionManagerObject.name = "TransitionManager";
		transitionManagerObject.AddComponent<TransitionManager>();

		transitionManager = transitionManagerObject.GetComponent<TransitionManager>();
		GameObject fadeScreenObj = Instantiate(Resources.Load("FadeScreen", typeof(GameObject))) as GameObject;
		GameObject mainCamera = GameObject.FindGameObjectsWithTag("MainCamera")[0];
        transitionManager.Setup(mainCamera, fadeScreenObj);
	}

	// Sets values for tasks, such as a goal amount for aspirate tasks
	private void defineTaskValues() {

		// Create a shallow copy of the task list so that the original is not modified
		TaskList oldTaskList = this.taskList;
		this.taskList = new TaskList();
		this.taskList.tasks = new List<Task>(oldTaskList.tasks);

		float rangeValue = 1f;
		string liquidType = "red";

		// Go through each task, replacing DEFINE_RANGE() and DEFINE_LIQUID with actual values
		int numTasks = taskList.tasks.Count;
		for (int i = 0; i < numTasks; i++) {

			Task task = taskList.tasks[i];

			// Pattern to find the liquid amount in the task description, defined as DEFINE_RANGE(some number, some number)
			string pattern = @"DEFINE_RANGE\(\s*([\-]?\d+(?:\.\d+)?)\s*,\s*([\-]?\d+(?:\.\d+)?)\s*\)";

			// Find the pattern in the task description
			MatchCollection matches = Regex.Matches(task.getDescription(), pattern);

			Match match = null;
			if (matches.Count > 0) {

				// Get the first match - realistically there should only be one anyways
				match = matches[0];

			}

			Random random = new Random();

			if (match != null) {

				// The min value for the range
				int min = Int32.Parse(match.Groups[1].Value);
				// The max value for the range
				int max = Int32.Parse(match.Groups[2].Value);

				// Set the range value to a random number between min and max
				rangeValue = random.Next(min, max + 1);

				string description = Regex.Replace(task.getDescription(), pattern, match =>
				{
					return rangeValue.ToString();
				});
				task.setDescription(description);

				taskList.tasks[i] = task;

			}

			pattern = "DEFINE_LIQUID";

			if (task.getDescription().Contains(pattern)) {

				liquidType = (random.Next(0, 2) == 0) ? "red" : "blue";
				
				string description = taskList.tasks[i].getDescription().Replace(pattern, liquidType);
				task.setDescription(description);

				taskList.tasks[i] = task;

			}
		}

		// Go through each task, replacing REFER_RANGE and REFER_LIQUID with actual values
		for (int i = 0; i < numTasks; i++) {

			Task task = taskList.tasks[i];

			// Pattern to find the liquid amount in the task description, defined as REFER_RANGE
			string pattern = "REFER_RANGE";
			if (task.getDescription().Contains(pattern)) {

				string description = task.getDescription().Replace(pattern, rangeValue.ToString());
				task.setDescription(description);

				taskList.tasks[i] = task;

			}

			pattern = "REFER_LIQUID";

			if (task.getDescription().Contains(pattern)) {

				string description = task.getDescription().Replace(pattern, liquidType);
				task.setDescription(description);

				taskList.tasks[i] = task;

			}
		}
	}

	// This function is only called when the root object is initialized or activated
	void Awake() {

		// If an instance already exists, destroy myself
		if (instance != null && instance != this) {
			instance.setTaskList(this.taskList);
			instance.defineTaskValues();
			Destroy(this.gameObject);
			return;
		}
		instance = this;
		defineTaskValues();

		// Sets the root object to not be destroyed when loading a new scene
		DontDestroyOnLoad(gameObject);

		// Subscribes the initiateChildManagers() function such that it is called whenever a new scene loads
		SceneManager.sceneLoaded += initiateChildManagers;

    }

	public void setTaskList(TaskList taskList) {

		this.taskList = taskList;

	}

	// Unsubscribes the initiateChildManagers() function to ensure no memory leaks occur
	void OnDestroy() {

		SceneManager.sceneLoaded -= initiateChildManagers;

	}
	public static GameManager getInstance() {

		if (instance == null) {

			instance = new GameManager();

		}

		return instance;

	}

	private void ResetManagers()
	{
        // Destroy the old child managers
        if (taskManager != null)
        {
            Destroy(taskManager.gameObject);
            taskManager = null;
        }
        if (alertManager != null)
        {
            Destroy(alertManager.gameObject);
            alertManager = null;
        }
        if (handManager != null)
        {
            Destroy(handManager.gameObject);
            handManager = null;
        }
        if (transitionManager != null)
        {
            Destroy(transitionManager.gameObject);
            transitionManager = null;
        }
    }

	public TaskManager getTaskManager() {

		return taskManager;

	}

	public AlertManager getAlertManager() {

		return alertManager;

	}

	public HandManager getHandManager() {

		return handManager;

	}

	public TransitionManager getTransitionmanager()
	{
		return transitionManager;
	}
}
