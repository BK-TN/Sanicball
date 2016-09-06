using System.Collections;
using Sanicball.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class ScoreboardEntry : MonoBehaviour
    {
        [SerializeField]
        private Text positionField = null;
        [SerializeField]
        private Image iconField = null;
        [SerializeField]
        private Text nameField = null;
        [SerializeField]
        private Text timeField = null;

        public void Init(RacePlayer player)
        {
            RaceFinishReport report = player.FinishReport;
            if (report != null)
            {
                if (report.Position != RaceFinishReport.DISQUALIFIED_POS)
                {
                    positionField.text = Utils.GetPosString(report.Position);
                }
                else
                {
                    positionField.color = Color.red;
                    positionField.text = "DSQ";
                }

                timeField.text = Utils.GetTimeString(report.Time);
            }
            else
            {
                positionField.text = "???";

                timeField.text = "Still racing";
            }

            iconField.sprite = Data.ActiveData.Characters[player.Character].icon;
            nameField.text = player.Name;
        }
    }
}