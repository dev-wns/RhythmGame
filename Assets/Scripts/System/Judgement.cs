using System;
using UnityEngine;

public enum HitResult { None, Maximum, Perfect, Great, Good, Bad, Miss, Fast, Slow, Accuracy, Combo, Score, Count }

public class Judgement : MonoBehaviour
{
    private InGame scene;
    public int TotalJudge { get; private set; }
    private int curJudge;

    public struct JudgeData
    {
        public double maximum;
        public double perfect;
        public double great;
        public double good;
        public double bad;
        public double miss;

        public JudgeData Multiply( float _value )
        {
            var newData = new JudgeData();
            newData.maximum = this.maximum * _value;
            newData.perfect = this.perfect * _value;
            newData.great   = this.great   * _value;
            newData.good    = this.good    * _value;
            newData.bad     = this.bad     * _value;
            newData.miss    = this.miss    * _value;
            return newData;
        }
    }
    public static readonly JudgeData OriginJudgeData = new JudgeData() 
    { 
        maximum = .0165d, 
        perfect = .039d, 
        great   = .061d, 
        good    = .076d, 
        bad     = .089d, 
        miss    = .1d 
    };
    public static JudgeData NoteJudgeData;
    public static JudgeData SliderJudgeData;
    public event Action<HitResult, NoteType> OnJudge;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();

        scene.OnReLoad += () => curJudge = 0;

        bool hasHardJudge = GameSetting.CurrentGameMode.HasFlag( GameMode.HardJudge );
        NoteJudgeData   = hasHardJudge ? OriginJudgeData.Multiply( .75f ) : OriginJudgeData;
        SliderJudgeData = NoteJudgeData.Multiply( 2f );

        var song = NowPlaying.CurrentSong;
        bool hasNoSlider      = GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider );
        bool hasKeyConversion = GameSetting.CurrentGameMode.HasFlag( GameMode.KeyConversion ) && song.keyCount == 7;

        var slider = hasKeyConversion ? song.sliderCount - song.delSliderCount : song.sliderCount;
        var note   = hasKeyConversion ? song.noteCount   - song.delNoteCount   : song.noteCount;
        TotalJudge = note + ( slider * 2 );
    }

    public bool CanBeHit( double _diff, NoteType _noteType )
    {
        return _noteType == NoteType.Default ? Global.Math.Abs( _diff ) <= NoteJudgeData.bad : Global.Math.Abs( _diff ) <= SliderJudgeData.bad;
    }

    public bool IsMiss( double _diff, NoteType _noteType )
    {
        return _noteType == NoteType.Default ? _diff < -NoteJudgeData.bad : _diff < -SliderJudgeData.bad;
    }

    public void ResultUpdate( double _diff, NoteType _noteType )
    {
        var Judge = _noteType == NoteType.Default ? NoteJudgeData : SliderJudgeData;
        double diffAbs = Math.Abs( _diff );
        HitResult result = diffAbs <= Judge.maximum                             ? HitResult.Maximum :
                           diffAbs >  Judge.maximum && diffAbs <= Judge.perfect ? HitResult.Perfect :
                           diffAbs >  Judge.perfect && diffAbs <= Judge.great   ? HitResult.Great   :
                           diffAbs >  Judge.great   && diffAbs <= Judge.good    ? HitResult.Good    :
                           diffAbs >  Judge.good    && diffAbs <= Judge.bad     ? HitResult.Bad     :
                                                                                  HitResult.None;

        if ( diffAbs > Judge.perfect && diffAbs <= Judge.bad )
        {
            HitResult fsType = _diff >= 0d ? HitResult.Fast : HitResult.Slow;
            OnJudge?.Invoke( fsType, _noteType );
            NowPlaying.Inst.IncreaseResult( fsType );
        }

        OnJudge?.Invoke( result, _noteType );
        NowPlaying.Inst.IncreaseResult( result );
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
            OnJudge?.Invoke( _result, _type );

        NowPlaying.Inst.IncreaseResult( _result, _count );

        if ( _result != HitResult.None && ( curJudge += _count ) >= TotalJudge )
        {
            StartCoroutine( scene.GameEnd() );
            Debug.Log( $"All lanes are empty ( {curJudge} judgement )" );
        }
    }
}