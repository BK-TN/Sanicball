using Sanicball.Data;
using Sanicball.Logic;
using SanicballCore;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class MatchSettingsDisplayPanel : MonoBehaviour
    {
        [Header("Fields")]
        public Text stageName;

        public Image stageImage;
        public Text lapCount;
        public Text aiOpponents;
        public Text aiSkill;

        private Vector3 targetStageCamPos;

        [SerializeField]
        private Animation settingsChangedAnimation = null;

        [SerializeField]
        private Camera stageLayoutCamera = null;

        private MatchManager manager;

        private void Start()
        {
            manager = FindObjectOfType<MatchManager>();
            manager.MatchSettingsChanged += Manager_MatchSettingsChanged;

            //Invoke callback immediately to set initial settings
            Manager_MatchSettingsChanged(this, System.EventArgs.Empty);
        }

        private void Manager_MatchSettingsChanged(object sender, System.EventArgs e)
        {
            MatchSettings s = manager.CurrentSettings;

            targetStageCamPos = new Vector3(s.StageId * 50, stageLayoutCamera.transform.position.y, stageLayoutCamera.transform.position.z);
            stageName.text = ActiveData.Stages[s.StageId].name;
            stageImage.sprite = ActiveData.Stages[s.StageId].picture;
            lapCount.text = s.Laps + (s.Laps == 1 ? " lap" : " laps");
            aiOpponents.text = "";
            /*foreach (var i in s.aiCharacters)
            {
                aiOpponents.text += ActiveData.Characters[i].name + "\n";
            }*/
            aiSkill.text = "AI Skill: " + s.AISkill;

            settingsChangedAnimation.Rewind();
            settingsChangedAnimation.Play();
        }

        private void Update()
        {
            if (Vector3.Distance(stageLayoutCamera.transform.position, targetStageCamPos) > 0.1f)
            {
                stageLayoutCamera.transform.position = Vector3.Lerp(stageLayoutCamera.transform.position, targetStageCamPos, Time.deltaTime * 10f);
                if (Vector3.Distance(stageLayoutCamera.transform.position, targetStageCamPos) <= 0.1f)
                {
                    stageLayoutCamera.transform.position = targetStageCamPos;
                }
            }
        }

        private void OnDestroy()
        {
            manager.MatchSettingsChanged -= Manager_MatchSettingsChanged;
        }
    }
}