using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum HitResult { None, Maximum, Perfect, Great, Good, Bad, Miss, Fast, Slow, Rate, Combo, Score, Count }
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
    public const double Maximum      = .0165d;           
    public const double Perfect      = .0225d + Maximum; 
    public const double Great        = .022d + Perfect;  
    public const double Good         = .015d + Great;    
    public const double Bad          = .013d + Good;     
    public const double Miss         = .1f;

    public event Action<HitResult, NoteType> OnJudge;
    public List<HitData> hitDatas = new List<HitData>();
    private Dictionary<HitResult, int /* count */> results = new Dictionary<HitResult, int>();

    private void Awake()
    {
        DontDestroyOnLoad( gameObject );
    }

    public void ReLoad()
    {
        hitDatas.Clear();
        results.Clear();
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

    public bool CanBeHit( double _diff ) => Global.Math.Abs( _diff ) <= Bad;
    
    public bool IsMiss( double _diff ) =>_diff < -Bad;

    public void ResultUpdate( double _diff, NoteType _type )
    {
        double diff = _diff;
        double diffAbs = Math.Abs( diff );
        HitResult result;

        if ( diffAbs <= Maximum )                           result = HitResult.Maximum;
        else if ( diffAbs > Maximum && diffAbs <= Perfect ) result = HitResult.Perfect;
        else if ( diffAbs > Perfect && diffAbs <= Great   ) result = HitResult.Great;
        else if ( diffAbs > Great   && diffAbs <= Good    ) result = HitResult.Good;
        else if ( diffAbs > Good    && diffAbs <= Bad     ) result = HitResult.Bad;
        else if ( diff    < -Bad                          ) result = HitResult.Miss;
        else                                                result = HitResult.None;
        
        if ( diffAbs > Perfect && diffAbs <= Bad )
        {
            if ( diff > 0d ) OnJudge?.Invoke( HitResult.Fast, _type );
            else             OnJudge?.Invoke( HitResult.Slow, _type );
        }

        OnJudge?.Invoke( result, _type );

        hitDatas.Add( new HitData( result, NowPlaying.Playback, diff ) );
    }

    public void ResultUpdate( HitResult _result, NoteType _type )
    {
        OnJudge?.Invoke( _result, _type );

        if ( _result == HitResult.Miss )
             hitDatas.Add( new HitData( HitResult.Miss, NowPlaying.Playback, Miss ) );
    }
}
