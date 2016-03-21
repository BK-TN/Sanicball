using Sanicball.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class StageSelection : MonoBehaviour
    {
        public Text title;
        public Transform stageList;
        public Vector3 stageOverviewSpawnPosition;

        public StageImage stageImagePrefab;

        private GameObject currentStageOverview;

        public void SetActiveStage(StageInfo s)
        {
            title.text = s.name;
            if (currentStageOverview != null)
            {
                Destroy(currentStageOverview);
            }
            currentStageOverview = Instantiate(s.overviewPrefab, stageOverviewSpawnPosition, Quaternion.Euler(270, 0, 0)) as GameObject;
        }

        private void Start()
        {
            //Add all stages to list
            for (int i = 0; i < ActiveData.Stages.Length; i++)
            {
                StageInfo s = ActiveData.Stages[i];
                StageImage simg = Instantiate(stageImagePrefab);
                simg.transform.SetParent(stageList, false);
                simg.GetComponent<Image>().sprite = s.picture;
                simg.onSelect += () =>
                {
                    SetActiveStage(s);
                };
                //if (i == 0) {
                //	simg.GetComponent<Button>().Select();
                //}
            }
        }
    }
}
