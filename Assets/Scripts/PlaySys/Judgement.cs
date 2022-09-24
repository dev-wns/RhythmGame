using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum HitResult { None, Perfect, Great, Good, Bad, Miss, Fast, Slow, Rate, Combo, Score, Count }
public struct HitData
{
    public double time;
    public double diff;

    public HitData( double _time, double _diff )
    {
        time = _time;
        diff = _diff;
    }
}

public class Judgement : MonoBehaviour
{
    public const double Perfect      = .042d;           // .0421d;
    public const double Great        = .022d + Perfect; //.064d;
    public const double Good         = .015d + Great;   //.097d;
    public const double Bad          = .013d + Good;    //.127d;

    public event Action<HitResult> OnJudge;
    public List<HitData> hitDatas = new List<HitData>();
    private Dictionary<HitResult, int /* count */> results = new Dictionary<HitResult, int>();

    private void Awake()
    {
        DontDestroyOnLoad( gameObject );
    }

    public int GetResult( HitResult _type )
    {
        if ( !results.ContainsKey( _type ) )
        {
            Debug.LogError( $"type is not found key {_type}" );
            return -1;
        }

        return results[_type];
    }

    public void SetResult( HitResult _type, int _count )
    {
        if ( results.ContainsKey( _type ) )
            results[_type] = _count;
        else
            results.Add( _type, _count );
    }

    public bool CanBeHit( double _diff ) => Globals.Abs( _diff ) <= Bad ? true : false;
    
    public bool IsMiss( double _diff ) =>_diff < -Bad ? true : false;

    public void ResultUpdate( double _diff )
    {
        double diffAbs = Globals.Abs( _diff );

        if      ( diffAbs <= Perfect                    ) OnJudge?.Invoke( HitResult.Perfect );
        else if ( diffAbs > Perfect && diffAbs <= Great ) OnJudge?.Invoke( HitResult.Great   );
        else if ( diffAbs > Great   && diffAbs <= Good  ) OnJudge?.Invoke( HitResult.Good    );
        else if ( diffAbs > Good    && diffAbs <= Bad   ) OnJudge?.Invoke( HitResult.Bad     );
        else if ( _diff   < -Bad                        ) OnJudge?.Invoke( HitResult.Miss    );
        else                                              OnJudge?.Invoke( HitResult.None    );

        hitDatas.Add( new HitData( NowPlaying.Playback, _diff ) );
        if ( diffAbs > Perfect && diffAbs <= Bad )
        {
            if ( _diff > 0d ) OnJudge?.Invoke( HitResult.Fast );
            else              OnJudge?.Invoke( HitResult.Slow );
        }
    }

    public void ResultUpdate( HitResult _type )
    {
        OnJudge?.Invoke( _type );

        if ( _type == HitResult.Miss )
             hitDatas.Add( new HitData( NowPlaying.Playback, .1f ) );
    }
}
