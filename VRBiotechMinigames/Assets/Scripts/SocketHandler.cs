using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/**
 * @author Brayden Batts
 * @since 2025-03-04
 * 
 * Disables collision between socket object and object attached to socket
 * and reenables collision once the object is detatched from socket.
 * Also handles attaching and detaching colliders from the attached/detached
 * object to/from this socket object by cloning/destroying them.
 */
public class SocketHandler : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The socket interactor associated with this handler.")]
    /** The socket interactor to check for attachment. */
    private XRSocketInteractor interactor;

    [SerializeField]
    [Tooltip("The collider for the object this socket is attached to.")]
    /** The collider for this object to have collision disabled/enabled. */
    private Collider objectCollider;

    /** The empty GameObject that serves as the parent for all cloned attachment colliders. */
    private GameObject attachedColliderParent = null;

    private bool hasSmoothTracking = false;
    
    /**
     * When this script loads, add listeners for attachment to detachment from the socket.
     */
    void Awake()
    {
        interactor.selectEntered.AddListener(SelectEntered);
        interactor.selectExited.AddListener(SelectExited);
    }

    /**
     * If something attaches to the socket, disable collision between this object and the attached object and attach all colliders to this socket.
     */
    void SelectEntered(SelectEnterEventArgs args)
    {
        // Call the method to attach all of the colliders to this socket
        AttachColliders(args.interactableObject);
        // Disable collision between the collider on this object and all colliders of the object
        foreach (Collider other in args.interactableObject.colliders)
            Physics.IgnoreCollision(objectCollider, other, true);

		XRGrabInteractable interactable = args.interactableObject.transform.gameObject.GetComponent<XRGrabInteractable>();
		hasSmoothTracking = interactable.smoothPosition;
		interactable.smoothPosition = false;
    }

    /**
     * If something detaches from this socket, enable collision between this object and the detached object after the ease in delay and detach all colliders.
     */
    void SelectExited(SelectExitEventArgs args)
    {
        // If the parent collider object exists, destroy it to detach the colliders
        if (attachedColliderParent != null)
            Destroy(attachedColliderParent);

		XRGrabInteractable interactable = args.interactableObject.transform.gameObject.GetComponent<XRGrabInteractable>();
		interactable.smoothPosition = hasSmoothTracking;
		// Start the coroutine to enable collision with the detached object after the ease in delay
		StartCoroutine(EnableCollisionAfterDelay(args.interactableObject.colliders, args.interactableObject.transform.GetComponent<XRGrabInteractable>().attachEaseInTime));
    }

    /**
     * Attach all of the selected object's colliders to the socket object by cloning as children of an empty GameObject parented to the attach point.
     * 
     * This method is a really indirect way of doing this, but, unfortunately, my research indicates this is the only way to do it.
     */
    void AttachColliders(IXRSelectInteractable selected)
    {
        // The actual GameObject selected by the socket
        GameObject selectedObject = selected.transform.gameObject;
        // The XRGrabInteractable belonging to the selected object
        XRGrabInteractable interactable = selectedObject.GetComponent<XRGrabInteractable>();
        // The interactable's attach transform if it exists, otherwise the transform of the selected object
        Transform attachTransform = (interactable.attachTransform == null) ? selectedObject.transform : interactable.attachTransform;
        // Create a new temporary game object with no scale to replace the attach transform
        GameObject attachObject = new("Temporary Attach");
        attachObject.transform.SetPositionAndRotation(attachTransform.position, attachTransform.rotation);
        attachTransform = attachObject.transform;
        // The position of the attach transform relative to the interactable
        Vector3 attachPosition = attachTransform.InverseTransformPoint(selectedObject.transform.position);
        // The rotation of the attach transform relative to the interactable
        Quaternion attachRotation = Quaternion.Inverse(attachTransform.rotation) * selectedObject.transform.rotation;
        // Destroy the temporary game object
        Destroy(attachObject);

        // Create a new empty GameObject to parent the cloned colliders to
        attachedColliderParent = new GameObject("Attached Colliders");
        // Set its parent to be the socket's attach transform
        attachedColliderParent.transform.SetParent(interactor.attachTransform);
        // Set its position and rotation to be where the selected object will be once it finishes attaching
        attachedColliderParent.transform.SetLocalPositionAndRotation(attachPosition, attachRotation);

        // Create an empty list store only one collider per GameObject
        List<Collider> uniqueColliders = new();
        // For each collider in the selected object
        foreach (Collider colliderToAdd in selected.colliders)
        {
            // Whether or not this collider belongs to a GameObject that already exists in the list
            bool duplicate = false;
            // For each existing collider that is unique
            foreach (Collider existingCollider in uniqueColliders)
            {
                // If the candidate collider has the same GameObject as a collider already in the list
                if (colliderToAdd.gameObject == existingCollider.gameObject)
                {
                    // Mark it as a duplicate and stop checking for a duplicate
                    duplicate = true;
                    break;
                }
            }

            // If the candidate collider is a duplicate, check the next one
            if (duplicate)
                continue;

            // Otherwise, add it to the list of unique colliders
            uniqueColliders.Add(colliderToAdd);
        }

        // For each unique collider
        foreach (Collider collider in uniqueColliders)
        {
            // Instantiate a copy of that collider's GameObject and parent it to the empty attachment GameObject
            GameObject colliderObject = Instantiate(collider, attachedColliderParent.transform).gameObject;
            // Calculate the collider GameObject's position relative to the selected object
            Vector3 colliderPosition = selectedObject.transform.InverseTransformPoint(collider.transform.position);
            // Calculate the collider GameObject's rotation relative to the selected object
            Quaternion colliderRotation = Quaternion.Inverse(selectedObject.transform.rotation) * collider.transform.rotation;
            // Set the cloned collider object's position and rotation to be the same as the original will be once it finishes attaching
            colliderObject.transform.SetLocalPositionAndRotation(colliderPosition, colliderRotation);
            // Set the name of the cloned object for clarity in the editor
            colliderObject.name = "Cloned Collider";

            // For each child of the cloned collider
            foreach (Transform childTransform in colliderObject.transform)
                // Destroy the child
                Destroy(childTransform.gameObject);

            // Whether or not this script should continue trying to destroy components
            bool tryDestroyComponents = true;
            // While this script should continue trying to destroy components
            while (tryDestroyComponents)
            {
                // For now, assume that we should stop trying to destroy components
                tryDestroyComponents = false;

                // For each component in the cloned object
                foreach (Component component in colliderObject.GetComponents<Component>())
                {
                    // If the component is not a collider or the transform
                    if (!component.GetType().IsSubclassOf(typeof(Collider)) && component is not Transform)
                    {
                        // Destroy it since we don't need or want anything other than the collider
                        // This has to use DestroyImmediate since we need to check if the destroy was successful immediately
                        DestroyImmediate(component);

                        // If the destruction was successful
                        if (component == null)
                            // There might be other components dependent on this component we can only now destroy
                            tryDestroyComponents = true;
                    }
                }
            }

            // For each collider in the cloned collider object
            foreach (Collider newCollider in colliderObject.GetComponents<Collider>())
                // For each collider in the selected object
                foreach (Collider other in selected.colliders)
                    // Disable collision between the cloned collider and selected object collider
                    Physics.IgnoreCollision(newCollider, other, true);
        }
    }

    /**
     * Enable collision between the socket object and the deselected object after a specified delay in seconds.
     */
    IEnumerator EnableCollisionAfterDelay(List<Collider> colliders, float delay)
    {
        // Wait for the delay
        yield return new WaitForSeconds(delay);

        // For each collider in the deselected object
        foreach (Collider other in colliders)
            // Enable collision between the socket object and the deselected object collider
            Physics.IgnoreCollision(objectCollider, other, false);
	}
}
