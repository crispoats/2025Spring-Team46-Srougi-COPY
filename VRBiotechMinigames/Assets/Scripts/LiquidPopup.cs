using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

/**
 * 
 * @author Colby Cress
 * 
 * Updates the Text object to show the current volume of the serological
 * 
 */
public class LiquidPopup : MonoBehaviour
{

	// The text to display
	[SerializeField]
	private TextMeshProUGUI text;

    void Update() {

		XRSocketInteractor interactor = this.transform.parent.GetComponentInChildren<XRSocketInteractor>();
		// If there is no serological attached, don't show the volume text
		if (interactor.interactablesSelected.Count == 0) {

			GetComponent<Canvas>().enabled = false;
			return;

		}
		XRGrabInteractable interactable = (XRGrabInteractable)interactor.interactablesSelected[0];

		Serological serologicalScript = interactable.GetComponentInChildren<Serological>();

		GetComponent<Canvas>().enabled = true;

		text.text = serologicalScript.getCurrentVolume().ToString("0.00");

	}
}
