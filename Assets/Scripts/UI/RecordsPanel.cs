using System.Linq;
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
            UpdateFields();
            UpdateStageName();
        }

        private void UpdateFields()
        {
            var records = ActiveData.RaceRecords.Where(a => a.Stage == selectedStage && a.GameVersion == GameVersion.AS_FLOAT && a.WasTesting == GameVersion.IS_TESTING).OrderBy(a => a.Time);

            var bestLapRecord = records.Where(a => a.Type == RecordType.Lap).FirstOrDefault();
            lapRecord.SetRecord(bestLapRecord);

            var bestHyperspeedLapRecord = records.Where(a => a.Type == RecordType.HyperspeedLap).FirstOrDefault();
            hyperspeedLapRecord.SetRecord(bestHyperspeedLapRecord);
        }

        private void UpdateStageName()
        {
            stageNameField.text = ActiveData.Stages[selectedStage].name;
            UpdateFields();
        }
    }
}