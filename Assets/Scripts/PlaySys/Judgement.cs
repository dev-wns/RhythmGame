using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum HitResult { None, Perfect, Great, Good, Bad, Miss, Fast, Slow, Rate, Combo, Score, Count }
public struct HitData
{
    public HitResult result;
    public double time;
    public double diff;

    public HitData( HitResult _res, double _time, double _diff )
    {
        result = _res;
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
    public const double Miss         = .1f;

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

    public bool CanBeHit( double _diff ) => Global.Math.Abs( _diff ) <= Bad ? true : false;
    
    public bool IsMiss( double _diff ) =>_diff < -Bad ? true : false;

    public void ResultUpdate( double _diff )
    {
        double diff = _diff;
        double diffAbs = Global.Math.Abs( diff );
        HitResult result;

        if      ( diffAbs <= Perfect                    ) result = HitResult.Perfect;
        else if ( diffAbs > Perfect && diffAbs <= Great ) result = HitResult.Great;
        else if ( diffAbs > Great   && diffAbs <= Good  ) result = HitResult.Good;
        else if ( diffAbs > Good    && diffAbs <= Bad   ) result = HitResult.Bad ;
        else if ( diff    < -Bad                        ) result = HitResult.Miss;
        else                                              result = HitResult.None;

        if ( diffAbs > Perfect && diffAbs <= Bad )
        {
            if ( diff > 0d ) OnJudge?.Invoke( HitResult.Fast );
            else             OnJudge?.Invoke( HitResult.Slow );
        }

        OnJudge?.Invoke( result );
        hitDatas.Add( new HitData( result, NowPlaying.Playback, diff ) );
    }

    public void ResultUpdate( HitResult _type )
    {
        OnJudge?.Invoke( _type );

        if ( _type == HitResult.Miss )
             hitDatas.Add( new HitData( HitResult.Miss, NowPlaying.Playback, Miss ) );
    }
}
