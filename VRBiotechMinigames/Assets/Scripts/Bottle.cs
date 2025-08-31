using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

/**
 * 
 * @author Colby Cress
 * 
 * Contains all functionality regarding bottles, including updating the liquid level. 
 * 
 */
public class Bottle : MonoBehaviour {

	// The maximum volume of the bottle
	[SerializeField]
	private float maxVolume;
	// Current volume of the bottle
	[SerializeField]
	private float currentVolume = 0.0f;

	// The material/color of the liquid inside of the bottle
	[SerializeField]
	private Material liquidMaterial;

	// This object's local scaling acts as a bounds for which the liquid is to be rendered
	[SerializeField]
	private GameObject bottleLiquid;

	private MeshRenderer bottleRenderer;

    // These fields are used for the Uncapped Bottle Mistake Event
    private float timeUncapped = 0.0f;
    private readonly float maxTimeUncapped = 30.0f;
	private bool alertThrown = false;

    // This field is used in Update() to ensure that it only runs when the volume has changed, for optimization purposes
    private float previousVolume = -1.0f;

	public void Awake() {

		bottleRenderer = bottleLiquid.GetComponent<MeshRenderer>();

		setLiquidMaterial(liquidMaterial);

	}

	// Sets the liquid's material
	public void setLiquidMaterial(Material liquidMaterial) {

		this.liquidMaterial = liquidMaterial;

		List<Material> materials = new List<Material>();
		materials.Add(liquidMaterial);
		bottleRenderer.SetMaterials(materials);

	}

	// Updates the liquid's rendered volume
	public void Update() {

        // If the bottle doesn't have a cap on it, increase the uncapped time
        if (!GetComponentInChildren<XRSocketInteractor>().hasSelection)
        {
            // If the max uncap time is reached, invoke a MistakeEvent
            if (timeUncapped > maxTimeUncapped)
            {
				if (!alertThrown)
				{
                    MistakeEventArgs args = new MistakeEventArgs(transform, new Vector3(0, 0.25f, 0), 5);
                    MistakeEvent.GetInstance().Invoke(args);
					alertThrown = true;
                } 
			} else
			{
                timeUncapped += Time.deltaTime;
            }
        }
        else
        {
			// If the cap is on set the time to 0
			alertThrown = false;
            timeUncapped = 0.0f;
        }

        // If the volume has not changed since last time, no need to do any updates
        if (previousVolume == currentVolume) {

			return;

		}

		previousVolume = currentVolume;

		// If the current volume is greater than the max capacity of the bottle, cap the volume
		if (currentVolume >= maxVolume) {

			currentVolume = maxVolume;

		}

		if (currentVolume <= maxVolume && currentVolume > 0.0f) {

			bottleLiquid.transform.localScale = new Vector3(1.0f, 1.0f, currentVolume / maxVolume);

		}
		else { // currentVolume <= 0

			bottleLiquid.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);

		}
	}

	public float getMaxVolume() {

		return maxVolume;

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

}
