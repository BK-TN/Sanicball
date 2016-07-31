using Sanicball.Data;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Sanicball.UI
{
    /// <summary>
    /// Handles active state and input for a local player identified by its control type.
    /// </summary>
	public class LocalPlayerPanel : NetworkBehaviour
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

            infoBox.SetIcon(controlTypeIcons[(int)AssignedCtrlType]);

            //string kbButton = GameInput.GetKeyCodeName(ActiveData.Keybinds[Keybind.Menu]);
            //infoBox.SetLines("<b>" + kbButton + "</b>: Join with keyboard", "<b>Start</b>: Join with joystick");
        }

        private void Update()
        {
            //This method handles input from the assigned controltype (if any)
			if (PauseMenu.GamePaused || NetworkManager.singleton.GetComponent<SanicNetworkManager>().isSpawning) return; //Short circuit if paused

            bool accept = GameInput.IsOpeningMenu(AssignedCtrlType);
            bool left = GameInput.UILeft(AssignedCtrlType);
            bool right = GameInput.UIRight(AssignedCtrlType);

            var cActive = characterSelectSubpanel.gameObject.activeSelf;

            if (accept || left || right)
            {
                if (!uiPressed )
                {
                    if (accept)
                    {
						if (cActive){
                            characterSelectSubpanel.Accept();
						}
						else{
                            characterSelectSubpanel.gameObject.SetActive(true);
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

		public bool lefth(  bool tru){
			return tru;
		}


        public void ToggleReady()
        {
				if(NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players.Count!=0  )
            {
				if(NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.isServer){

	                AssignedPlayer.ReadyToRace = !AssignedPlayer.ReadyToRace;
	                readyIndicator.On = AssignedPlayer.ReadyToRace;

				}else{

				}
            }
        }

        public void SetCharacter(int c)
        {

			if(  NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players.Count!=0  && NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players[0].BallObject){

				if( NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players[0].BallObject.ballConnection == connectionToClient ){
						AssignedPlayer= NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.Players[0];
				}else{
					Debug.Log("A player was here before you");

				}
					
			}

			if (AssignedPlayer == null)
            {

                AssignedPlayer = playerManager.CreatePlayerForControlType(AssignedCtrlType, c);

            }
            else
            {

				if(NetworkManager.singleton.GetComponent<SanicNetworkManager>().matchManager.isServer){
					playerManager.SetCharacter(AssignedPlayer, c);

				}else{
					Debug.Log(AssignedPlayer.BallObject);

					GameObject.Find("SanicNetworkManager").GetComponent<SanicNetworkManager>().controlTipo = AssignedPlayer.CtrlType;

					GameObject.Find("SanicNetworkManager").GetComponent<SanicNetworkManager>().personaje = c;


					GameObject.Find("SanicNetworkManager").GetComponent<SanicNetworkManager>().nickName = AssignedPlayer.Name;


					AssignedPlayer.CharacterId= c;
					AssignedPlayer.BallObject.CmdSetcharacterFromBall( c );
				}


            }
            if (characterSelectSubpanel.gameObject.activeSelf)
            {
                characterSelectSubpanel.gameObject.SetActive(false);
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
    }
}