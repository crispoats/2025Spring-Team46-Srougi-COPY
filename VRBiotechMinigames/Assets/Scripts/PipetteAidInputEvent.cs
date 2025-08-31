using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

/*
 * 
 * @author Siri Mudunuri
 * 
 * Contains all the information for a event to be created to pass custom controller information.
 * 
 */
public class PipetteAidInputEvent : UnityEvent<string, float>
{

	public static PipetteAidInputEvent instance;

	/// Get the instance of the PipetteAidInputEvent
	public static PipetteAidInputEvent GetInstance()
	{
		if (instance == null)
		{
			instance = new PipetteAidInputEvent();
		}
		return instance;
	}
}
