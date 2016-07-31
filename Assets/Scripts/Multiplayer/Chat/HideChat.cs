using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HideChat : MonoBehaviour {
	public Mask ChatHolder;
	bool isChatShowing = false;

	void Start () {
		ToggleChat ();
	}

	public void ToggleChat (){
		isChatShowing = !isChatShowing;
		if(isChatShowing){
			ChatHolder.enabled= false;
		} else {
			ChatHolder.enabled= true;

		}
	}

}
