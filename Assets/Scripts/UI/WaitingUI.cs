using UnityEngine;
using System.Collections;

public class WaitingUI : MonoBehaviour {

    [SerializeField]
    private UnityEngine.UI.Text stageNameField;

    public string StageNameToShow
    {
        set
        {
            stageNameField.text = value;
        }
    }
}
