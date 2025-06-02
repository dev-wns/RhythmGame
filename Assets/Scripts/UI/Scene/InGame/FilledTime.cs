using UnityEngine;
using UnityEngine.UI;

public class FilledTime : MonoBehaviour
{
    public Image image;

    private InGame game;
    private double length;

    private void Awake()
    {
        game = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        game.OnGameStart += Initialize;
    }

    private void Initialize()
    {
        length = NowPlaying.CurrentSong.totalTime / GameSetting.CurrentPitch;
    }

    private void Update()
    {
        image.fillAmount = ( float )( NowPlaying.Playback / length );
    }
}
