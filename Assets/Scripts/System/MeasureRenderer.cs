using UnityEngine;

public class MeasureRenderer : MonoBehaviour, IObjectPool<MeasureRenderer>
{
    public ObjectPool<MeasureRenderer> pool { get; set; }
    private double distance;

    private void Awake()
    {
        transform.localScale = new Vector3( GameSetting.GearWidth, GameSetting.MeasureHeight, 1f );
    }

    private void LateUpdate()
    {
        transform.position = new Vector2( GameSetting.GearOffsetX, GameSetting.JudgePos + ( float )( distance - NowPlaying.Distance ) * GameSetting.Weight );
        if ( distance < NowPlaying.Distance )
             pool.Despawn( this );
    }

    public void SetInfo( double _distance )
    {
        distance = _distance;
    }
}
