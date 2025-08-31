using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.EventSystems;

/**
 * This script manages the teleportation and interaction ray interactor switching functionality.
 * It enables the teleportation interactor when the teleport button is pressed and the interaction interactor otherwise.
 *
 * @author Jonathan Kolesar jkolesa
 */
public class InteractorSwitcher : MonoBehaviour
{

    public GameObject teleportationInteraction;
    public GameObject everythingElseInteraction;

    //public XRRayInteractor teleportRayInteractor;
    //public XRRayInteractor rayInteractor;
    public InputActionProperty teleportAction; // Button for teleportation

    public ActionBasedController controller;
    private InputActionProperty defaultSelectAction;

    private bool cancelTeleport = true;

    void Start()
    {
        defaultSelectAction = controller.selectAction;
    }

    void Awake()
    {
        teleportAction.action.performed += Performed;
        teleportAction.action.canceled += Canceled;
    }

    private void Performed(InputAction.CallbackContext e)
    {
        teleportationInteraction.SetActive(true);
        everythingElseInteraction.SetActive(false);
        controller.selectAction = teleportAction;
    }

    private void Canceled(InputAction.CallbackContext e)
    {
        cancelTeleport = true;
    }

    void Update()
    {
        if (cancelTeleport)
        {
            teleportationInteraction.SetActive(false);
            everythingElseInteraction.SetActive(true);
            controller.selectAction = defaultSelectAction;

            cancelTeleport = false;
        }
    }
}
