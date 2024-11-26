using UnityEngine;

public class MeasureRenderer : MonoBehaviour, IObjectPool<MeasureRenderer>
{
    public ObjectPool<MeasureRenderer> pool { get; set; }
    public double distance { get; private set; }

    private void Awake()
    {
        transform.localScale = new Vector3( GameSetting.GearWidth, GameSetting.MeasureHeight, 1f );
    }

    private void OnDestroy()
    {
        //NowPlaying.OnUpdateDistance -= UpdatePosition;
    }

    //private void LateUpdate()
    //{
    //    transform.position = new Vector2( GameSetting.GearOffsetX, GameSetting.JudgePos + ( float )( distance - NowPlaying.Distance ) * GameSetting.Weight );
    //    if ( distance < NowPlaying.Distance )
    //         pool.Despawn( this );
    //}

    private void LateUpdate()
    {
        transform.position = new Vector2( GameSetting.GearOffsetX, GameSetting.JudgePos + ( float )( distance - NowPlaying.Distance ) * GameSetting.Weight );
        if ( distance < NowPlaying.Distance )
        {
            //NowPlaying.OnUpdateDistance -= UpdatePosition;
            pool.Despawn( this );
        }
    }

    public void SetInfo( double _scaledTime )
    {
        distance = _scaledTime;
        //NowPlaying.OnUpdateDistance += UpdatePosition;
    }
}
