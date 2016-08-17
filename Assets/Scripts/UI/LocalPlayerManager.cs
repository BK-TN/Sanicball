using System.Collections.Generic;
using Sanicball.Data;
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
        public event System.EventHandler<Match.MatchPlayerEventArgs> LocalPlayerJoined;

        private const int maxPlayers = 4;
        private Match.MatchManager manager;
        private List<ControlType> usedControls = new List<ControlType>();

        private void Start()
        {
            manager = FindObjectOfType<Match.MatchManager>();

            if (manager)
            {
                //Create local player panels for players already in the game
                foreach (var p in manager.Players)
                {
                    if (p.ClientGuid == manager.LocalClientGuid && p.CtrlType != ControlType.Remote)
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

        private LocalPlayerPanel CreatePanelForControlType(ControlType ctrlType, bool alreadyJoined)
        {
            usedControls.Add(ctrlType);

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

        private void Manager_MatchPlayerAdded(object sender, Match.MatchPlayerEventArgs e)
        {
            if (e.IsLocal)
            {
                if (LocalPlayerJoined != null)
                    LocalPlayerJoined(this, e);
            }
        }

        public void SetCharacter(Match.MatchPlayer player, int c)
        {
            manager.RequestCharacterChange(player.CtrlType, c);
        }

        public void SetReady(Match.MatchPlayer player, bool ready)
        {
            manager.RequestReadyChange(player.CtrlType, ready);
        }

        public void RemoveControlType(ControlType ctrlType)
        {
            usedControls.Remove(ctrlType);
        }

        public void LeaveMatch(Match.MatchPlayer player)
        {
            manager.RequestPlayerLeave(player.CtrlType);
            usedControls.Remove(player.CtrlType);
        }
    }
}