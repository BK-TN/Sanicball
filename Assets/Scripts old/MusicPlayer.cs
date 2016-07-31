using Sanicball;
using Sanicball.Data;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    public GUISkin skin;

    public bool startPlaying = false;
    public bool fadeIn = false;

    public Song[] playlist;
    public AudioClip mlgSong;
    public AudioSource fastSource;

    [System.NonSerialized]
    public bool fastMode = false;

    private int currentSongID;
    private bool isPlaying;
    private string currentSongCredits;

    //Song credits
    private float timer = 0;

    private float slidePosition;
    private float slidePositionMax = 20;

    public void Play()
    {
        Play(playlist[currentSongID].name);
    }

    public void Play(string credits)
    {
        if (!ActiveData.GameSettings.music) return;
        currentSongCredits = "Now playing: " + credits;
        if (FindObjectOfType<MlgMode>() != null)
        {//IS MLG MODE
            GetComponent<AudioSource>().clip = mlgSong;
            currentSongCredits = "Now playing: old memes";
            FindObjectOfType<MlgMode>().StartTheShit();//Start the wubs
        }
        isPlaying = true;
        if (!GetComponent<AudioSource>().mute)
        {
            timer = 8;
        }
        GetComponent<AudioSource>().Play();
    }

    public void Pause()
    {
        GetComponent<AudioSource>().Pause();
        isPlaying = false;
    }

    private void Start()
    {
        slidePosition = slidePositionMax;
        ShuffleSongs();
        GetComponent<AudioSource>().clip = playlist[0].clip;
        currentSongID = 0;
        isPlaying = GetComponent<AudioSource>().isPlaying;
        if (startPlaying && ActiveData.GameSettings.music)
        {
            Play();
        }
        if (fadeIn)
        {
            GetComponent<AudioSource>().volume = 0f;
        }
        if (!ActiveData.GameSettings.music)
        {
            fastSource.Stop();
        }
    }

    private void Update()
    {
        if (fadeIn && GetComponent<AudioSource>().volume < 0.5f)
        {
            GetComponent<AudioSource>().volume = Mathf.Min(GetComponent<AudioSource>().volume + Time.deltaTime * 0.1f, 0.5f);
        }
        //If it's not playing but supposed to play, change song
		if ((!GetComponent<AudioSource>().isPlaying || GameInput.IsChangingSong() ) && isPlaying)
        {
            if (currentSongID < playlist.Length - 1)
            {
                currentSongID++;
            }
            else {
                currentSongID = 0;
            }
            GetComponent<AudioSource>().clip = playlist[currentSongID].clip;
            slidePosition = slidePositionMax;
            Play();
        }
        //Timer
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }

        if (fastMode && fastSource.volume < 1)
        {
            fastSource.volume = Mathf.Min(1, fastSource.volume + Time.deltaTime * 0.25f);
            GetComponent<AudioSource>().volume = 0.5f - fastSource.volume / 2;
        }
        if (!fastMode && fastSource.volume > 0)
        {
            fastSource.volume = Mathf.Max(0, fastSource.volume - Time.deltaTime * 0.5f);
            GetComponent<AudioSource>().volume = 0.5f - fastSource.volume / 2;
        }
        if (timer > 0)
        {
            slidePosition = Mathf.Lerp(slidePosition, 0, Time.deltaTime * 4);
        }
        else {
            slidePosition = Mathf.Lerp(slidePosition, slidePositionMax, Time.deltaTime * 2);
        }
    }

    private void OnGUI()
    {
        if (slidePosition < slidePositionMax - 0.1f)
        {
            GUI.skin = skin;
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 16;
            style.alignment = TextAnchor.MiddleRight;
            Rect rect = new Rect(0, Screen.height - 30 + slidePosition, Screen.width, 30);

            //GUIX.ShadowLabel(rect,currentSongCredits,style,1);
            GUILayout.BeginArea(rect);
            GUILayout.FlexibleSpace(); //Push down
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); //Push to the right
            GUILayout.Label(currentSongCredits, GUI.skin.GetStyle("SoundCredits"), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }

    private void ShuffleSongs()
    {
        //Shuffle playlist using Fisher-Yates algorithm
        for (int i = playlist.Length; i > 1; i--)
        {
            int j = Random.Range(0, i);
            Song tmp = playlist[j];
            playlist[j] = playlist[i - 1];
            playlist[i - 1] = tmp;
        }
    }
}

[System.Serializable]
public class Song
{
    public string name;
    public AudioClip clip;
}
