using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;

/**
 * 
 * @author Colby Cress
 * 
 * Contains all functionality regarding serologicals, including updating the liquid level. 
 * 
 */
public class Serological : MonoBehaviour {

	private GameManager gameManager;

	// The actual max volume of the serological; any more and the serological would overflow in real life
	[SerializeField]
	private float trueMaxVolume;
	// The maximum recommended volume for use of the serological
	[SerializeField]
	private float maxVolume;
	// Current volume of the serological
	[SerializeField]
	private float currentVolume = 0.0f;

	// The material/color of the liquid inside of the serological
	[SerializeField]
	private Material liquidMaterial;
	// The previous material of the liquid inside of the serological (if any), for use in mistake when reusing serological for different liquids
	private Material previousLiquidMaterial = null;

	// Essentially, each of these objects' local scaling acts as a bounds for which the liquid is to be rendered
	[SerializeField]
	private GameObject serologicalLiquidTop;
	[SerializeField]
	private GameObject serologicalLiquidMiddle;
	[SerializeField]
	private GameObject serologicalLiquidTip;

	// This field is used in Update() to ensure that it only runs when the volume has changed, for optimization purposes
	private float previousVolume = -1.0f;

	private MeshRenderer liquidRendererTop;
	private MeshRenderer liquidRendererMiddle;
	private MeshRenderer liquidRendererTip;

	// The index for minigame 1's task of picking up a serological
	private int taskPickUpSerological;
	// Mapping for each mistake key value to a bool representing whether or not said mistake has been triggered yet
	private Dictionary<int, bool> mistakesTriggered = new Dictionary<int, bool>
	{
		{0, false},
		{1, false}
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
		
		liquidRendererTop = serologicalLiquidTop.GetComponent<MeshRenderer>();
		liquidRendererMiddle = serologicalLiquidMiddle.GetComponent<MeshRenderer>();
		liquidRendererTip = serologicalLiquidTip.GetComponent<MeshRenderer>();

		// Set the game object representing the top of the serological to its parent (serological_top) so that we can maintain correct scaling
		serologicalLiquidTop = serologicalLiquidTop.transform.parent.gameObject;

		setLiquidMaterial(liquidMaterial);
		// Retroactively set previousLiquidMaterial, because setLiquidMaterial() sets it to whatever the liquid material was before calling the function (as it should)
		this.previousLiquidMaterial = null;

		// Set the index of this task (to pick up a serological)
		taskPickUpSerological = gameManager.getTaskManager().taskList.getTaskByName("Select Serological");

	}

	public GameObject getLiquidTip() {

		return serologicalLiquidTip;

	}

	public GameObject getLiquidTop() {

		return serologicalLiquidTop;

	}

	// Sets the liquid's material and its previous material
	public void setLiquidMaterial(Material liquidMaterial) {

		if (currentVolume > 0f) {

			this.previousLiquidMaterial = this.liquidMaterial;

		}
		this.liquidMaterial = liquidMaterial;

		List<Material> materials = new List<Material>();
		materials.Add(liquidMaterial);
		liquidRendererTop.SetMaterials(materials);
		liquidRendererMiddle.SetMaterials(materials);
		liquidRendererTip.SetMaterials(materials);

	}

