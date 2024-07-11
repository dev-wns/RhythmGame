using System;
using UnityEngine;

public enum HitResult { None, Maximum, Perfect, Great, Good, Bad, Miss, Fast, Slow, Accuracy, Combo, Score, Count }
public struct JudgeResult
{
    public HitResult hitResult;
    public NoteType  noteType;
    public double diff;
    public double diffAbs;

    public JudgeResult( HitResult _hitResult, NoteType _noteType )
    {
        hitResult = _hitResult;
        noteType = _noteType;
        diff = diffAbs = 0d;
    }
}

public class Judgement : MonoBehaviour
{
    private InGame scene;
    public int TotalJudge { get; private set; }
    private int curJudge;

    public static double Maximum => .0165d * Multiply;
    public static double Perfect => .0405d * Multiply; 
    public static double Great   => .0735d * Multiply; 
    public static double Good    => .1035d * Multiply; 
    public static double Bad     => .1275d * Multiply; 
    public static double Miss    => .1500d * Multiply;
    private static  double Multiply;

    public event Action<JudgeResult> OnJudge;


    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnReLoad += () => curJudge = 0;

        Multiply = GameSetting.CurrentGameMode.HasFlag( GameMode.HardJudge ) ? .75d : 1d;

        var song = NowPlaying.CurrentSong;
        bool hasNoSlider      = GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider );
        bool hasKeyConversion = GameSetting.CurrentGameMode.HasFlag( GameMode.KeyConversion ) && song.keyCount == 7;

        var slider = hasKeyConversion ? song.sliderCount - song.delSliderCount : song.sliderCount;
        var note   = hasKeyConversion ? song.noteCount   - song.delNoteCount   : song.noteCount;
        TotalJudge = note + ( slider * 2 );
    }

    public bool CanBeHit( double _diff, NoteType _noteType )
    {
        return Global.Math.Abs( _diff ) <= Bad;
    }

    public bool IsMiss( double _diff, NoteType _noteType )
    {
        return _diff < -Bad;
    }

    public void ResultUpdate( double _diff, NoteType _noteType )
    {
        JudgeResult result;
        result.noteType = _noteType;
        result.diff     = _diff;

        double diffAbs = result.diffAbs = Math.Abs( _diff );
        result.hitResult = diffAbs <= Maximum                       ? HitResult.Maximum :
                           diffAbs >  Maximum && diffAbs <= Perfect ? HitResult.Perfect :
                           diffAbs >  Perfect && diffAbs <= Great   ? HitResult.Great   :
                           diffAbs >  Great   && diffAbs <= Good    ? HitResult.Good    :
                           diffAbs >  Good    && diffAbs <= Bad     ? HitResult.Bad     :
                                                                      HitResult.None;

        if ( diffAbs > Perfect && diffAbs <= Bad )
        {
            NowPlaying.Inst.IncreaseResult( _diff >= 0d ? HitResult.Fast : HitResult.Slow );
        }

        OnJudge?.Invoke( result );
        NowPlaying.Inst.IncreaseResult( result.hitResult );
        NowPlaying.Inst.AddHitData( _noteType, _diff );

        if ( ++curJudge >= TotalJudge )
        {
            StartCoroutine( scene.GameEnd() );
            Debug.Log( $"All lanes are empty ( {curJudge} judgement )" );
        }
    }

    public void ResultUpdate( HitResult _result, NoteType _type, int _count = 1 )
    {
        for ( int i = 0; i < _count; i++ )
            OnJudge?.Invoke( new JudgeResult( _result, _type ) );

        NowPlaying.Inst.IncreaseResult( _result, _count );

        if ( _result != HitResult.None && ( curJudge += _count ) >= TotalJudge )
        {
            StartCoroutine( scene.GameEnd() );
            Debug.Log( $"All lanes are empty ( {curJudge} judgement )" );
        }
    }
}