using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

/**
 * 
 * @author Colby Cress
 * 
 * Contains all functionality regarding the pipette aid, including interaction with an attached serological and bottles.
 * 
 */
public class PipetteAid : ObjectInteractor {

	private GameManager gameManager;

	[SerializeField]
	private Notebook notebook;

	// The available sizes of serologicals; modify this as necessary in the future if any new serological sizes are added
	private static int[] availableSerologicalSizes = {2, 5, 10, 25};

	// The rate at which liquid will be aspirated/dispersed
	[SerializeField]
	private float liquidFlowRate = 0.05f;

	// The max angle at which the pipette aid/serological can aspirate/disperse liquid from a bottle
	[SerializeField]
	private float angleOfTolerance = 15.0f;

	// Degree to which the aspirated liquid amount can vary from the goal amount for the task of aspirating liquid. The higher the value, the greater the tolerance
	[SerializeField]
	private float aspirateTaskTolerance = 0.01f;

	// How many seconds should pass before the aspirate/disperse tasks can be completed
	[SerializeField]
	private float liquidTaskCompletionTime = 0.25f;

	// Represents the current state of the modifier. If false, we are aspirating. If true, we are dispersing
	private bool actionModifier = false;
	// The current amount of liquid to aspirate/disperse
	private float actionModifierAmount = 0.0f;

	// Goal fluid amount for the aspirate task
	private int aspirateGoalFluid;
	// Goal fluid type for the aspirate task
	private string aspirateGoalFluidName;

	// The time at which liquid aspiration/dispersion was last started; 0 if not currently flowing
	private float liquidFlowStartTime = 0.0f;

	// Goal fluid amount for the disperse task
	private int disperseGoalFluid;
	// Goal fluid type for the disperse task
	private string disperseGoalFluidName;

	// The index for minigame 1's task of attaching a serological to the pipette aid
	private int taskAttachSerological;
	// The index for minigame 1's task of aspirating liquid
	private int taskAspirateLiquid;
	// The index for minigame 1's task of dispersing liquid
	private int taskDisperseLiquid;
	// The index for minigame 1's task of placing the pipette aid
	private int taskPlace;

	// This variable is for tracking how much liquid has been dispersed for minigame 1's task of dispersing liquid
	private float dispersedAmountTask = 0.0f;
	private bool isHeld = false;
	private bool isOnTable = false;

	[SerializeField]
	private Collider tableCollider;

	// Mapping for each mistake key value to a bool representing whether or not said mistake has been triggered yet
	private Dictionary<int, bool> mistakesTriggered = new Dictionary<int, bool>
	{
		{0, false},
		{1, false},
		{2, false},
		{3, false},
		{4, false},
		{6, false},
		{7, false},
		{8, false}
	};

	// Invokes a mistake using the given parameters, returning a boolean representing whether or not the specific mistake has already been triggered (and therefore if it was invoked or not)
	private bool triggerMistake(Transform transform, Vector3 offset, int messageKey) {

		// If this mistake has not been invoked yet, we can invoke it
		bool triggered = false;
		if (mistakesTriggered[messageKey] == false) {

			mistakesTriggered[messageKey] = true;

		}
		// If it has, do not invoke it
		else {

			triggered = true;

		}
		if (!triggered) {

			MistakeEventArgs mistakeEventArgs = new MistakeEventArgs(transform, offset, messageKey);
			MistakeEvent.GetInstance().Invoke(mistakeEventArgs);

			return true;

		}

		return false;

	}

