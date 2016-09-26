using System.Collections.Generic;
using Sanicball.Data;
using Sanicball.Logic;
using SanicballCore;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    /// <summary>
    /// Tracks used control types for local players and handles them joining/leaving the lobby.
    /// </summary>
    public class LocalPlayerManager : MonoBehaviour
    {
        public LocalPlayerPanel localPlayerPanelPrefab;
        public event System.EventHandler<MatchPlayerEventArgs> LocalPlayerJoined;

        [SerializeField]
        private Text matchJoiningHelpField = null;

        private const int maxPlayers = 4;
        private MatchManager manager;
        private List<ControlType> usedControls = new List<ControlType>();

        private void Start()
        {
            manager = FindObjectOfType<MatchManager>();

            if (manager)
            {
                //Create local player panels for players already in the game
                foreach (var p in manager.Players)
                {
                    if (p.ClientGuid == manager.LocalClientGuid && p.CtrlType != ControlType.None)
                    {
                        var panel = CreatePanelForControlType(p.CtrlType, true);
                        panel.AssignedPlayer = p;
                        panel.SetCharacter(p.CharacterId);
                    }
                }

                manager.MatchPlayerAdded += Manager_MatchPlayerAdded;
            }
            else
            {
                Debug.LogWarning("Game manager not found - players cannot be added");
            }

            UpdateHelpText();
        }

        private void Update()
        {
            //This where I check if any control types are trying to join
            if (PauseMenu.GamePaused) return; //Short circuit if the game is paused
            foreach (ControlType ctrlType in System.Enum.GetValues(typeof(ControlType)))
            {
                if (GameInput.IsOpeningMenu(ctrlType))
                {
                    if (!manager
                        || usedControls.Count >= maxPlayers //Max players reached?
                        || usedControls.Contains(ctrlType)) //Control type taken?
                        return; //Fuk off

                    CreatePanelForControlType(ctrlType, false);
                }
            }
        }

        private void OnDestroy()
        {
            manager.MatchPlayerAdded -= Manager_MatchPlayerAdded;
        }

        private LocalPlayerPanel CreatePanelForControlType(ControlType ctrlType, bool alreadyJoined)
        {
            usedControls.Add(ctrlType);
            UpdateHelpText();

            //Create a new panel and assign the joining control type
            var panel = Instantiate(localPlayerPanelPrefab);

            panel.transform.SetParent(transform, false);
            //panel.transform.SetAsFirstSibling(); use to reverse direction of panels
            panel.playerManager = this;
            panel.AssignedCtrlType = ctrlType;
            panel.gameObject.SetActive(true);
            return panel;
        }

        public void CreatePlayerForControlType(ControlType ctrlType, int character)
        {
            manager.RequestPlayerJoin(ctrlType, character);
            //var newPlayer = manager.CreatePlayer(ctrlType.ToString(), ctrlType, character);
            //return newPlayer;
        }

        private void Manager_MatchPlayerAdded(object sender, MatchPlayerEventArgs e)
        {
            if (e.IsLocal)
            {
                if (LocalPlayerJoined != null)
                    LocalPlayerJoined(this, e);
            }
        }

        public void SetCharacter(MatchPlayer player, int c)
        {
            manager.RequestCharacterChange(player.CtrlType, c);
        }

        public void SetReady(MatchPlayer player, bool ready)
        {
            manager.RequestReadyChange(player.CtrlType, ready);
        }

        public void RemoveControlType(ControlType ctrlType)
        {
            usedControls.Remove(ctrlType);
            UpdateHelpText();
        }

        public void LeaveMatch(MatchPlayer player)
        {
            manager.RequestPlayerLeave(player.CtrlType);
            usedControls.Remove(player.CtrlType);
            UpdateHelpText();
        }

        private void UpdateHelpText()
        {
            bool anyLeft = usedControls.Count < maxPlayers;
            bool hasKeyboard = usedControls.Contains(ControlType.Keyboard);

            matchJoiningHelpField.text = "";
            if (anyLeft)
            {
                if (!hasKeyboard)
                {
                    matchJoiningHelpField.text += "Press <b>" + GameInput.GetKeyCodeName(ActiveData.Keybinds[Keybind.Menu]) + "</b> to join with a keyboard. ";
                }
                matchJoiningHelpField.text += "Press <b>X</b> to join with a joystick.";
            }
        }
    }
}