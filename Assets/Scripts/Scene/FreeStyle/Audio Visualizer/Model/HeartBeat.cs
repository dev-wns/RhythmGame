using UnityEngine;

public class HeartBeat : MonoBehaviour
{
    public FreeStyleMainScroll mainScroll;
    public SoundPitchOption pitchOption;
    private float startSize, endSize;

    [Header( "BPM" )]
    private double curBPM;
    private float spb;

    public  float duration = .1f;
    public  float power    = 1.25f;
    private float time     = 0f;
    private bool  isStop   = true;

    private void Awake()
    {
        mainScroll.OnSelectSong        += UpdateSong;
        mainScroll.OnSoundRestart      += UpdateSong;
        AudioManager.OnUpdatePitch     += UpdatePitch;

        startSize = transform.localScale.x;
        endSize = startSize * power;
    }

    private void Update()
    {
        if ( isStop ) return;

        time += Time.deltaTime;
        if ( spb < time )
             time %= spb;

        float cos  = Mathf.Cos( ( Global.Math.Clamp( time, 0f, duration ) / duration ) * Mathf.PI );
        float t    = ( 1f + cos ) * .5f; // 1 -> 0
        float size = Global.Math.Clamp( Global.Math.Lerp( startSize, endSize, t ), startSize, endSize );
        transform.localScale = new Vector2( size, size );
    }

    private void OnDestroy()
    {
        AudioManager.OnUpdatePitch -= UpdatePitch;
    }

    private void Initialize( double _bpm )
    {
        spb  = ( float )( 60d / _bpm );
        time = ( float )( FreeStyleMainScroll.Playback * GameSetting.CurrentPitch * .001d ) % spb;
        transform.localScale = new Vector2( startSize, startSize );
    }

    private void UpdateSong( Song _song )
    {
        isStop = false;
        curBPM = _song.mainBPM;
        Initialize( curBPM * GameSetting.CurrentPitch );
    }

    private void UpdatePitch( float _pitch )
    {
        Initialize( curBPM * _pitch );
    }
}
