using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ChatBoxFunctions : MonoBehaviour {
//	[SerializeField] ContentSizeFitter contentSizeFitter;
	[SerializeField] Text showHideButtonText;
	[SerializeField] Transform messageParentPanel;
	[SerializeField] GameObject newMessagePrefab;

	bool isChatShowing = false;
	string message = "";

//	void Start () {
//		ToggleChat ();
//
//	}
//
//	public void ToggleChat (){
//		isChatShowing = !isChatShowing;
//		if(isChatShowing){
//			contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
//			showHideButtonText.text = "Hide Chat";
//		} else {
//			contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;
//			showHideButtonText.text = "Show Chat";
//		}
//	}

//	public void SetMessage (string message){
//		this.message = message;
//	}

	public void ShowMessage (string messa){
		if(messa != ""){
			GameObject clone = (GameObject) Instantiate (newMessagePrefab);
			clone.transform.SetParent (messageParentPanel);
			clone.transform.SetSiblingIndex (messageParentPanel.childCount - 1);
			clone.GetComponent<RectTransform>().localScale= new Vector3(1,1,1);

			clone.GetComponent<MessageFunctions>().ShowMessage ( messa);
		}
	}
}