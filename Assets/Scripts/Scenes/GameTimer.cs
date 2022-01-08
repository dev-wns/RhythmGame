using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Playback
{
    public float playback;
    public float playbackChanged;
}

public class GameTimer : MonoBehaviour
{
    private InGame scene;
    private List<Timing> timings;
    private Playback time;

    public delegate void DelPlaybackChanged( Playback _time );
    public event DelPlaybackChanged OnPlaybackUpdate;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        //scene.OnSystemInitialized += Initialized;
        //scene.OnGameStart += () => StartCoroutine( WaitTimeUpdate() );
    }

    private void Initialized( Chart _chart )
    {
        timings = _chart.timings;

        SoundManager.Inst.LoadBgm( GameManager.Inst.CurrentSong.audioPath );
        SoundManager.Inst.PlayBgm( true );
    }

    private IEnumerator WaitTimeUpdate()
    {
        time.playback = -5000;
        while ( true )
        {
            time.playback += Time.deltaTime * 1000f;
            time.playbackChanged = GetChangedTime( time.playback );
            OnPlaybackUpdate( time );

            if ( time.playback >= 0 )
            {
                time.playback = 0;
                SoundManager.Inst.PauseBgm( false );
                StartCoroutine( GameTimeUpdate() );
                break;
            }
            yield return null;
        }
    }

    private IEnumerator GameTimeUpdate()
    {
        while ( true )
        {
            time.playback += Time.deltaTime * 1000f;
            time.playbackChanged = GetChangedTime( time.playback );
            OnPlaybackUpdate( time );

            yield return null;
        }
    }

    private float GetChangedTime( float _time ) // BPM 변화에 따른 시간 계산
    {
        double newTime = _time;
        double prevBpm = 0d;
        for ( int i = 0; i < timings.Count; i++ )
        {
            double time = timings[i].time;
            double bpm  = timings[i].bpm;

            if ( time > _time ) break;
            newTime += ( bpm - prevBpm ) * ( _time - time );
            prevBpm = bpm;
        }
        return ( float )newTime;
    }
}
