using System;

public enum HitResult : int { None = -2, Miss, Maximum, Perfect, Great, Good, Bad }

public class Judgement : Singleton<Judgement>
{
    private static class HitRange
    {
        public static readonly int Maximum = 16;
        public static readonly int Perfect = 64;
        public static readonly int Great   = 97;
        public static readonly int Good    = 127;
        public static readonly int Bad     = 151;
        public static readonly int Miss    = 188;
    }
    private static class HitScore
    {
        public static readonly int Maximum = 320;
        public static readonly int Perfect = 300;
        public static readonly int Great   = 200;
        public static readonly int Good    = 100;
        public static readonly int Bad     =  50;
        public static readonly int Miss    =   0;
    }
    private static class HitBonus
    {
        public static readonly int Maximum = 2;
        public static readonly int Perfect = 1;
        public static readonly int Great   = -8;
        public static readonly int Good    = -24;
        public static readonly int Bad     = -44;
        public static readonly int Miss    = -100;
    }
    public struct ResultData
    {
        public int Maximum;
        public int Perfect;
        public int Great;
        public int Good;
        public int Bad;
        public int Miss;
        public int Fast;
        public int Slow;
        public int Combo;
        public int MaxCombo;
        public double Score;

        public double Accuracy
        {
            get
            {
                double total = ( HitScore.Perfect * ( Maximum + Perfect ) ) + ( HitScore.Great * Great ) + ( HitScore.Good * Good ) + ( HitScore.Bad * Bad );
                double max   = ( HitScore.Perfect * .01d ) * ( Maximum + Perfect + Great + Good + Bad + Miss );
                return Global.Math.Abs( max ) <= double.Epsilon ? 0d : total / max;
            }
        }
        public int Count => Maximum + Perfect + Great + Good + Bad + Miss;
    }

    private static ResultData Results;
    private static double     MaxScore;
    private static int        Bonus;

    protected override void Awake()
    {
        base.Awake();
        NowPlaying.OnPreInit += Initialize;
        NowPlaying.OnClear   += Clear;
    }

    private void Initialize()
    {
        MaxScore = 500000d / NowPlaying.TotalJudge;
    }

    private void Clear()
    {
        Bonus   = 100; // 게임시작시 100으로 시작
        Results = new ResultData();
    }

    public static ResultData CurrentResult => Results;
    public static bool CanBeHit( double _diff ) => Math.Abs( _diff ) <= HitRange.Bad;
    public static bool IsMiss( double _diff )   => _diff < -HitRange.Bad;
    public static HitResult UpdateResult( double _diff, bool _isDoubleMiss = false )
    {
        double diffAbs      = Math.Abs( _diff );
        HitResult hitResult = GetHitResult( diffAbs );

        switch ( hitResult ) 
        {
            case HitResult.Maximum: Results.Maximum += 1;                     break;
            case HitResult.Perfect: Results.Perfect += 1;                     break;
            case HitResult.Great:   Results.Great   += 1;                     break;
            case HitResult.Good:    Results.Good    += 1;                     break;
            case HitResult.Bad:     Results.Bad     += 1;                     break;
            case HitResult.Miss:    Results.Miss    += _isDoubleMiss ? 2 : 1; break;
        }

        Results.Combo    = hitResult == HitResult.Miss ? 0 : Results.Combo += 1;
        Results.MaxCombo = Results.Combo > Results.MaxCombo ? Results.Combo : Results.MaxCombo;

        // Maximum과 Miss 판정은 제외한다.
        if ( diffAbs > HitRange.Perfect && diffAbs <= HitRange.Bad )
        {
            Results.Fast += _diff > 0d ? 1 : 0;
            Results.Slow += _diff < 0d ? 1 : 0;
        }

        // 스코어
        int hitScore = GetHitScore( hitResult ); // 320, 300, 200, 100, 50, 0
        int hitBonus = GetHitBonus( hitResult ); // 2, 1, -8, -16, -44, -100
        Bonus = Math.Clamp( Bonus + hitBonus, 0, 100 ); // 판정에 따라 증감
        double bonusScore = Math.Sqrt( Bonus ) * Math.Clamp( 64 >> Convert.ToInt32( hitResult ), 0, HitScore.Maximum / 10 );

        // 기본 500,000, 보너스 500,000 => 최대 : ( 50만 / 전체판정수 ) * ( 320 + 320 ) / 320;
        Results.Score += MaxScore * ( hitScore + bonusScore ) / HitScore.Maximum;

        if ( hitResult == HitResult.None )
             Debug.LogError( $"{hitResult} {diffAbs} " );

        return hitResult;
    }

    private static HitResult GetHitResult( double _diffAbs )
    {
        return                                _diffAbs <= HitRange.Maximum ? HitResult.Maximum :
               _diffAbs > HitRange.Maximum && _diffAbs <= HitRange.Perfect ? HitResult.Perfect :
               _diffAbs > HitRange.Perfect && _diffAbs <= HitRange.Great   ? HitResult.Great   :
               _diffAbs > HitRange.Great   && _diffAbs <= HitRange.Good    ? HitResult.Good    :
               _diffAbs > HitRange.Good    && _diffAbs <= HitRange.Bad     ? HitResult.Bad     :
               _diffAbs > HitRange.Bad     && _diffAbs <= HitRange.Miss    ? HitResult.Miss    :
                                                                             HitResult.Miss; // 롱노트 Up을 빨리 했을 경우
    }

    private static int GetHitScore( HitResult _result )
    {
        return _result switch
        { 
            HitResult.Maximum => HitScore.Maximum,
            HitResult.Perfect => HitScore.Perfect,
            HitResult.Great   => HitScore.Great,
            HitResult.Good    => HitScore.Good,
            HitResult.Bad     => HitScore.Bad,
            HitResult.Miss    => HitScore.Miss,
            _                 => HitScore.Miss
        };
    }
    private static int GetHitBonus( HitResult _result )
    {
        return _result switch
        { 
            HitResult.Maximum => HitBonus.Maximum,
            HitResult.Perfect => HitBonus.Perfect,
            HitResult.Great   => HitBonus.Great,
            HitResult.Good    => HitBonus.Good,
            HitResult.Bad     => HitBonus.Bad,
            HitResult.Miss    => HitBonus.Miss,
            _                 => HitBonus.Miss
        };
    }
}