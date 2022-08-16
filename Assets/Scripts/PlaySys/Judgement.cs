using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum HitResult { None, Perfect, Great, Good, Bad, Miss, Fast, Slow, Rate, Combo, Score, Count }

public class Judgement : MonoBehaviour
{
    public const double Perfect      = .0287d; // .0421d;
    public const double Great        = .0264d + Perfect; //.064d;
    public const double Good         = .0229d + Great; //.097d;
    public const double Bad          = .0181d + Good; //.127d;

    public event Action<HitResult> OnJudge;

    private Dictionary<HitResult, int /* count */> results = new Dictionary<HitResult, int>();
    public TextMeshProUGUI hitAverageText;
    private double hitTotalValue;
    private double hitAverage;
    private Queue<double/*diff*/> hitValues = new Queue<double>();

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

    public bool CanBeHit( double _diff )
    {
        double diff = ( _diff >= 0 ) ? _diff - NowPlaying.PlaybackOffset
                                     : _diff + NowPlaying.PlaybackOffset;

        return Globals.Abs( diff ) <= Bad ? true : false;
    }
    
    public bool IsMiss( double _diff )
    {
        return _diff + NowPlaying.PlaybackOffset < -Bad ? true : false;
    }

    public void ResultUpdate( double _diff )
    {
        double diff = ( _diff >= 0 ) ? _diff - NowPlaying.PlaybackOffset
                                     : _diff + NowPlaying.PlaybackOffset;
        double diffAbs = Globals.Abs( diff );

        if      ( diffAbs <= Perfect                    ) OnJudge?.Invoke( HitResult.Perfect );
        else if ( diffAbs > Perfect && diffAbs <= Great ) OnJudge?.Invoke( HitResult.Great   );
        else if ( diffAbs > Great   && diffAbs <= Good  ) OnJudge?.Invoke( HitResult.Good    );
        else if ( diffAbs > Good    && diffAbs <= Bad   ) OnJudge?.Invoke( HitResult.Bad     );
        else if ( diff    < -Bad                        ) OnJudge?.Invoke( HitResult.Miss    );
        else                                              OnJudge?.Invoke( HitResult.None    );

        if ( diffAbs > Perfect && diffAbs <= Bad )
        {
            if ( diff > 0d ) OnJudge?.Invoke( HitResult.Fast );
            else             OnJudge?.Invoke( HitResult.Slow );
        }

        double hitms = diff * 1000d;
        if ( hitValues.Count <= 50 )
        {
            hitValues.Enqueue( hitms );
            hitTotalValue += hitms;
        }
        else
        {
            hitValues.Enqueue( hitms );
            hitTotalValue += hitms - hitValues.Dequeue();
        }

        hitAverage = hitTotalValue / hitValues.Count;
        hitAverageText.text = $"{( int )hitAverage} ms";
    }

    public void ResultUpdate( HitResult _type )
    {
        OnJudge?.Invoke( _type );
    }
}