	public void Start() {

		gameManager = GameManager.getInstance();

		// Set the index of this task (to attach a serological to the pipette aid)
		taskAttachSerological = gameManager.getTaskManager().taskList.getTaskByName("Attach Serological to Pipette Aid");
		// Set the index of this task (to aspirate liquid)
		taskAspirateLiquid = gameManager.getTaskManager().taskList.getTaskByName("Aspirate Liquid");
		// Set the index of this task (to disperse liquid)
		taskDisperseLiquid = gameManager.getTaskManager().taskList.getTaskByName("Disperse Liquid");
		// Set the index of this task (to place the pipette aid)
		taskPlace = gameManager.getTaskManager().taskList.getTaskByName("Place Pipette Aid");

		string taskDesc = gameManager.getTaskManager().taskList.getTasks().ToArray()[taskAspirateLiquid].description;
		Match match = Regex.Match(taskDesc, @"\d+");
		aspirateGoalFluid = int.Parse(match.Value);
		aspirateGoalFluidName = taskDesc.Contains("red") ? "red" : "blue";

		taskDesc = gameManager.getTaskManager().taskList.getTasks().ToArray()[taskDisperseLiquid].description;
		match = Regex.Match(taskDesc, @"\d+");
		disperseGoalFluid = int.Parse(match.Value);
		disperseGoalFluidName = taskDesc.Contains("red") ? "red" : "blue";

		GetComponent<XRBaseInteractable>().selectEntered.AddListener(SelectEntered);
		GetComponent<XRBaseInteractable>().selectExited.AddListener(SelectExited);
	}

	// When script is enabled, subscribe to the PipetteAidInputEvent
    private void OnEnable() {
        PipetteAidInputEvent.GetInstance().AddListener(HandleInputEvent);
    }

	// When script is disabled, unsubscribe to the PipetteAidInputEvent
    private void OnDisable() {
        PipetteAidInputEvent.GetInstance().RemoveListener(HandleInputEvent);
    }

	// Updates action modifier and liquid flow rate with passed information from pipette aid custom controller
    private void HandleInputEvent(string actionType, float speed) {
        actionModifier = (actionType == "Dispersion");
        liquidFlowRate = speed;
    }


