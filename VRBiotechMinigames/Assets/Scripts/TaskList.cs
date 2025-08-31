using System.Collections.Generic;
using UnityEngine;
//author: Mark Guo
[CreateAssetMenu(fileName = "NewTaskList", menuName = "Task System/Task List")]
public class TaskList : ScriptableObject
{
    public List<Task> tasks = new List<Task>();

    public List<Task> getTasks() => tasks;

	public int getTaskByName(string name) {

		int numTasks = tasks.Count;
		for (int i = 0; i < numTasks; i++) {

			if (tasks[i].name.Equals(name)) {

				return i;

			}
		}

		return -1;

	}
}
