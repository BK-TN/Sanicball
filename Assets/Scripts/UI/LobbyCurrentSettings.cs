using Sanicball.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class LobbyCurrentSettings : MonoBehaviour
    {
        [Header("Fields")]
        public Text stageName;

        public Image stageImage;
        public Text lapCount;
        public Text aiOpponents;
        public Text aiSkill;

        [SerializeField]
        private Camera stageLayoutCamera;

        private void Update()
        {
            var manager = FindObjectOfType<MatchManager>();
            if (manager)
            {
                var s = manager.CurrentSettings;

                stageLayoutCamera.transform.position = new Vector3(s.StageId * 50, stageLayoutCamera.transform.position.y, stageLayoutCamera.transform.position.z);

                stageName.text = ActiveData.Stages[s.StageId].name;
                stageImage.sprite = ActiveData.Stages[s.StageId].picture;
                lapCount.text = s.Laps + (s.Laps == 1 ? " lap" : " laps");
                aiOpponents.text = "";
                /*foreach (var i in s.aiCharacters)
                {
                    aiOpponents.text += ActiveData.Characters[i].name + "\n";
                }*/
                aiSkill.text = "AI Skill: " + s.AISkill;
            }
        }
    }
}
