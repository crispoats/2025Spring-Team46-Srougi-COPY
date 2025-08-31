using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{

    /// <summary>
    /// Controller button action to open and close the Notebook
    /// </summary>
    [SerializeField]
    private InputActionProperty menuButtonAction;

    /// <summary>
    /// Control button for left hand snap turn
    /// </summary>
    [SerializeField]
    private InputActionProperty leftSnapTurnAction;

    /// <summary>
    /// Control button for right hand snap turn
    /// </summary>
    [SerializeField]
    private InputActionProperty rightSnapTurnAction;

    /// <summary>
    /// VR Face Button Actions to track
    /// </summary>
    [SerializeField]
    private InputActionProperty aButtonAction;
    [SerializeField]
    private InputActionProperty bButtonAction;
    [SerializeField]
    private InputActionProperty xButtonAction;
    [SerializeField]
    private InputActionProperty yButtonAction;

    /// <summary>
    /// Trigger Buttons to track
    /// </summary>
    [SerializeField]
    private InputActionProperty leftTriggerAction;
    [SerializeField]
    private InputActionProperty rightTriggerAction;


    // TaskIds
    private int taskOpenNotebook;
    private int taskSnapTurn;
    private int taskTeleport;
    private int taskClick;
    private int taskGrab;
    private int taskPrimary;
    private int taskRayGrab;
    private int taskSecondary;
    private int taskBothAction;

    /// <summary>
    /// The list of buttons used to switch between pages
    /// </summary>
    [SerializeField]
    private GameObject[] taskDisplays;

    /// <summary>
    /// White Cube object used for the Tutorial
    /// </summary>
    [SerializeField]
    private GameObject whiteBox;

    /// <summary>
    /// Gray Cube object used for the Tutorial
    /// </summary>
    [SerializeField]
    private GameObject grayBox;

    // Queues for the tasks display
    private Queue<IEnumerator> taskDisplayQueue = new Queue<IEnumerator>();
    private bool isTaskDisplayRunning = false;



    private void Start()
    {
        menuButtonAction.action.performed += NotebookOpenEvent;
        leftSnapTurnAction.action.performed += SnapTurnEvent;
        rightSnapTurnAction.action.performed += SnapTurnEvent;

        aButtonAction.action.performed += RightHandButtonAction;
        aButtonAction.action.canceled += RightHandButtonAction;
        bButtonAction.action.performed += RightHandButtonAction;
        bButtonAction.action.canceled += RightHandButtonAction;
        xButtonAction.action.performed += LeftHandButtonAction;
        xButtonAction.action.canceled += LeftHandButtonAction;
        yButtonAction.action.performed += LeftHandButtonAction;
        yButtonAction.action.canceled += LeftHandButtonAction;

        leftTriggerAction.action.performed += LeftHandTriggerAction;
        leftTriggerAction.action.canceled += LeftHandTriggerAction;
        rightTriggerAction.action.performed += RightHandTriggerAction;
        rightTriggerAction.action.canceled += RightHandTriggerAction;



        taskOpenNotebook = GameManager.getInstance().getTaskManager().taskList.getTaskByName("Open Notebook");
        taskSnapTurn = GameManager.getInstance().getTaskManager().taskList.getTaskByName("Perform Snap Turn");
        taskTeleport = GameManager.getInstance().getTaskManager().taskList.getTaskByName("Teleport to White Floor");
        taskClick = GameManager.getInstance().getTaskManager().taskList.getTaskByName("Click the GUI");
        taskGrab = GameManager.getInstance().getTaskManager().taskList.getTaskByName("Grab the White Cube");
        taskPrimary = GameManager.getInstance().getTaskManager().taskList.getTaskByName("Perform Action");
        taskRayGrab = GameManager.getInstance().getTaskManager().taskList.getTaskByName("Grab Distant Objects");
        taskSecondary = GameManager.getInstance().getTaskManager().taskList.getTaskByName("Use Modifier Button");
        taskBothAction = GameManager.getInstance().getTaskManager().taskList.getTaskByName("Combine Actions");

    }

    void Update()
    {
        if (GameManager.getInstance().getHandManager().isHeld(whiteBox))
        {
            CompleteGrabTask();
        }

        if (GameManager.getInstance().getHandManager().isHeld(grayBox))
        {
            CompleteDistantGrabTask();
        }

    }

    private void OnDestroy()
    {
        menuButtonAction.action.performed -= NotebookOpenEvent;
        leftSnapTurnAction.action.performed -= SnapTurnEvent;
        rightSnapTurnAction.action.performed -= SnapTurnEvent;

        aButtonAction.action.performed -= RightHandButtonAction;
        aButtonAction.action.canceled -= RightHandButtonAction;
        bButtonAction.action.performed -= RightHandButtonAction;
        bButtonAction.action.canceled -= RightHandButtonAction;
        xButtonAction.action.performed -= LeftHandButtonAction;
        xButtonAction.action.canceled -= LeftHandButtonAction;
        yButtonAction.action.performed -= LeftHandButtonAction;
        yButtonAction.action.canceled -= LeftHandButtonAction;

        leftTriggerAction.action.performed -= LeftHandTriggerAction;
        leftTriggerAction.action.canceled -= LeftHandTriggerAction;
        rightTriggerAction.action.performed -= RightHandTriggerAction;
        rightTriggerAction.action.canceled -= RightHandTriggerAction;
    }

    private void LeftHandButtonAction(InputAction.CallbackContext callback)
    {
        if (GameManager.getInstance().getHandManager().getObjectInLeftHand() == grayBox)
        {
            // Get the Renderer component from the cube
            Renderer cubeRenderer = grayBox.GetComponent<Renderer>();

            if (callback.action == null)
            {
                return;
            }
            if (callback.performed)
            {
                cubeRenderer.material.color = Color.blue;
                CompleteSecondaryActionTask();
            } 
            else if (callback.canceled)
            {
                cubeRenderer.material.color = Color.gray;
            }
        }
    }

    private void RightHandButtonAction(InputAction.CallbackContext callback)
    {
        if (GameManager.getInstance().getHandManager().getObjectInRightHand() == grayBox)
        {
            // Get the Renderer component from the cube
            Renderer cubeRenderer = grayBox.GetComponent<Renderer>();

            if (callback.action == null)
            {
                return;
            }
            if (callback.performed)
            {
                
                cubeRenderer.material.color = Color.blue;
                CompleteSecondaryActionTask();
            }
            else if (callback.canceled)
            {
                cubeRenderer.material.color = Color.gray;
            }
        }
    }

    private void LeftHandTriggerAction(InputAction.CallbackContext callback)
    {
        if (GameManager.getInstance().getHandManager().getObjectInLeftHand() == whiteBox)
        {
            // Get the Renderer component from the cube
            Renderer cubeRenderer = whiteBox.GetComponent<Renderer>();

            Debug.Log(callback.action);

            if (callback.action == null)
            {
                return;
            }
            if (callback.performed)
            {
                cubeRenderer.material.color = Color.red;
                CompletePrimaryActionTask();
            }
            else if (callback.canceled)
            {
                cubeRenderer.material.color = Color.white;
            }
        }
        if (GameManager.getInstance().getHandManager().getObjectInLeftHand() == grayBox)
        {
            // Get the Renderer component from the cube
            Renderer cubeRenderer = grayBox.GetComponent<Renderer>();

            if (cubeRenderer.material.color == Color.blue)
            {
                // Set the color to green
                cubeRenderer.material.color = Color.green;
                CompleteBothActionTask();
            }
        }
    }

    private void RightHandTriggerAction(InputAction.CallbackContext callback)
    {
        if (GameManager.getInstance().getHandManager().getObjectInRightHand() == whiteBox)
        {
            // Get the Renderer component from the cube
            Renderer cubeRenderer = whiteBox.GetComponent<Renderer>();

            if (callback.action == null)
            {
                return;
            }
            if (callback.performed)
            {
                cubeRenderer.material.color = Color.red;
                CompletePrimaryActionTask();
            }
            else if (callback.canceled)
            {
                cubeRenderer.material.color = Color.white;
            }
        }
        Debug.Log("Action");
        if (GameManager.getInstance().getHandManager().getObjectInRightHand() == grayBox)
        {
            // Get the Renderer component from the cube
            Renderer cubeRenderer = grayBox.GetComponent<Renderer>();

            Debug.Log(cubeRenderer.material.color);
            if (cubeRenderer.material.color == Color.blue)
            {
                // Set the color to green
                cubeRenderer.material.color = Color.green;
                CompleteBothActionTask();
            }
        }
    }

    /// <summary>
    /// This task will be completed when the player opens the Notebook GUI
    /// </summary>
    /// <param name="callback">The data of the input performed (Not Used)</param>
    public void NotebookOpenEvent(InputAction.CallbackContext callback)
    {
        if (GameManager.getInstance().getTaskManager().GetCurrentTaskIdx() == taskOpenNotebook)
        {
            GameManager.getInstance().getTaskManager().completeTask(taskOpenNotebook);
            EnqueueTaskDisplay(0, 1);
        }

    }

    /// <summary>
    /// This task will be completed when the player performs the snap turn input (left, right, or back)
    /// </summary>
    /// <param name="callback">The data of the input performed (Not Used)</param>
    public void SnapTurnEvent(InputAction.CallbackContext callback)
    {
        if (GameManager.getInstance().getTaskManager().GetCurrentTaskIdx() == taskSnapTurn)
        {
            GameManager.getInstance().getTaskManager().completeTask(taskSnapTurn);
            EnqueueTaskDisplay(1, 2);
        }
    }


    /// <summary>
    /// This task will be completed when the player arrives at the white box at the floor, whhich triggers this method
    /// </summary>
    public void CompleteTeleportTask()
    {
        if (GameManager.getInstance().getTaskManager().GetCurrentTaskIdx() == taskTeleport)
        {
            GameManager.getInstance().getTaskManager().completeTask(taskTeleport);
            EnqueueTaskDisplay(2, 3);
        }
    }

    public void CompleteClickTask()
    {
        if (GameManager.getInstance().getTaskManager().GetCurrentTaskIdx() == taskClick)
        {
            GameManager.getInstance().getTaskManager().completeTask(taskClick);
            EnqueueTaskDisplay(3, 4);
            whiteBox.SetActive(true);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void CompleteGrabTask()
    {
        if (GameManager.getInstance().getTaskManager().GetCurrentTaskIdx() == taskGrab)
        {
            GameManager.getInstance().getTaskManager().completeTask(taskGrab);
            EnqueueTaskDisplay(4, 5);
        }
    }

    public void CompletePrimaryActionTask()
    {
        if (GameManager.getInstance().getTaskManager().GetCurrentTaskIdx() == taskPrimary)
        {
            GameManager.getInstance().getTaskManager().completeTask(taskPrimary);
            EnqueueTaskDisplay(5, 6);
            grayBox.SetActive(true);
        }
    }

    public void CompleteDistantGrabTask()
    {
        if (GameManager.getInstance().getTaskManager().GetCurrentTaskIdx() == taskRayGrab)
        {
            GameManager.getInstance().getTaskManager().completeTask(taskRayGrab);
            EnqueueTaskDisplay(6, 7);
        }
    }

    public void CompleteSecondaryActionTask()
    {
        if (GameManager.getInstance().getTaskManager().GetCurrentTaskIdx() == taskSecondary)
        {
            GameManager.getInstance().getTaskManager().completeTask(taskSecondary);
            EnqueueTaskDisplay(7, 8);
        }
    }

    public void CompleteBothActionTask()
    {
        if (GameManager.getInstance().getTaskManager().GetCurrentTaskIdx() == taskBothAction)
        {
            GameManager.getInstance().getTaskManager().completeTask(taskBothAction);
            EnqueueTaskDisplay(8, 9);
        }
    }

    private IEnumerator TaskDisplaySequence(int fadeOutIdx, int fadeInIdx)
    {
        yield return StartCoroutine(FadeOutTaskDisplay(fadeOutIdx));
        yield return StartCoroutine(FadeInTaskDisplay(fadeInIdx));
    }

    private void EnqueueTaskDisplay(int fadeOutIdx, int fadeInIdx)
    {
        IEnumerator taskSequence = TaskDisplaySequence(fadeOutIdx, fadeInIdx);
        taskDisplayQueue.Enqueue(taskSequence);

        if (!isTaskDisplayRunning)
            StartCoroutine(RunTaskDisplayQueue());
    }

    private IEnumerator RunTaskDisplayQueue()
    {
        isTaskDisplayRunning = true;

        while (taskDisplayQueue.Count > 0)
        {
            yield return StartCoroutine(taskDisplayQueue.Dequeue());
        }

        isTaskDisplayRunning = false;
    }



    /// Fades in the alert UI.
    private IEnumerator FadeInTaskDisplay(int idx)
    {
        CanvasGroup canvasGroup = taskDisplays[idx].GetComponent<CanvasGroup>();

        if (canvasGroup == null) yield break;

        taskDisplays[idx].SetActive(true);
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.deltaTime * 1.5f;
            yield return null;
        }

        canvasGroup.alpha = 1f;

    }


    /// Fades out the alert UI and hides it.
    private IEnumerator FadeOutTaskDisplay(int idx)
    {
        CanvasGroup canvasGroup = taskDisplays[idx].GetComponent<CanvasGroup>();
        if (canvasGroup == null) yield break;

        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.deltaTime * 1.5f;
            yield return null;
        }
        canvasGroup.alpha = 0f;
        taskDisplays[idx].SetActive(false);
    }


}
