using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIHider : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    // Start is called before the first frame update
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HideUI(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
			if (canvasGroup.alpha == 1)
            {
				canvasGroup.alpha = 0;
				canvasGroup.interactable = false;
				canvasGroup.blocksRaycasts = false;
			}
			else
            {
				canvasGroup.alpha = 1;
				canvasGroup.interactable = true;
				canvasGroup.blocksRaycasts = true;
			}
		}
	}
}
