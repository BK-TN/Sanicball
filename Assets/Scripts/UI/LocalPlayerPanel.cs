using Sanicball.Data;
using Sanicball.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    /// <summary>
    /// Handles active state and input for a local player identified by its control type.
    /// </summary>
    public class LocalPlayerPanel : MonoBehaviour
    {
        [System.NonSerialized]
        public LocalPlayerManager playerManager;

        public ImageColorToggle readyIndicator;

        [SerializeField]
        private Image ctrlTypeImageField = null;

        [SerializeField]
        private Text helpTextField = null;

        [SerializeField]
        private Sprite[] controlTypeIcons;

        [SerializeField]
        private CharacterSelectPanel characterSelectSubpanel = null;

        public ControlType AssignedCtrlType { get; set; }
        public MatchPlayer AssignedPlayer { get; set; }

        private bool uiPressed = false;

        private void Start()
        {
            characterSelectSubpanel.CharacterSelected += CharacterSelectSubpanel_CharacterSelected;
            characterSelectSubpanel.CancelSelected += CharacterSelectSubpanel_Cancelled;

            playerManager.LocalPlayerJoined += PlayerManager_LocalPlayerJoined;

            ctrlTypeImageField.sprite = controlTypeIcons[(int)AssignedCtrlType];

            ShowCharacterSelectHelp();
        }

        private void Update()
        {
            //This method handles input from the assigned controltype (if any)
            if (PauseMenu.GamePaused) return; //Short circuit if paused
            bool accept = GameInput.IsOpeningMenu(AssignedCtrlType);
            bool left = GameInput.UILeft(AssignedCtrlType);
            bool right = GameInput.UIRight(AssignedCtrlType);

            var cActive = characterSelectSubpanel.gameObject.activeSelf;

            if (GameInput.IsRespawning(AssignedCtrlType) && !cActive)
            {
                ToggleReady();
            }

            if (accept || left || right)
            {
                if (!uiPressed)
                {
                    if (accept)
                    {
                        if (cActive)
                        {
                            characterSelectSubpanel.Accept();
                            ShowMainHelp();
                        }
                        else
                        {
                            characterSelectSubpanel.gameObject.SetActive(true);
                            ShowCharacterSelectHelp();
                        }
                    }
                    if (left && cActive)
                    {
                        characterSelectSubpanel.PrevCharacter();
                    }
                    if (right && cActive)
                    {
                        characterSelectSubpanel.NextCharacter();
                    }
                    uiPressed = true;
                }
            }
            else
            {
                uiPressed = false;
            }
        }

        public void LeaveMatch()
        {
            if (AssignedPlayer == null) return;

            playerManager.LeaveMatch(AssignedPlayer);
            Destroy(gameObject);
        }

        public void ToggleReady()
        {
            if (AssignedPlayer != null)
            {
                readyIndicator.On = !AssignedPlayer.ReadyToRace;
                playerManager.SetReady(AssignedPlayer, !AssignedPlayer.ReadyToRace);
            }
        }

        public void SetCharacter(int c)
        {
            if (AssignedPlayer == null)
            {
                playerManager.CreatePlayerForControlType(AssignedCtrlType, c);
            }
            else
            {
                playerManager.SetCharacter(AssignedPlayer, c);
            }
            if (characterSelectSubpanel.gameObject.activeSelf)
            {
                characterSelectSubpanel.gameObject.SetActive(false);
            }
        }

        private void PlayerManager_LocalPlayerJoined(object sender, MatchPlayerEventArgs e)
        {
            if (e.Player.CtrlType == AssignedCtrlType)
            {
                AssignedPlayer = e.Player;
            }
        }

        private void CharacterSelectSubpanel_CharacterSelected(object sender, CharacterSelectionArgs e)
        {
            SetCharacter(e.SelectedCharacter);
            characterSelectSubpanel.gameObject.SetActive(false);
        }

        private void CharacterSelectSubpanel_Cancelled(object sender, System.EventArgs e)
        {
            if (AssignedPlayer == null)
            {
                playerManager.RemoveControlType(AssignedCtrlType);
                Destroy(gameObject);
                return;
            }
            else
            {
                LeaveMatch();
            }
        }

        private void ShowMainHelp()
        {
            string kbConfirm = GameInput.GetKeyCodeName(ActiveData.Keybinds[Keybind.Menu]);
            const string joyConfirm = "X";
            string kbAction = GameInput.GetKeyCodeName(ActiveData.Keybinds[Keybind.Respawn]);
            const string joyAction = "Y";

            string confirm = AssignedCtrlType == ControlType.Keyboard ? kbConfirm : joyConfirm;
            string action = AssignedCtrlType == ControlType.Keyboard ? kbAction : joyAction;

            helpTextField.text = string.Format("<b>{0}</b>: Change character\n<b>{1}</b>: Toggle ready to play", confirm, action);
        }

        private void ShowCharacterSelectHelp()
        {
            const string kbLeft = "Left";
            const string joyLeft = kbLeft;
            const string kbRight = "Right";
            const string joyRight = kbRight;
            string kbConfirm = GameInput.GetKeyCodeName(ActiveData.Keybinds[Keybind.Menu]);
            const string joyConfirm = "X";

            string left = AssignedCtrlType == ControlType.Keyboard ? kbLeft : joyLeft;
            string right = AssignedCtrlType == ControlType.Keyboard ? kbRight : joyRight;
            string confirm = AssignedCtrlType == ControlType.Keyboard ? kbConfirm : joyConfirm;

            helpTextField.text = string.Format("<b>{0}</b>/<b>{1}</b>: Select character\n<b>{2}</b>: Confirm", left, right, confirm);
        }
    }
}