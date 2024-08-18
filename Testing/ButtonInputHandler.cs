using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonInputHandler : MonoBehaviour
{
   public void OnJumpButtonClicked()
	{
		Debug.Log("Jump button clicked");
		Jump();
	}

	private void Jump()
	{
		Debug.Log("Character should jump now");
	}
}