	// Updates the liquid's rendered volume
	public void Update() {

		bool serologicalIsHeld = gameManager.getHandManager().isHeld(this.gameObject);
		int[] availableSerologicalSizes = PipetteAid.getAvailableSerologicalSizes();

		// If the serological is currently being held...
		if (serologicalIsHeld && gameManager.getTaskManager().GetCurrentTaskIdx() == taskPickUpSerological) {

			string taskDesc = gameManager.getTaskManager().taskList.getTasks().ToArray()[taskPickUpSerological].description;
			Match match = Regex.Match(taskDesc, @"\d+");
			int goalFluid = int.Parse(match.Value);

			// Represents whether or not the held serological's size is correct; 0 = correct (complete the task), -1 = too small, 1 = too big
			int isSerologicalSizeCorrect = 1;

			int numPotentialSizes = availableSerologicalSizes.Length;
			// Represents whether or not the held serological is of the minimum valid size
			bool isMinimumSize = true;
			// Go through all potential serological sizes, checking to see if the held serological is the correct size
			for (int i = 0; i < numPotentialSizes; i++) {

				int potentialSerologicalSize = availableSerologicalSizes[i];

				if (potentialSerologicalSize <= goalFluid) {

					continue;

				}
				// If the held serological is not of the minimum valid size, it is invalid
				if (!isMinimumSize) {

					break;

				}

				// If the held serological's max volume is greater than all the other sizes, we can simply assume that it is the correct size for the task
				if (i == numPotentialSizes - 1 && maxVolume >= potentialSerologicalSize) {

					isSerologicalSizeCorrect = 0;
					break;

				}

				// If the held serological's max volume is greater than the current serological size, but less than the next one, it is the correct size for the task
				if (maxVolume >= potentialSerologicalSize && maxVolume < availableSerologicalSizes[i + 1]) {

					isSerologicalSizeCorrect = 0;
					break;

				}

				// If there as a valid serological size smaller than the held serological's size, then this serological is invalid
				isMinimumSize = false;

			}
			if (maxVolume <= goalFluid) {

				isSerologicalSizeCorrect = -1;

			}

			// If the held serological's size is correct, complete the task
			if (isSerologicalSizeCorrect == 0) {

				gameManager.getTaskManager().completeTask(taskPickUpSerological);

			}
			// If the held serological is not the correct size, invoke a mistake
			else {

				Transform transform = this.transform;
				Vector3 offset = new Vector3(0f, 0.1f, 0f);
				// If the serological is too small, use the first mistake message. If it's too big, use the second mistake message
				int tooSmallMistakeIndex = 0, tooBigMistakeIndex = 1;
				int messageKey = isSerologicalSizeCorrect == -1 ? tooSmallMistakeIndex : tooBigMistakeIndex;
				
				triggerMistake(transform, offset, messageKey);

			}
		}

		// If the volume has not changed since last time, no need to do any updates
		if (previousVolume == currentVolume) {

			return;

		}

		previousVolume = currentVolume;

		// If the current volume is greater than the true max capacity of the serological, cap the volume
		if (currentVolume >= trueMaxVolume) {

			currentVolume = trueMaxVolume;

		}

		// Have to hardcode this due to different z-scaling between the serologicals
		// The max size of the serological's tip in mL, due to scaling issues
		float maxTipVolume = 0f;
		// The max size of the serological's middle part in mL, due to scaling issues
		float maxMiddleVolume = 0f;

		// 2 mL
		if (maxVolume == availableSerologicalSizes[0]) {

			maxTipVolume = 0.2f;
			maxMiddleVolume = 2f;

		}
		// 5 mL
		else if (maxVolume == availableSerologicalSizes[1]) {

			maxTipVolume = 1f;
			maxMiddleVolume = 5.75f;

		}
		// 10 mL
		else if (maxVolume == availableSerologicalSizes[2]) {

			maxTipVolume = 1f;
			maxMiddleVolume = 10f;

		}
		// 25 mL
		else if (maxVolume == availableSerologicalSizes[3]) {

			maxTipVolume = 2f;
			maxMiddleVolume = 25f;

		}

		// The current volume is greater than the recommended max volume, but less than (or equal to) the true max capacity
		if (currentVolume <= trueMaxVolume && currentVolume > maxVolume) {

			float topZ = (currentVolume - maxVolume) / (trueMaxVolume - maxVolume);
			serologicalLiquidTop.transform.localScale = new Vector3(1.0f, 1.0f, topZ);
			serologicalLiquidMiddle.transform.localScale = Vector3.one;
			serologicalLiquidTip.transform.localScale = Vector3.one;

		}
		// The current volume is less than (or equal to) the recommended max volume, but greater than the amount held by the tip
		else if (currentVolume <= maxVolume && currentVolume > maxTipVolume) {

			float middleZ = (currentVolume - maxTipVolume) / (maxMiddleVolume - maxTipVolume);
			serologicalLiquidTop.transform.localScale = Vector3.zero;
			serologicalLiquidMiddle.transform.localScale = new Vector3(1.0f, 1.0f, middleZ);
			serologicalLiquidTip.transform.localScale = Vector3.one;

		}
		// The current volume is less than or equal to the amount held by the tip, but not empty
		else if (currentVolume <= maxTipVolume && currentVolume > 0f) {

			float tipZ = currentVolume / maxTipVolume;
			serologicalLiquidTop.transform.localScale = Vector3.zero;
			serologicalLiquidMiddle.transform.localScale = Vector3.zero;
			serologicalLiquidTip.transform.localScale = new Vector3(1.0f, 1.0f, tipZ);

		}
		else { // currentVolume <= 0

			currentVolume = 0f;

			serologicalLiquidTop.transform.localScale = Vector3.zero;
			serologicalLiquidMiddle.transform.localScale = Vector3.zero;
			serologicalLiquidTip.transform.localScale = Vector3.zero;

		}

		// TODO remove the stuff below this






		// The current volume is greater than the recommended max volume, but less than (or equal to) the true max capacity
		//if (currentVolume <= trueMaxVolume && currentVolume > maxVolume) {

		//	float topZ = ((currentVolume - maxVolume) / (trueMaxVolume - maxVolume)) * 0.66f;
		//	serologicalLiquidTop.transform.localScale = new Vector3(1.0f, 1.0f, topZ);
		//	serologicalLiquidMiddle.transform.localScale = Vector3.one;
		//	serologicalLiquidTip.transform.localScale = Vector3.one;

		//}
		//// The current volume is less than (or equal to) the recommended max volume, but greater than 1 (the amount held by the tip)
		//else if (currentVolume <= maxVolume && currentVolume > 1) {

		//	serologicalLiquidTop.transform.localScale = Vector3.zero;
		//	serologicalLiquidMiddle.transform.localScale = new Vector3(1.0f, 1.0f, currentVolume / maxVolume);
		//	serologicalLiquidTip.transform.localScale = Vector3.one;

		//}
		//// The current volume is less than or equal to 1 (the amount held by the tip), but not empty
		//else if (currentVolume <= 1 && currentVolume > 0) {

		//	serologicalLiquidTop.transform.localScale = Vector3.zero;
		//	serologicalLiquidMiddle.transform.localScale = Vector3.zero;
		//	serologicalLiquidTip.transform.localScale = new Vector3(1.0f, 1.0f, currentVolume);

		//}
		//else { // currentVolume <= 0

		//	currentVolume = 0.0f;

		//	serologicalLiquidTop.transform.localScale = Vector3.zero;
		//	serologicalLiquidMiddle.transform.localScale = Vector3.zero;
		//	serologicalLiquidTip.transform.localScale = Vector3.zero;

		//}
	}

	public float getCurrentVolume() {

		return currentVolume;

	}

	public void setCurrentVolume(float newVolume) {

		currentVolume = newVolume;

	}

	public Material getLiquidMaterial() {

		return liquidMaterial;

	}

	public Material getPreviousLiquidMaterial() {

		return previousLiquidMaterial;

	}

	public float getMaxVolume() {

		return maxVolume;

	}

	public float getTrueMaxVolume() {

		return trueMaxVolume;

	}

}
