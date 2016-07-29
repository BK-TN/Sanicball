using Sanicball.Data;
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
        private LocalPlayerInfoBox infoBox = null;

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

            infoBox.SetIcon(controlTypeIcons[(int)AssignedCtrlType]);

            //string kbButton = GameInput.GetKeyCodeName(ActiveData.Keybinds[Keybind.Menu]);
            //infoBox.SetLines("<b>" + kbButton + "</b>: Join with keyboard", "<b>Start</b>: Join with joystick");
        }

        private void Update()
        {
            //This method handles input from the assigned controltype (if any)
            if (PauseMenu.GamePaused) return; //Short circuit if paused

            if (GameInput.IsRespawning(AssignedCtrlType))
            {
                ToggleReady();
            }

            bool accept = GameInput.IsOpeningMenu(AssignedCtrlType);
            bool left = GameInput.UILeft(AssignedCtrlType);
            bool right = GameInput.UIRight(AssignedCtrlType);

            var cActive = characterSelectSubpanel.gameObject.activeSelf;

            if (accept || left || right)
            {
                if (!uiPressed)
                {
                    if (accept)
                    {
                        if (cActive)
                            characterSelectSubpanel.Accept();
                        else
                            characterSelectSubpanel.gameObject.SetActive(true);
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
                //TODO toggle ready in a better way

                //AssignedPlayer.ReadyToRace = !AssignedPlayer.ReadyToRace;
                //readyIndicator.On = AssignedPlayer.ReadyToRace;
            }
        }

        public void SetCharacter(int c)
        {
            if (AssignedPlayer == null)
            {
                Debug.Log("A player panel just requested creating a player");
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
                Debug.Log("A player panel just got a player assigned");
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
            //SetActiveSubpanel(null);
        }
    }
}