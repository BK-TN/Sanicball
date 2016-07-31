using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MessageFunctions : MonoBehaviour {

	[SerializeField] Text text;

	public void ShowMessage (string message){
		text.text = message;
	}

	public void HideMessage (){
		Destroy (gameObject);
	}

}