using UnityEngine;

public class MeasureRenderer : MonoBehaviour, IObjectPool<MeasureRenderer>
{
    public ObjectPool<MeasureRenderer> pool { get; set; }
    public double ScaledTime { get; private set; }

    private void Awake()
    {
        transform.localScale = new Vector3( GameSetting.GearWidth, GameSetting.MeasureHeight, 1f );
    }

    private void LateUpdate()
    {
        transform.position = new Vector2( GameSetting.GearOffsetX, GameSetting.JudgePos + ( float )( ScaledTime - NowPlaying.ScaledPlayback ) * GameSetting.Weight );
        if ( ScaledTime <= NowPlaying.ScaledPlayback )
             pool.Despawn( this );
    }

    public void SetInfo( double _scaledTime )
    {
        ScaledTime = _scaledTime;
    }
}
