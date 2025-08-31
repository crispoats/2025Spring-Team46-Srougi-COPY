using System.Collections.Generic;
using UnityEngine;

/**
  @author: Mark Guo
*/
public class TaskManager : MonoBehaviour
{
    [SerializeField] public TaskList taskList;
    private int currentTaskIndex = 0;

    private AudioSource taskCompleteSFX;
    private AudioSource minigameCompleteSFX;

    //Start State
    private void Start()
    {
        taskCompleteSFX = gameObject.AddComponent<AudioSource>();
        AudioClip clip = Resources.Load<AudioClip>("Audio/TaskCompleteSFX");
        if (clip != null)
        {
            taskCompleteSFX.clip = clip;
        }

        minigameCompleteSFX = gameObject.AddComponent<AudioSource>();
        AudioClip minigameClip = Resources.Load<AudioClip>("Audio/MinigameCompleteSFX");
        if (minigameClip != null)
        {
            minigameCompleteSFX.clip = minigameClip;
        }


        if (taskList == null || taskList.getTasks().Count == 0)
        {
            Debug.LogWarning("TaskManager: No tasks assigned.");
        }
        else
        {
            Debug.Log($"Starting Task: {getCurrentTask().getName()}");
        }
    }

    public Task getCurrentTask()
    {
        if (currentTaskIndex < taskList.getTasks().Count)
        {
            return taskList.getTasks()[currentTaskIndex];
        }
        return default;
    }

    // Completes a task at a specific index and moves to the next task.
    public void completeTask(int taskIndex)
    {
        if (taskIndex == currentTaskIndex && taskIndex < taskList.getTasks().Count)
        {
            Debug.Log($"Completed Task: {getCurrentTask().getName()}");
            currentTaskIndex++;
            if (currentTaskIndex < taskList.getTasks().Count)
            {
                taskCompleteSFX.Play();
            }
            else
            {
                minigameCompleteSFX.Play();
            }


            if (currentTaskIndex < taskList.getTasks().Count)
            {
                Debug.Log($"New Task: {getCurrentTask().getName()}");
            }
            else
            {
                Debug.Log("All tasks completed!");
            }

            // Quick Fix: Can be done more properly next semester
            // When the task gets completed, make the Notebook show the next task when the Notebook is open
            GameManager.getInstance().getAlertManager().notebook.DisplayTask(currentTaskIndex);
            
        }
        //else
        //{
        //  Debug.LogWarning($"TaskManager: Task at index {taskIndex} is not the current task or is out of range.");
        //}

    }

    //add tasks
    public void addTask(Task task)
    {
        taskList.getTasks().Add(task);
    }

    //determine current task
    public bool isCurrentTask(int taskIndex) => taskIndex == currentTaskIndex;

    /// <summary>
    /// Return the current task index
    /// </summary>
    /// <returns>The current task index</returns>
    public int GetCurrentTaskIdx()
    {
        return currentTaskIndex;
    }
}
