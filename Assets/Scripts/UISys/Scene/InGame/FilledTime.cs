using System.Collections;
using System.Collections.Generic;
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
        length = NowPlaying.Inst.CurrentSong.totalTime * .001d / GameSetting.CurrentPitch;
        StartCoroutine( Process() );
    }

    private IEnumerator Process()
    {
        while ( NowPlaying.Playback < length )
        {
            image.fillAmount = ( float )( NowPlaying.Playback / length );
            yield return null;
        }
    }
}
