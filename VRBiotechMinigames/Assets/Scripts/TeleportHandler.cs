using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

/**
 * @author Brayden Batts
 * @since 2025-02-07
 * 
 * Handles the enabling and disabling of a teleport interactor GameObject for
 * the purpose of reducing visual clutter and providing a more intuitive UI.
 * 
 * When the teleportModeActivate action is performed, the teleport interactor is enabled.
 * When the teleportModeActivate action is canceled, the teleport interactor is disabled
 * on the next update, so that the teleport interactor can confirm the teleport first.
 */
public class TeleportHandler : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The interactor this handler should be responsible for.")]
    /** The interactor to be enabled/disabled when appropriate. */
    private GameObject teleportInteractor;

    [SerializeField]
    [Tooltip("The action to use for activating teleport mode.")]
    /** The action that triggers the enabling/disabling. */
    private InputActionProperty teleportModeActivate;

    /** Set to true whenever the teleport interactor should be disabled. */
    private bool disableInteractor = false;

    /**
     * Adds listeners to the "performed" and "canceled" events
     * that will be raised by the teleport mode activate action.
     */
    void Awake()
    {
        teleportModeActivate.action.performed += Performed;
        teleportModeActivate.action.canceled += Canceled;
    }

    private void OnDestroy()
    {
        teleportModeActivate.action.performed -= Performed;
        teleportModeActivate.action.canceled -= Canceled;
    }

    /**
     * Checks each update if the interactor should be disabled.
     */
    void FixedUpdate()
    {
        if (disableInteractor)
        { // If the interactor is set to be disabled
            teleportInteractor.SetActive(false); // Disable it
            disableInteractor = false; // And clear the disable flag
        }
    }

    /**
     * Enable the interactor if the action is performed.
     */
    private void Performed(InputAction.CallbackContext e)
    {
        teleportInteractor.SetActive(true); // Enable the interactor
    }

    /**
     * Set the interactor to be disabled next update if the action is canceled.
     */
    private void Canceled(InputAction.CallbackContext e)
    {
        disableInteractor = true; // Set the disable flag
    }
}
