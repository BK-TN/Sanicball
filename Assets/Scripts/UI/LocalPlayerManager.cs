using System.Collections.Generic;
using Sanicball.Data;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Sanicball.UI
{
    /// <summary>
    /// Tracks used control types for local players and handles them joining/leaving the lobby.
    /// </summary>
	public class LocalPlayerManager : NetworkBehaviour
    {
        public LocalPlayerPanel localPlayerPanelPrefab;
		public LocalPlayerPanelLocal localPlayerPanelLocalPrefab;


        private const int maxPlayers = 4;
        private MatchManager manager;
		private MatchManagerLocal managerLocal;

        private List<ControlType> usedControls = new List<ControlType>();
	



        private void Start()
        {

			if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Lobby")
				manager = FindObjectOfType<MatchManager>();

			if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "LobbyLocal")
				managerLocal = FindObjectOfType<MatchManagerLocal>();
			
			if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Lobby"){
				
				if (manager && manager.Players.Count ==0 )
	            {
	                //Create local player panels for players already in the game
	                foreach (var p in manager.Players)
					{				
						
	                    if (p.CtrlType != ControlType.None)
	                    {
							Debug.Log("O sea como hay,,, corre?? pfft..");
	                        var panel = CreatePanelForControlType(p.CtrlType, true);
	                        panel.AssignedPlayer = p;
							panel.SetCharacter(p.CharacterId);
	                    }
	                }
	            }
	            else
	            {
	                Debug.LogWarning("Game manager not found - players cannot be added");
	            }

			}

			if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "LobbyLocal"){
				
				if (managerLocal  )
				{
					//Create local player panels for players already in the game
					foreach (var p in managerLocal.Players)
					{				

						if (p.CtrlType != ControlType.None)
						{
							var panel = CreatePanelForControlTypeLocal(p.CtrlType, true);
							panel.AssignedPlayer = p;
							panel.SetCharacter(p.CharacterId);
						}
					}
				}
				else
				{
					Debug.LogWarning("Game manager not found - players cannot be added");
				}


			}
        }

		public void InitManager(){
			
			manager = FindObjectOfType<MatchManager>();

			if (manager)
			{
				//Create local player panels for players already in the game
				foreach (var p in manager.Players)
				{
					if (p.CtrlType != ControlType.None)
					{
						Debug.Log("OOOhh,, initmanager,, oh aja..");
						var panel = CreatePanelForControlType(p.CtrlType, true);

						panel.AssignedPlayer = p;
						Debug.Log("asignado...");
						panel.SetCharacter(p.CharacterId);
					}
				}
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

					if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Lobby"){
						

	                    if (!manager
	                        || usedControls.Count >= maxPlayers //Max players reached?
	//						|| usedControls.Contains(ControlType.Joystick1)) //Control type taken?

	                        || usedControls.Contains(ctrlType)) //Control type taken?
	                        return; //Fuk off

	                    CreatePanelForControlType(ctrlType, false);
	//					CreatePanelForControlType(ControlType.Joystick1, false);
					}


					if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "LobbyLocal"){

						if (!managerLocal
							|| usedControls.Count >= maxPlayers //Max players reached?
							//						|| usedControls.Contains(ControlType.Joystick1)) //Control type taken?

							|| usedControls.Contains(ctrlType)) //Control type taken?
							return; //Fuk off

						CreatePanelForControlTypeLocal(ctrlType, false);
						//					CreatePanelForControlType(ControlType.Joystick1, false);

					}
						

                }
            }
        }


		// añadir el panel de seleccion de personajes
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


		private LocalPlayerPanelLocal CreatePanelForControlTypeLocal(ControlType ctrlType, bool alreadyJoined)
		{
			usedControls.Add(ctrlType);

			//Create a new panel and assign the joining control type
			var panel = Instantiate(localPlayerPanelLocalPrefab);

			panel.transform.SetParent(transform, false);
			//panel.transform.SetAsFirstSibling(); use to reverse direction of panels
			panel.playerManager = this;
			panel.AssignedCtrlType = ctrlType;
			panel.gameObject.SetActive(true);
			return panel;
		}

        public MatchPlayer CreatePlayerForControlType(ControlType ctrlType, int character)
        {
			MatchPlayer newPlayer;
			newPlayer = manager.CreatePlayer(Data.ActiveData.GameSettings.nickname, ctrlType, character);

            return newPlayer;
        }


		public MatchPlayerLocal CreatePlayerForControlTypeLocal(ControlType ctrlType, int character)
		{
			MatchPlayerLocal newPlayer;

			newPlayer = managerLocal.CreatePlayer(Data.ActiveData.GameSettings.nickname, ctrlType, character);

			return newPlayer;
		}

        public void RemoveControlType(ControlType ctrlType)
        {
            usedControls.Remove(ctrlType);
        }

        public void LeaveMatch(MatchPlayer player)
        {
            manager.RemovePlayer(player);
            usedControls.Remove(player.CtrlType);
        }


		public void LeaveMatchLocal(MatchPlayerLocal player)
		{
			managerLocal.RemovePlayer(player);
			usedControls.Remove(player.CtrlType);
		}

        public void SetCharacter(MatchPlayer player, int c)
        {
            manager.SetCharacter(player, c);
        }


		public void SetCharacterLocal(MatchPlayerLocal player, int c)
		{
			managerLocal.SetCharacter(player, c);
		}


    }
}