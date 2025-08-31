using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/**
 * @author Brayden Batts
 * @since 2025-02-08
 * 
 * Disables the associated Line Visual when the Ray Interactor has a selection
 * for the purpose of reducing user interface clutter and improving visual clarity.
 * Also re-enables the Line Visual when the selection is lost.
 */
public class LineVisualHandler : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The ray interactor that is checked for selection to disable the line visual.")]
    /** The XRRayInteractor that will raise selection events. */
    private XRRayInteractor rayInteractor;

    [SerializeField]
    [Tooltip("The line visual to be disabled when a selection is made, and re-enabled upon deselection.")]
    /** The XRInteractorLineVisual that will be hidden/shown in response to selection events. */
    private XRInteractorLineVisual lineVisual;

    /**
     * When this script is loaded, add listeners to the Select Entered and Exited events.
     */
    private void Awake()
    {
        // Listen for the ray interactor selecting something
        rayInteractor.selectEntered.AddListener(SelectEntered);
        // Listen for the ray interactor unselecting something
        rayInteractor.selectExited.AddListener(SelectExited);
    }

    /**
     * When the ray interactor selects something, hide the line visual.
     */
    void SelectEntered(SelectEnterEventArgs args)
    {
        lineVisual.enabled = false;
    }

    /**
     * When the ray interactor unselects something, show the line visual.
     */
    void SelectExited(SelectExitEventArgs args)
    {
        lineVisual.enabled = true;
    }
}
