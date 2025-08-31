using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// This holds the functionality for the Goggles
/// Goggles disappear when selected and connects to TaskManager
/// @author: Siri Mudunuri
/// </summary>
public class Goggles : MonoBehaviour
{
    private int putOnSafetyGoggles;

    void Start()
    {

        // Get the index of the task from TaskManager
        putOnSafetyGoggles = GameManager.getInstance().getTaskManager().taskList.getTaskByName("Put on Safety Goggles");

        GetComponent<XRSimpleInteractable>().selectEntered.AddListener(OnGogglesSelected);
    }

    private void OnGogglesSelected(SelectEnterEventArgs args)
    {
        args.manager.CancelInteractorSelection(args.interactorObject);
        if (!GameManager.getInstance().getTaskManager().isCurrentTask(putOnSafetyGoggles))
            return;

        GameManager.getInstance().getTaskManager().completeTask(putOnSafetyGoggles);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        XRSimpleInteractable interactable = GetComponent<XRSimpleInteractable>();
        if (interactable != null)
            interactable.selectEntered.RemoveListener(OnGogglesSelected);
    }
}
