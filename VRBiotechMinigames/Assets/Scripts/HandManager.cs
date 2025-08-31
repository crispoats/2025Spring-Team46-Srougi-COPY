using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;

/**
 * 
 * @author Colby Cress
 * 
 * Handles any input related to the hands and relays it to held objects.
 * 
 */
public class HandManager : MonoBehaviour {

	private XRBaseController leftHand;
	private XRDirectInteractor leftDirectInteractor;
	private XRRayInteractor leftRayInteractor;

	private XRBaseController rightHand;
	private XRDirectInteractor rightDirectInteractor;
	private XRRayInteractor rightRayInteractor;

	[SerializeField]
	private InputActionAsset inputActions;

	public void setup() {

		XRBaseController[] hands = FindObjectsOfType<XRBaseController>();

		// If the first hand in the array is the left one, set the interactors accordingly
		if (hands[0].gameObject.name.ToLower().Contains("left")) {

			leftHand = hands[0];
			leftDirectInteractor = leftHand.GetComponentInChildren<XRDirectInteractor>();
			leftRayInteractor = leftHand.GetComponentInChildren<XRRayInteractor>();

			rightHand = hands[1];
			rightDirectInteractor = rightHand.GetComponentInChildren<XRDirectInteractor>();
			rightRayInteractor = rightHand.GetComponentInChildren<XRRayInteractor>();

		}
		// If the first hand in the array is the right one, set the interactors accordingly
		else {

			leftHand = hands[1];
			leftDirectInteractor = leftHand.GetComponentInChildren<XRDirectInteractor>();
			leftRayInteractor = leftHand.GetComponentInChildren<XRRayInteractor>();

			rightHand = hands[0];
			rightDirectInteractor = rightHand.GetComponentInChildren<XRDirectInteractor>();
			rightRayInteractor = rightHand.GetComponentInChildren<XRRayInteractor>();

		}

		// Get the action maps for any actions we care about
		InputActionMap leftHandInteractionMap = inputActions.FindActionMap("XRI LeftHand Interaction");
		InputActionMap leftHandMap = inputActions.FindActionMap("XRI LeftHand");
		InputActionMap rightHandInteractionMap = inputActions.FindActionMap("XRI RightHand Interaction");
		InputActionMap rightHandMap = inputActions.FindActionMap("XRI RightHand");

		ReadOnlyArray<InputAction> leftActions1 = leftHandInteractionMap.actions;
		ReadOnlyArray<InputAction> leftActions2 = leftHandMap.actions;
		ReadOnlyArray<InputAction> rightActions1 = rightHandInteractionMap.actions;
		ReadOnlyArray<InputAction> rightActions2 = rightHandMap.actions;

		// Add handleInputAction() to each input action, such that any time an action we care about is done, handleInputAction() is called
		foreach (InputAction action in leftActions1) {

			action.performed += handleInputAction;
			action.canceled += handleInputAction;

		}
		foreach (InputAction action in leftActions2) {

			action.performed += handleInputAction;
			action.canceled += handleInputAction;

		}
		foreach (InputAction action in rightActions1) {

			action.performed += handleInputAction;
			action.canceled += handleInputAction;

		}
		foreach (InputAction action in rightActions2) {

			action.performed += handleInputAction;
			action.canceled += handleInputAction;

		}
	}

	// Passes the input action in context to any held objects; in the future, could be extended upon for any other interactions with input actions
	public void handleInputAction(InputAction.CallbackContext context) {
		
		passInputToHandObjects(context);

	}

	// Passes the input action in context to any held objects
	private void passInputToHandObjects(InputAction.CallbackContext context) {

		GameObject leftObject = getObjectInLeftHand();
		GameObject rightObject = getObjectInRightHand();

		Hand hand = getHandFromAction(context);

		// Pass the input action to the left hand
		if (leftObject != null && leftObject.GetComponent<ObjectInteractor>() != null && hand == Hand.Left) {

			ObjectInteractor leftObjInt = leftObject.GetComponentInChildren<ObjectInteractor>();
			leftObjInt.handleInput(context);

		}
		// Pass the input action to the right hand
		if (rightObject != null && rightObject.GetComponent<ObjectInteractor>() != null && hand == Hand.Right) {

			ObjectInteractor rightObjInt = rightObject.GetComponentInChildren<ObjectInteractor>();
			rightObjInt.handleInput(context);

		}
	}

	// Returns the object in the left hand, or null if there is no object held
	public GameObject getObjectInLeftHand() {

		if (leftDirectInteractor.interactablesSelected.Count > 0) {

			return leftDirectInteractor.interactablesSelected[0].transform.gameObject;

		}
		else if (leftRayInteractor.interactablesSelected.Count > 0) {

			return leftRayInteractor.interactablesSelected[0].transform.gameObject;

		}

		return null;

	}

	// Returns the object in the right hand, or null if there is no object held
	public GameObject getObjectInRightHand() {

		if (rightDirectInteractor.interactablesSelected.Count > 0) {

			return rightDirectInteractor.interactablesSelected[0].transform.gameObject;

		}
		else if (rightRayInteractor.interactablesSelected.Count > 0) {

			return rightRayInteractor.interactablesSelected[0].transform.gameObject;

		}

		return null;

	}
	
	public bool isHeld(GameObject obj) {

		return (getObjectInLeftHand() == obj) || (getObjectInRightHand() == obj);

	}

	public void setInputActions(InputActionAsset inputActions) {

		this.inputActions = inputActions;

	}

	private enum Hand { Left, Right, None }

    // Returns string representing the identity of the hand performing the action. Values are "Left", "Right", and "Other"
    private Hand getHandFromAction(InputAction.CallbackContext context) {

        return context.action.actionMap.name.Contains("LeftHand") ? Hand.Left : context.action.actionMap.name.Contains("RightHand") ? Hand.Right : Hand.None;

    }

}

// Abstract container class for all interactable objects
public abstract class ObjectInteractor : MonoBehaviour {

	// Returns a value representing whether or not the held object is held by the correct hand (the one creating the input)
	public abstract bool handleInput(InputAction.CallbackContext context);

}
