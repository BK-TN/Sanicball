using System.Linq;
using System.Collections.Generic;
using Sanicball.Data;
using SanicballCore;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class RecordsPanel : MonoBehaviour
    {
        public Text stageNameField;
        public RecordTypeControl lapRecord;
        public RecordTypeControl hyperspeedLapRecord;

		public RecordTypeControl recordTypeControlPrefab;
		public RectTransform sectionHeaderPrefab;
		public RectTransform recordTypeContainer; 

		private List<RecordTypeControl> recordTypes = new List<RecordTypeControl> ();

        private int selectedStage = 0;

        public void IncrementStage()
        {
            selectedStage++;
            if (selectedStage >= ActiveData.Stages.Length)
            {
                selectedStage = 0;
            }
            UpdateStageName();
        }

        public void DecrementStage()
        {
            selectedStage--;
            if (selectedStage < 0)
            {
                selectedStage = ActiveData.Stages.Length - 1;
            }
            UpdateStageName();
        }

        private void Start()
        {
			foreach (CharacterTier tier in System.Enum.GetValues (typeof(CharacterTier)).Cast<CharacterTier>())
			{
				RectTransform header = Instantiate(sectionHeaderPrefab);
				header.GetComponentInChildren<Text>().text = tier.ToString () + " balls"; //yes
				header.SetParent (recordTypeContainer, false);

				RecordTypeControl ctrl = Instantiate (recordTypeControlPrefab);
				ctrl.transform.SetParent (recordTypeContainer, false);
				ctrl.titleField.text = "Lap record";
				recordTypes.Add (ctrl);
			}

            UpdateFields();
            UpdateStageName();
        }

        private void UpdateFields()
        {
            var records = ActiveData.RaceRecords.Where(a => a.Stage == selectedStage && a.GameVersion == GameVersion.AS_FLOAT && a.WasTesting == GameVersion.IS_TESTING).OrderBy(a => a.Time);

			for (int i = 0; i < recordTypes.Count (); i++) {
				var ctrl = recordTypes [i];
				var bestLapRecord = records.Where (a => a.Tier == (CharacterTier)i).FirstOrDefault();
				ctrl.SetRecord (bestLapRecord);
			}

//            var bestLapRecord = records.Where(a => a.Type == RecordType.Lap).FirstOrDefault();
//            lapRecord.SetRecord(bestLapRecord);
//
//            var bestHyperspeedLapRecord = records.Where(a => a.Type == RecordType.HyperspeedLap).FirstOrDefault();
//            hyperspeedLapRecord.SetRecord(bestHyperspeedLapRecord);
        }

        private void UpdateStageName()
        {
            stageNameField.text = ActiveData.Stages[selectedStage].name;
            UpdateFields();
        }
    }
}