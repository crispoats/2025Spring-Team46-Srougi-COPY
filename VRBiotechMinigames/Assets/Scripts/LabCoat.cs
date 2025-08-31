using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// This holds the functionality for completing the lab coat task and connects to TaskManager
/// @author: Siri Mudunuri
/// </summary>
public class LabCoat : MonoBehaviour
{
    private int putOnLabCoat;
    private AudioSource audioSource;

    void Start()
    {
        GetComponent<XRSimpleInteractable>().selectEntered.AddListener(OnLabCoatSelected);

        // Get AudioSource component
        audioSource = GetComponent<AudioSource>();

        // Get the index of the task from TaskManager
        putOnLabCoat = GameManager.getInstance().getTaskManager().taskList.getTaskByName("Put on Lab Coat");
    }

    private void OnLabCoatSelected(SelectEnterEventArgs args)
    {
        args.manager.CancelInteractorSelection(args.interactorObject);
        if (!GameManager.getInstance().getTaskManager().isCurrentTask(putOnLabCoat))
            return;

        if (audioSource != null)
            audioSource.Play();

        GameManager.getInstance().getTaskManager().completeTask(putOnLabCoat);
    }

    /// <summary>
    /// When the object is destroyed, disconnect the listeners
    /// </summary>
    void OnDestroy()
    {
        var interactable = GetComponent<XRSimpleInteractable>();
        if (interactable != null)
            interactable.selectEntered.RemoveListener(OnLabCoatSelected);
    }
}
