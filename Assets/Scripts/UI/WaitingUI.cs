using System.Collections;
using UnityEngine;

public class WaitingUI : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.UI.Text stageNameField;

    [SerializeField]
    private UnityEngine.UI.Text infoField;

    public string StageNameToShow
    {
        set
        {
            stageNameField.text = value;
        }
    }

    public string InfoToShow
    {
        set
        {
            infoField.text = value;
        }
    }
}