	// Performs all input handling for the pipette aid, including aspirating and dispersing liquid
	public void FixedUpdate() {
		if (!isHeld && isOnTable && transform.position.y > tableCollider.transform.position.y
				&& GetComponent<Rigidbody>().velocity.magnitude < 0.001)
			gameManager.getTaskManager().completeTask(taskPlace);


		// If the socket interactor has a serological attached, complete this task
		XRSocketInteractor interactor = GetComponentInChildren<XRSocketInteractor>();
		Serological serologicalScript = null;
		// Get the serological's script
		if (interactor.hasSelection) {

			serologicalScript = interactor.interactablesSelected[0].transform.GetComponent<Serological>();

		}
		// Do nothing if there is no serological attached
		if (serologicalScript == null) {

			return;

		}
		// If this is the correct task...
		if (gameManager.getTaskManager().GetCurrentTaskIdx() == taskAttachSerological) {

			// The recommeneded max volume of the serological
			float serologicalMaxVolume = serologicalScript.getMaxVolume();

			// Represents whether or not the held serological's size is correct; 0 = correct (complete the task), -1 = too small, 1 = too big
			int isSerologicalSizeCorrect = 1;

			int numPotentialSizes = availableSerologicalSizes.Length;
			// Go through all potential serological sizes, checking to see if the held serological is the correct size
			for (int i = 0; i < numPotentialSizes; i++) {

				int potentialSerologicalSize = availableSerologicalSizes[i];

				if (potentialSerologicalSize <= aspirateGoalFluid) {

					continue;

				}

				// If the held serological's max volume is greater than all the other sizes, we can simply assume that it is the correct size for the task
				if (i == numPotentialSizes - 1 && serologicalMaxVolume >= potentialSerologicalSize) {

					isSerologicalSizeCorrect = 0;
					break;

				}

				// If the held serological's max volume is greater than the current serological size, but less than the next one, it is the correct size for the task
				if (serologicalMaxVolume >= potentialSerologicalSize && serologicalMaxVolume <= availableSerologicalSizes[i + 1]) {

					isSerologicalSizeCorrect = 0;
					break;

				}
			}
			if (serologicalMaxVolume <= aspirateGoalFluid) {

				isSerologicalSizeCorrect = -1;

			}

			// If the attached serological's size is correct, complete the task
			if (isSerologicalSizeCorrect == 0) {

				gameManager.getTaskManager().completeTask(taskAttachSerological);

			}
			// If the attached serological is not the correct size, invoke a mistake
			else {

				Transform transform = this.transform;
				Vector3 offset = new Vector3(0f, 0.125f, 0f);
				// If the serological is too small, use the first mistake message. If it's too big, use the second mistake message
				int tooSmallMistakeIndex = 0, tooBigMistakeIndex = 1;
				int messageKey = isSerologicalSizeCorrect == -1 ? tooSmallMistakeIndex : tooBigMistakeIndex;

				triggerMistake(transform, offset, messageKey);

			}
		}

		// If the aspirate task's timer is complete and the liquid amount is still within the goal amount, complete the task
		if (gameManager.getTaskManager().GetCurrentTaskIdx() == taskAspirateLiquid && liquidFlowStartTime > 0.0f && Time.time - liquidFlowStartTime >= liquidTaskCompletionTime && serologicalScript.getLiquidMaterial().name.Contains(aspirateGoalFluidName) && serologicalScript.getCurrentVolume() >= aspirateGoalFluid - (aspirateGoalFluid * aspirateTaskTolerance) && serologicalScript.getCurrentVolume() <= aspirateGoalFluid + (aspirateGoalFluid * aspirateTaskTolerance)) {

			gameManager.getTaskManager().completeTask(taskAspirateLiquid);
			dispersedAmountTask = 0f;

		}
		// If the disperse task's timer is complete and the dispersed liquid amount is still within the goal amount, complete the task
		if (gameManager.getTaskManager().GetCurrentTaskIdx() == taskDisperseLiquid && liquidFlowStartTime > 0.0f && Time.time - liquidFlowStartTime >= liquidTaskCompletionTime && dispersedAmountTask >= disperseGoalFluid - (disperseGoalFluid * aspirateTaskTolerance)) {

			gameManager.getTaskManager().completeTask(taskDisperseLiquid);

		}

		// Represents which hand is/was holding the pipette aid
		int hand = -1;
		// Represents whether each hand "exists" in the scene (is not hidden/not holding anything)
		bool[] handsExist = {false, false};
		// Go through each object in the scene...
		foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>()) {

			// If the object is one of the hands...
			if (obj.name.Contains("VR_Hand_") && obj.name.Contains("(Clone)")) {

				// Mark that the left hand "exists"
				if (obj.name.Contains("Left")) {

					handsExist[0] = true;

				}
				// Mark that the right hand "exists"
				else if (obj.name.Contains("Right")) {

					handsExist[1] = true;

				}
			}
		}
		// For each child of the pipette aid...
		foreach (Transform child in this.transform) {

			if (child.name.Contains("Interactor")) {

				// If it has the dynamic attachment for the left interactor, then it is being held by the left hand
				if (child.name.Contains("Left")) {

					hand = 0;

				}
				// Similarly for the right interactor
				else if (child.name.Contains("Right")) {

					hand = 1;

				}

				break;

			}
		}
		// If the pipette aid had been held, and its attached serological has liquid in it...
		if (hand != -1 && serologicalScript.getCurrentVolume() > 0.0f) {

			bool isHeld = true;
			// If the hand that held the pipette aid "exists", then the pipette aid is no longer being held
			if (handsExist[hand]) {

				isHeld = false;

			}
			if (!isHeld) {

				Transform transform = this.transform;
				Vector3 offset = new Vector3(0f, 0.5f, 0f);
				int messageKey = 4;

				bool invoked = triggerMistake(transform, offset, messageKey);
				if (invoked) {

					return;

				}
			}
		}

		// If attempting to disperse when the serological is/would be empty, invoke a mistake
		if (actionModifier && actionModifierAmount > 0 && serologicalScript.getCurrentVolume() - liquidFlowRate * actionModifierAmount <= 0) {

			Transform transform = this.transform;
			Vector3 offset = new Vector3(0f, 0.125f, 0f);
			int messageKey = 3;

			bool invoked = triggerMistake(transform, offset, messageKey);
			if (invoked) {

				return;

			}
		}

		// Get the transforms for the top and tip of the serological, for use directly after in calculating the angle
		Transform serologicalTipTransform = serologicalScript.getLiquidTip().transform;
		Transform serologicalTopTransform = serologicalScript.getLiquidTop().transform;

