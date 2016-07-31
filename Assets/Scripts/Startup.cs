using Sanicball.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball
{
    public class Startup : MonoBehaviour
    {
        public UI.Intro intro;
        public CanvasGroup setNicknameGroup;
        public InputField nicknameField;

        [SerializeField]
        private bool skipNickname = false;

        public void ValidateNickname()
        {
            if (nicknameField.text.Trim() != "")
            {
                setNicknameGroup.alpha = 0f;
                ActiveData.GameSettings.nickname = nicknameField.text;
				PlayerPrefs.SetString("nickname",ActiveData.GameSettings.nickname);
                intro.enabled = true;
            }
        }
		public void cambio(bool valor){

			int v=0;
			if( valor== true  )
				v=1;
			if(valor==false)
				v=0;
			PlayerPrefs.SetInt("skipNickname",v);
//			skipNickname= valor;

		}
        private void Start()
        {
//			PlayerPrefs.DeleteKey("skipNickname");

			PlayerPrefs.SetString("nickname",ActiveData.GameSettings.nickname);

			nicknameField.text =ActiveData.GameSettings.nickname;
			if (PlayerPrefs.GetInt("skipNickname")==1)
            {
				UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");

//                ActiveData.GameSettings.nickname = "Player";
			}else{

				setNicknameGroup.alpha = 1f;

			}



            if (string.IsNullOrEmpty(ActiveData.GameSettings.nickname))
            {
                //Set nickname before continuing
                setNicknameGroup.alpha = 1f;
            }
//            else
//            {
//                setNicknameGroup.alpha = 0f;
//                intro.enabled = true;
//            }
        }
    }
}