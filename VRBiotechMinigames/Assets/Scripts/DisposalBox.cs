using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// This holds the functionality for the DisposalBox
/// Removes objects that get attached to the Disposal and connects to TaskManager
/// DisposalBox will only accept Serological Objects.
/// @author: Jason Nguyen
/// </summary>
public class DisposalBox : MonoBehaviour
{

    /// <summary>
    /// The index for minigame 1's task of disposing a serological
    /// </summary>
    private int disposeSerological;

    /// <summary>
    /// The socket component to listen to events when objects are attached
    /// </summary>
    private XRSocketInteractor socket;

    void Start()
    {
        socket = GetComponent<XRSocketInteractor>();

        // Subscribe to events
        socket.selectEntered.AddListener(OnObjectAttached);

        // Get the index of the task from TaskManager
        disposeSerological = GameManager.getInstance().getTaskManager().taskList.getTaskByName("Dispose Serological");

    }

    /// <summary>
    /// Detect when an object is attached to the socket, then distroy it
    /// Complete the Task when the serological is attached
    /// </summary>
    /// <param name="args"></param>
    private void OnObjectAttached(SelectEnterEventArgs args)
    {
        GameObject attachedObject = args.interactableObject.transform.gameObject;
        Destroy(attachedObject);
        GameManager.getInstance().getTaskManager().completeTask(disposeSerological);
        Debug.Log("Object attached: " + attachedObject.name);
    }

    /// <summary>
    /// When the object is destroyed, disconnect the listeners
    /// </summary>
    void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        socket.selectEntered.RemoveListener(OnObjectAttached);
    }
}