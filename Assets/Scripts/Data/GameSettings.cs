using UnityEngine;

namespace Sanicball.Data
{
    [System.Serializable]
    public class GameSettings
    {
        [Header("Online")]
        public string nickname = "";
		public string serverListURL = "https://sanicball.bdgr.zone/servers/";

        public string gameJoltUsername;
        public string gameJoltToken;

        [Header("Display")]
        public int resolution = 0;

        public bool fullscreen = true;
        public bool vsync = false;
        public bool useImperial = false;
        public bool showControlsWhileWaiting = true;

        [Header("Graphics")]
        public int aa = 0;

        public bool trails = true;
        public bool shadows = true;
        public bool motionBlur = false;
        public bool bloom = false;
		public ReflectionQuality reflectionQuality = ReflectionQuality.Off;
        public bool eSportsReady = false;

        [Header("Gameplay")]
        public bool useOldControls = false;

        public float oldControlsMouseSpeed = 3f;
        public float oldControlsKbSpeed = 10f;

        [Header("Audio")]
        public float soundVolume = 1f;

        public bool music = true;
        public bool fastMusic = true;

        public GameSettings()
        {
        }

        public void CopyValues(GameSettings original)
        {
            nickname = original.nickname;
			serverListURL = original.serverListURL;
            gameJoltUsername = original.gameJoltUsername;
            gameJoltToken = original.gameJoltToken;

            resolution = original.resolution;
            fullscreen = original.fullscreen;
            vsync = original.vsync;
            useImperial = original.useImperial;
            showControlsWhileWaiting = original.showControlsWhileWaiting;

            aa = original.aa;
            trails = original.trails;
            shadows = original.shadows;
            motionBlur = original.motionBlur;
            bloom = original.bloom;
            reflectionQuality = original.reflectionQuality;
            eSportsReady = original.eSportsReady;

            useOldControls = original.useOldControls;
            oldControlsMouseSpeed = original.oldControlsMouseSpeed;
            oldControlsKbSpeed = original.oldControlsKbSpeed;

            soundVolume = original.soundVolume;
            music = original.music;
            fastMusic = original.fastMusic;
        }

        //Since settings are saved and the user can modify them externally,
        //they should be validated when loaded
        public void Validate()
        {
            //Resolution
            if (resolution >= Screen.resolutions.Length)
                resolution = Screen.resolutions.Length - 1;
            if (resolution < 0)
                resolution = 0;
            //AA
            if (aa != 0 && aa != 2 && aa != 4 && aa != 8)
                aa = 0;
            //Mouse speed
            oldControlsMouseSpeed = Mathf.Clamp(oldControlsMouseSpeed, 0.5f, 10f);
            //KB speed
            oldControlsKbSpeed = Mathf.Clamp(oldControlsKbSpeed, 0.5f, 10f);
            //Sound volume
            soundVolume = Mathf.Clamp(soundVolume, 0f, 1f);
        }

        public void Apply(bool changeWindow)
        {
            if (changeWindow)
            {
                //Resolution and fullscreen
                if (resolution < Screen.resolutions.Length)
                {
                    Resolution res = Screen.resolutions[(int)resolution];
                    if (Screen.width != res.width || Screen.height != res.height || fullscreen != Screen.fullScreen)
                        Screen.SetResolution(res.width, res.height, fullscreen);
                }
            }
            //AA
            QualitySettings.antiAliasing = aa;
            //Vsync
            if (vsync) { QualitySettings.vSyncCount = 1; } else { QualitySettings.vSyncCount = 0; }
            //Shadows
            GameObject dl = GameObject.Find("Directional light");
            if (dl != null)
            {
                LightShadows ls;
                if (shadows) { ls = LightShadows.Hard; } else { ls = LightShadows.None; }
                dl.GetComponent<Light>().shadows = ls;
            }
            //Volume
            AudioListener.volume = soundVolume;
            //Mute
            MusicPlayer music = GameObject.FindObjectOfType<MusicPlayer>();
            if (music)
                music.GetComponent<AudioSource>().mute = !music;
            //Camera effects
            foreach(var cam in GameObject.FindObjectsOfType<CameraEffects>())
            {
                cam.EnableEffects();
            }
        }
    }

    public enum ReflectionQuality
    {
        Off,
        Low,
        Medium,
        High
    }
}