		// Do nothing if the angle of the serological is greater than the tolerance angle
		Vector3 serologicalDirection = (serologicalTipTransform.position - serologicalTopTransform.position).normalized;
		float upAngle = Vector3.Angle(Vector3.down, serologicalDirection);
		// If the angle is too large, liquid would start flowing up the pipette aid in real life. Invoke a mistake
		if (upAngle > angleOfTolerance && serologicalScript.getCurrentVolume() > 0.0f) {

			Transform transform = this.transform;
			Vector3 offset = new Vector3(0f, 0.1f, 0f);
			int messageKey = 7;

			bool invoked = triggerMistake(transform, offset, messageKey);
			if (invoked) {

				return;

			}
		}
		// Do nothing if the angle is too wide
		if (upAngle > angleOfTolerance) {

			return;

		}

		// Get the bottle's script within a small radius of the serological's tip
		Collider[] colliders = Physics.OverlapSphere(serologicalTipTransform.position, 0.05f);
		Bottle bottleScript = null;
		foreach (Collider collider in colliders) {

			// If this collider is the fluid and we are aspirating, set the bottle script
			//if (!actionModifier && collider.name.Contains("bottle") && collider.name.Contains("fluid")) {
			if (!actionModifier && collider.tag.Equals("liquid")) {

				bottleScript = collider.transform.parent.GetComponent<Bottle>();
				break;

			}
			// If this collider is the bottle's base and we are dispersing, set the bottle script
			//else if (actionModifier && collider.name.Contains("bottle") && collider.name.Contains("base")) {
			else if (actionModifier && collider.tag.Equals("Bottle")) {

				bottleScript = collider.transform.parent.GetComponent<Bottle>();
				break;

			}
		}

		// If attempting to aspirate and there is only air
		if (!actionModifier && actionModifierAmount > 0.0f && (bottleScript == null || bottleScript.getCurrentVolume() == 0.0f)) {

			Transform transform = this.transform;
			Vector3 offset = new Vector3(0f, 0.125f, 0f);
			int messageKey = 8;

			bool invoked = triggerMistake(transform, offset, messageKey);
			if (invoked) {

				return;

			}
		}

		// Do nothing if no bottle script was found or the bottle's socket interactor has a selection (i.e. the bottle's cap)
		if (bottleScript == null || bottleScript.GetComponentInChildren<XRSocketInteractor>().hasSelection) {

			return;

		}

