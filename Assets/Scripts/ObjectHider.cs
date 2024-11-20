using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectHider : MonoBehaviour
{
	public void HideObject(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			// Hide the object
			gameObject.SetActive(!gameObject.activeSelf);
		}
	}
}
