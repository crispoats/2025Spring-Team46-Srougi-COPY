using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

/*
 * 
 * @author Colby Cress
 * 
 * Contains all the information for any mistake (mistake event) to be created.
 * 
 */
public class MistakeEvent : UnityEvent<MistakeEventArgs>
{

	public static MistakeEvent instance;

	private static readonly List<string> mistakeList = new List<string> {

		"This serological looks too small...", // Serological pipette is too small for the current task
		"This serological looks too big...", // Serological pipette is too big for the current task
		"The serological is full. Any more and it will draw into the pipette aid!", // User is trying to fill the serological past its capabilities - the volume goes beyond the serological's cotton plug, and into the pipet aid
		"There's no liquid left in the serological. If I disperse any more, it will create bubbles!", // User is trying to disperse liquid from an already empty serological - this creates bubbles, which is not ideal
		"I can't lay the pipette aid down right now, the serological has liquid in it!", // User is trying to lay the pipet aid down when the attached serological pipette has liquid in it
		"I should put the cap back on the bottle...", // A bottle has been left uncapped for too long without being actively used
		"I shouldn't reuse the serological for different liquids!", // User is trying to reuse a serological for a liquid different from the previous one it was used for
		"I should make sure the serological is level, I don't want the liquid to enter the pipette aid...", // User rotated the attached serological too much, to where in real life the liquid would start entering the pipette aid
		"I shouldn't aspirate air, it will create bubbles!" // User is aspirating when there is no liquid to aspirate (i.e. air)

	};

	/// <summary>
	/// Get the description of the mistake based on the id
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public static string getMistakeDescription(int id) {

		return mistakeList[id];

	}

	/// <summary>
	/// Get the instance of the MistakeEvent
	/// </summary>
	/// <returns></returns>
	public static MistakeEvent GetInstance()
	{
		if (instance == null)
		{
			instance = new MistakeEvent();
		}

		return instance;
	}

	//public static List<string> getAllMistakes() {

	//	return mistakeList;

	//}
}

public struct MistakeEventArgs
{

	private Transform transform;
	private Vector3 offset;
	private int messageKey;

	public MistakeEventArgs(Transform transform, Vector3 offset, int messageKey)
	{

		this.transform = transform;
		this.offset = offset;
		this.messageKey = messageKey;

	}

	public Transform getTransform()
	{

		return transform;

	}

	public Vector3 getOffset()
	{

		return offset;

	}

	public int getMessageKey()
	{

		return messageKey;

	}
}
