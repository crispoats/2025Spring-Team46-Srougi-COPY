using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// This holds the functionality for the GloveBox
/// Prompts user to put on gloves and connects to TaskManager
/// Also swaps the default XR hand model to a blue hand model
/// @author: Colby Cress
/// </summary>
public class GloveBox : MonoBehaviour
{

	private int putOnGloves;

	[SerializeField]
	private GameObject leftGrayHand;
	[SerializeField]
	private GameObject leftBlueHand;

	[SerializeField]
	private GameObject rightGrayHand;
	[SerializeField]
	private GameObject rightBlueHand;

	void Start()
    {

        // Get the index of the task from TaskManager
        putOnGloves = GameManager.getInstance().getTaskManager().taskList.getTaskByName("Put on Gloves");

        GetComponent<XRSimpleInteractable>().selectEntered.AddListener(OnGlovesSelected);
    }
	public void changeGloves() {

		this.GetComponent<XRSimpleInteractable>().enabled = false;

		leftGrayHand.SetActive(false);
		rightGrayHand.SetActive(false);

		leftBlueHand.SetActive(true);
		rightBlueHand.SetActive(true);

	}

	private void OnGlovesSelected(SelectEnterEventArgs args)
    {
        args.manager.CancelInteractorSelection(args.interactorObject);
        if (!GameManager.getInstance().getTaskManager().isCurrentTask(putOnGloves))
			return;

        changeGloves();
        GameManager.getInstance().getTaskManager().completeTask(putOnGloves);
        Destroy(gameObject);
    }

	void OnDestroy()
    {
        var interactable = GetComponent<XRSimpleInteractable>();
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnGlovesSelected);
        }
    }
}
