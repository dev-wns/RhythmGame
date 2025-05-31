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
    // 판정 범위 ( ms )
    public static double Maximum => .016d * Multiply;
    public static double Perfect => .064d * Multiply;
    public static double Great   => .097d * Multiply;
    public static double Good    => .127d * Multiply;
    public static double Bad     => .151d * Multiply;
    public static double Miss    => .188d * Multiply;
    private static  double Multiply;

    public event Action<JudgeResult> OnJudge;

    private void Awake()
    {
        Multiply = GameSetting.CurrentGameMode.HasFlag( GameMode.HardJudge ) ? .75d : 1d;

        //var song = NowPlaying.CurrentSong;
        //bool hasNoSlider      = GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider );
        //bool hasKeyConversion = GameSetting.CurrentGameMode.HasFlag( GameMode.KeyConversion ) && song.keyCount == 7;

        //var note   = hasKeyConversion ? song.noteCount   - song.delNoteCount   : song.noteCount;
        //var slider = hasKeyConversion ? song.sliderCount - song.delSliderCount : song.sliderCount;
        //TotalNotes = note + ( slider * 2 );
    }

    public static bool CanBeHit( double _diff )
    {
        return Global.Math.Abs( _diff ) <= Bad;
    }

    public static bool IsMiss( double _diff )
    {
        return _diff < -Bad;
    }

    public void ResultUpdate( double _diff, NoteType _noteType )
    {
        JudgeResult result;
        result.noteType = _noteType;
        result.diff = _diff;

        double diffAbs = result.diffAbs = Math.Abs( _diff );
        result.hitResult = diffAbs <= Maximum ? HitResult.Maximum :
                           diffAbs > Maximum && diffAbs <= Perfect ? HitResult.Perfect :
                           diffAbs > Perfect && diffAbs <= Great   ? HitResult.Great :
                           diffAbs > Great   && diffAbs <= Good    ? HitResult.Good :
                           diffAbs > Good    && diffAbs <= Bad     ? HitResult.Bad :
                                                                     HitResult.None;

        if ( diffAbs > Perfect && diffAbs <= Bad )
        {
            GameManager.Inst.UpdateResult( _diff >= 0d ? HitResult.Fast : HitResult.Slow );
        }

        GameManager.Inst.AddHitData( _noteType, _diff );
        GameManager.Inst.UpdateResult( result.hitResult );

        OnJudge?.Invoke( result );
    }

    public void ResultUpdate( HitResult _result, NoteType _type, int _count = 1 )
    {
        for ( int i = 0; i < _count; i++ )
            OnJudge?.Invoke( new JudgeResult( _result, _type ) );

        GameManager.Inst.UpdateResult( _result, _count );
    }
}