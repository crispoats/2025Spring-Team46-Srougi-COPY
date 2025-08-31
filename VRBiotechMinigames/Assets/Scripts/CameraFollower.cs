using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * @author Brayden Batts
 * @since 2025-02-07
 * 
 * Ensures the GameObject always faces towards the Main Camera.
 * Also keeps the GameObject in the same position relative to its
 * parent transform, irrespective of that parent's rotation.
 */
public class CameraFollower : MonoBehaviour
{
    /** The offset of this GameObject relative to its parent. */
    private Vector3 offsetFromParent;

    /**
     * When this object first awakes, determine its offset relative to its parent.
     */
    void Awake()
    {
        // The difference between this object and its parent's global position
        offsetFromParent = transform.position - transform.parent.position;
    }

    /**
     * Each update cycle, fix this object's position relative to its parent
     * and fix its rotation to face towards the main camera.
     */
    void Update()
    {
        // Set the position relative to its parent's position, ignoring its rotation.
        transform.position = transform.parent.position + offsetFromParent;

        // Turn the object to face the Camera
        transform.LookAt(Camera.main.transform);
        transform.forward *= -1;
    }
}
