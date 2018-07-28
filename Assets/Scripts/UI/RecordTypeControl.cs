using System;
using Sanicball.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class RecordTypeControl : MonoBehaviour
    {
		public Text tierField;
        public Text timeField;
        public Text characterField;
        public Text dateField;

        public void SetRecord(RaceRecord r)
        {
            if (r != null)
            {
                var timespan = TimeSpan.FromSeconds(r.Time);
                timeField.color = new Color(50 / 255f, 50 / 255f, 50 / 255f);
                timeField.text = string.Format("{0:00}:{1:00}.{2:000}", timespan.Minutes, timespan.Seconds, timespan.Milliseconds);
                characterField.text = "Set with " + ActiveData.Characters[r.Character].name;
                dateField.text = r.Date.ToString();
            }
            else
            {
                timeField.text = "No records found";
                timeField.color = new Color(0.5f, 0.5f, 0.5f);
                characterField.text = "";
                dateField.text = "";
            }
        }
    }
}