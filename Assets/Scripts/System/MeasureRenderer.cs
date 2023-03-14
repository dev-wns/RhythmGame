using UnityEngine;

public class MeasureRenderer : MonoBehaviour//, IObjectPool<MeasureRenderer>
{
    public double ScaledTime { get; private set; }

    private void Awake()
    {
        transform.localScale = new Vector3( GameSetting.GearWidth, GameSetting.MeasureHeight, 1f );
    }

    public void UpdateTransform( double _playback, double _scaledPlayback )
    {
        transform.position = new Vector2( GameSetting.GearOffsetX, GameSetting.JudgePos + ( float )( ( ScaledTime - _scaledPlayback ) * GameSetting.Weight ) );
    }

    public void SetInfo( double _scaledTime )
    {
        ScaledTime = _scaledTime;
    }
}
