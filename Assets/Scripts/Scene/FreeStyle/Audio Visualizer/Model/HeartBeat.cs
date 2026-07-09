using UnityEngine;

public class HeartBeat : MonoBehaviour
{
    [SerializeField] private FreeStyle scene;
    [SerializeField] private SoundPitchOption pitchOption;
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
        scene.OnStart   += UpdateSong;
        scene.OnRestart += UpdateSong;
        pitchOption.OnUpdatePitch += UpdatePitch;

        startSize = transform.localScale.x;
        endSize   = startSize * power;
    }

    private void OnDestroy()
    {
        scene.OnStart   -= UpdateSong;
        scene.OnRestart -= UpdateSong;
        pitchOption.OnUpdatePitch -= UpdatePitch;
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

    private void Initialize( double _bpm )
    {
        spb  = ( float )( 60d / _bpm );
        time = ( float )( NowPlaying.Playback * GameSetting.SoundPitch * .001d ) % spb;
        transform.localScale = new Vector2( startSize, startSize );
    }

    private void UpdateSong( in Song _song )
    {
        isStop = false;
        curBPM = _song.mainBPM;
        Initialize( curBPM * GameSetting.SoundPitch );
    }

    private void UpdatePitch( float _pitch )
    {
        Initialize( curBPM * _pitch );
    }
}
