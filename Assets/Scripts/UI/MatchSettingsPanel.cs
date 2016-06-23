using Sanicball.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    [RequireComponent(typeof(ToggleCanvasGroup))]
    public class MatchSettingsPanel : MonoBehaviour
    {
        [SerializeField]
        private GameObject firstActive;

        [Header("Fields")]
        [SerializeField]
        private Text stage;
        [SerializeField]
        private Text laps;
        [SerializeField]
        private Text aiCount;
        [SerializeField]
        private Text aiSkill;
        [SerializeField]
        private Text[] aiCharacters;

        private MatchSettings tempSettings = MatchSettings.CreateDefault();

        public void Show()
        {
            var canvasGroup = GetComponent<ToggleCanvasGroup>();
            canvasGroup.Show();
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(firstActive);
        }

        public void Hide()
        {
            var canvasGroup = GetComponent<ToggleCanvasGroup>();
            canvasGroup.Hide();
        }

        public void RevertSettings()
        {
            var manager = FindObjectOfType<MatchManager>();
            if (manager)
            {
                tempSettings = manager.CurrentSettings;
            }
            UpdateUiFields();
        }

        public void SaveSettings()
        {
            var manager = FindObjectOfType<MatchManager>();
            if (manager)
            {
                manager.RequestSettingsChange(tempSettings);
                ActiveData.MatchSettings = tempSettings;
            }
        }

        public void DefaultSettings()
        {
            tempSettings = new MatchSettings();
            UpdateUiFields();
        }

        public void IncrementLaps()
        {
            if (tempSettings.Laps < 6)
                tempSettings.Laps++;
            else
                tempSettings.Laps = 1;
            UpdateUiFields();
        }

        public void DecrementLaps()
        {
            if (tempSettings.Laps > 1) tempSettings.Laps--;
            else
                tempSettings.Laps = 6;
            UpdateUiFields();
        }

        public void IncrementStage()
        {
            if (tempSettings.StageId < ActiveData.Stages.Length - 1) tempSettings.StageId++;
            else
                tempSettings.StageId = 0;
            UpdateUiFields();
        }

        public void DecrementStage()
        {
            if (tempSettings.StageId > 0) tempSettings.StageId--;
            else
                tempSettings.StageId = ActiveData.Stages.Length - 1;
            UpdateUiFields();
        }

        public void IncrementAICount()
        {
            if (tempSettings.AICount < 12)
                tempSettings.AICount++;
            else
                tempSettings.AICount = 0;
            UpdateUiFields();
        }

        public void DecrementAICount()
        {
            if (tempSettings.AICount > 0)
                tempSettings.AICount--;
            else
                tempSettings.AICount = 12;
            UpdateUiFields();
        }

        public void IncrementAISkill()
        {
            if ((int)tempSettings.AISkill < System.Enum.GetNames(typeof(AISkillLevel)).Length - 1)
            {
                tempSettings.AISkill++;
            }
            UpdateUiFields();
        }

        public void DecrementAISkill()
        {
            if (tempSettings.AISkill > 0)
            {
                tempSettings.AISkill--;
            }
            UpdateUiFields();
        }

        public void IncrementAICharacter(int pos)
        {
            int characterId = tempSettings.GetAICharacter(pos);
            characterId++;
            if (characterId >= ActiveData.Characters.Length)
            {
                characterId = 0;
            }

            tempSettings.SetAICharacter(pos, characterId);
            UpdateUiFields();
        }

        public void DecrementAICharacter(int pos)
        {
            int characterId = tempSettings.GetAICharacter(pos);
            characterId--;
            if (characterId < 0)
            {
                characterId = ActiveData.Characters.Length - 1;
            }

            tempSettings.SetAICharacter(pos, characterId);
            UpdateUiFields();
        }

        private void UpdateUiFields()
        {
            stage.text = ActiveData.Stages[tempSettings.StageId].name;
            laps.text = tempSettings.Laps.ToString();
            aiCount.text = tempSettings.AICount == 0 ? "None" : tempSettings.AICount.ToString();
            aiSkill.text = tempSettings.AISkill.ToString();
            for (int i = 0; i < aiCharacters.Length; i++)
            {
                aiCharacters[i].text = ActiveData.Characters[tempSettings.GetAICharacter(i)].name;
            }
        }

        private void Start()
        {
            RevertSettings();
            UpdateUiFields();
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(firstActive);
        }
    }
}