		if (!actionModifier) {

			// Do nothing if the serological already has liquid in it and it is not the same kind of liquid as the bottle
			if (serologicalScript.getCurrentVolume() > 0.0f && serologicalScript.getLiquidMaterial() != bottleScript.getLiquidMaterial()) {

				return;

			}

			// If attempting to aspirate when the serological would be cross-contaminated, invoke a mistake (i.e. aspirating a different kind of liquid than previously aspirated with the same serological)
			if (serologicalScript.getCurrentVolume() == 0.0f && serologicalScript.getPreviousLiquidMaterial() != null && serologicalScript.getPreviousLiquidMaterial() != bottleScript.getLiquidMaterial()) {

				Transform transform = this.transform;
				Vector3 offset = new Vector3(0f, 0.125f, 0f);
				int messageKey = 6;

				bool invoked = triggerMistake(transform, offset, messageKey);
				if (invoked) {

					return;

				}
			}

			// Set the serological's liquid to be the bottle's liquid
			serologicalScript.setLiquidMaterial(bottleScript.getLiquidMaterial());

			float amountToAspirate = liquidFlowRate * actionModifierAmount;

			// If the amount in the bottle is too small, only aspirate the amount in the bottle
			float remainingVolumeInBottle = bottleScript.getCurrentVolume();
			if (remainingVolumeInBottle < amountToAspirate) {

				amountToAspirate = remainingVolumeInBottle;

			}
			// If the serological can't aspirate all of the liquid, only aspirate just enough
			float emptyVolumeInSerological = serologicalScript.getTrueMaxVolume() - serologicalScript.getCurrentVolume();
			if (emptyVolumeInSerological < amountToAspirate) {

				amountToAspirate = emptyVolumeInSerological;

			}

			serologicalScript.setCurrentVolume(serologicalScript.getCurrentVolume() + amountToAspirate);
			bottleScript.setCurrentVolume(bottleScript.getCurrentVolume() - amountToAspirate);

			if (amountToAspirate > 0.0f) {

				// Start a timer to complete this task
				liquidFlowStartTime = Time.time;

			}

			// If the serological's new volume after aspirating is above its recommended volume, invoke a mistake
			if (serologicalScript.getCurrentVolume() > serologicalScript.getMaxVolume()) {

				Transform transform = this.transform;
				Vector3 offset = new Vector3(0f, 0.125f, 0f);
				int messageKey = 2;
				
				bool invoked = triggerMistake(transform, offset, messageKey);
				if (invoked) {

					return;

				}
			}
		}
		else if (actionModifier) {

			// Do nothing if the bottle already has liquid in it and it is not the same kind of liquid as the serological
			if (bottleScript.getCurrentVolume() > 0.0f && bottleScript.getLiquidMaterial() != serologicalScript.getLiquidMaterial()) {

				return;

			}

			// Set the bottle's liquid to be the serological's liquid
			bottleScript.setLiquidMaterial(serologicalScript.getLiquidMaterial());

			float amountToDisperse = liquidFlowRate * actionModifierAmount;

			// If the amount in the serological is too small, only aspirate the amount in the serological
			float remainingVolumeInSerological = serologicalScript.getCurrentVolume();
			if (remainingVolumeInSerological < amountToDisperse) {

				amountToDisperse = remainingVolumeInSerological;

			}
			// If the bottle can't take all of the liquid, only disperse just enough
			float emptyVolumeInBottle = bottleScript.getMaxVolume() - bottleScript.getCurrentVolume();
			if (emptyVolumeInBottle < amountToDisperse) {

				amountToDisperse = emptyVolumeInBottle;

			}

			bottleScript.setCurrentVolume(bottleScript.getCurrentVolume() + amountToDisperse);
			serologicalScript.setCurrentVolume(serologicalScript.getCurrentVolume() - amountToDisperse);

			// If the bottle being dispersed into is a small one and its liquid is of the task's requested type...
			//if (bottleScript.name.Contains("Small Bottle") && serologicalScript.getLiquidMaterial().name.Contains(disperseGoalFluidName)) {
			if (bottleScript.tag.Equals("Small Bottle") && serologicalScript.getLiquidMaterial().name.Contains(disperseGoalFluidName)) {

				// Add amount of liquid dispersed to the tracked amount of liquid dispersed for this task
				dispersedAmountTask += amountToDisperse;
			
			}

			if (amountToDisperse > 0.0f) {

				// Start a timer to complete this task
				liquidFlowStartTime = Time.time;

			}
		}
	}

    private void OnDestroy() {
        GetComponent<XRBaseInteractable>().selectEntered.RemoveListener(SelectEntered);
        GetComponent<XRBaseInteractable>().selectExited.RemoveListener(SelectExited);
    }

    // Stores the action context so that it can be used in Update()
    public override bool handleInput(InputAction.CallbackContext context) {

		// Do nothing if there is no current action
		if (context.action == null) {

			return false;

		}

        switch (context.action.name) {

			// If using keyboard, this is the 'B' or 'N' key
			// Modifier button
			case "PrimaryButton":
			case "SecondaryButton":
				if (context.performed) {

					actionModifier = true;

				}
				if (context.canceled) {

					actionModifier = false;

				}
				break;
			// If using mouse, this is the left mouse button
			// Aspirate/Disperse liquid amount
			case "Activate Value":
				actionModifierAmount = context.ReadValue<float>();

				break;

		}

		return true;

	}

	public static int[] getAvailableSerologicalSizes() {

		return availableSerologicalSizes;

	}

    private void OnCollisionEnter(Collision collision)
    {
		if (collision.collider == tableCollider)
			isOnTable = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider == tableCollider)
            isOnTable = false;
    }

	private void SelectEntered(SelectEnterEventArgs args)
	{
		isHeld = true;
    }

    private void SelectExited(SelectExitEventArgs args)
    {
        isHeld = false;
    }
}
