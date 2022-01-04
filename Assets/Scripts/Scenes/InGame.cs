using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InGame : Scene
{
    // ui
    public TextMeshProUGUI timeText, bpmText, comboText, frameText, medianText;

    public delegate void DelSystemInitialized( Chart _chart );
    public event DelSystemInitialized SystemInitialized;

    private bool isStart = false;

    public delegate void DelStartGame();
    public event DelStartGame StartGame;
    private Chart chart;

    float delta;

    // Bpm
    public static float BPM { get; private set; } // 현재 BPM

    public static float PreLoadTime { get { return ( 1250f / Weight ); } }
    // 60bpm은 분당 1/4박자 60개, 스크롤 속도가 1일때 한박자(1/4) 시간은 1초
    public static float Weight { get { return ( 60f / GameManager.Inst.MedianBpm ) * GlobalSetting.ScrollSpeed; } }

    // time ( millisecond )
    public static float Playback { get; private set; } // 노래 재생 시간
    public static float PlaybackChanged { get; private set; } // BPM 변화에 따른 노래 재생 시간

    public static float GetChangedTime( float _time, Chart chart ) // BPM 변화에 따른 시간 계산
    {
        double newTime = _time;
        double prevBpm = 0d;
        for ( int i = 0; i < chart.timings.Count; i++ )
        {
            double time = chart.timings[i].time;
            double bpm = chart.timings[i].bpm;

            if ( time > _time ) break;
            newTime += ( bpm - prevBpm ) * ( _time - time );
            prevBpm = bpm;
        }
        return ( float )newTime;
    }

    protected override void Awake()
    {
        base.Awake();

        Playback = 0f;

        using ( FileParser parser = new FileParser() )
        {
            parser.TryParse( GameManager.Inst.CurrentSound.filePath, out chart );
        }

        SystemInitialized( chart );

        ChangeAction( SceneAction.InGame );
        
        SoundManager.Inst.LoadBgm( GameManager.Inst.CurrentSound.audioPath );
        StartGame();
        SoundManager.Inst.PlayBgm( true );
        StartCoroutine( WaitBeginningTime() );
        StartCoroutine( BpmChnager() );
    }

    private IEnumerator WaitBeginningTime()
    {
        yield return YieldCache.WaitForSeconds( 3f );
        SoundManager.Inst.PauseBgm( false );
        isStart = true;
    }


    private int timingIdx;
    private IEnumerator BpmChnager()
    {
        while ( timingIdx < chart.timings.Count )
        {
            yield return new WaitUntil( () => Playback > chart.timings[timingIdx].time );
            bpmText.text = $"{Mathf.RoundToInt(chart.timings[timingIdx++].bpm)} BPM";
        }
    }

    protected override void Update()
    {
        base.Update();

        if ( !isStart ) return;
        
        Playback += Time.deltaTime * 1000f;
        PlaybackChanged = GetChangedTime( Playback, chart );

        timeText.text = string.Format( "{0:F1} 초", Playback * 0.001f );
        //comboText.text = string.Format( "{0}", GameManager.Combo );
        delta += ( Time.unscaledDeltaTime - delta ) * .1f;
        frameText.text = string.Format( "{0:F1}", 1f / delta );
        //medianText.text = string.Format( "{0:F1}", MedianBpm ); 
    }

    public override void KeyBind()
    {
        Bind( SceneAction.InGame, KeyCode.Escape, () => SceneChanger.Inst.LoadScene( SCENE_TYPE.FREESTYLE ) );
    }
}
