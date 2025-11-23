using Cysharp.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public enum HitResult : int { None = -2, Miss, Maximum, Perfect, Great, Good, Bad }

public class Judgement : Singleton<Judgement>
{
    private enum HitType : byte { Head, Tail }

    public static class HitRange
    {
        public static double OD = 0;
        public static double Maximum { get; private set; }
        public static double Perfect { get; private set; }
        public static double Great { get; private set; }
        public static double Good { get; private set; }
        public static double Bad { get; private set; }
        public static double Miss { get; private set; }

        public static void Initialize( double _OD )
        {
            OD = _OD;
            Maximum = OD <= 5d ? 22.4d - ( 0.6d * OD ) : // 16.5d
                                 24.9d - ( 1.1d * OD );

            Perfect = 64.5d  - ( 3 * OD );
            Great   = 97.5d  - ( 3 * OD );
            Good    = 127.5d - ( 3 * OD );
            Bad     = 151.5d - ( 3 * OD );
            Miss    = 188.5d - ( 3 * OD );
        }
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
                double current  = ( HitScore.Perfect * ( Maximum + Perfect ) ) + ( HitScore.Great * Great ) + ( HitScore.Good * Good ) + ( HitScore.Bad * Bad );
                double maxValue = HitScore.Perfect * Count;
                return Global.Math.Abs( maxValue ) <= double.Epsilon ? 0d : ( current / maxValue ) * 100d;
            }
        }
        public int Count => Maximum + Perfect + Great + Good + Bad + Miss;
    }

    public static readonly double TailMultiply = 1.5d;
    private static ResultData Results;
    private static double     MaxScore;
    private static int        Bonus;

    public  static List<HitData>            HitDatas  = new ();
    private static ConcurrentQueue<HitData> DataQueue = new ();
    private CancellationTokenSource  dataCts;

    public static event Action<HitData> OnHitNote;

    protected override void Awake()
    {
        base.Awake();
        NowPlaying.OnInitialize += Initialize;
        NowPlaying.OnRelease += Release;
        NowPlaying.OnClear += Clear;
    }

    private void Initialize()
    {
        MaxScore = 500000d / NowPlaying.TotalJudge;
        HitRange.Initialize( 0 );

        DataProcess().Forget();
    }

    private void Clear()
    {
        Bonus = 100; // 게임시작시 최대 보너스로 시작
        Results = new ResultData();

        DataQueue.Clear();
        HitDatas.Clear();
    }

    private void Release()
    {
        dataCts?.Cancel();
        dataCts?.Dispose();
        dataCts = null;
    }

    private async UniTask DataProcess()
    {
        dataCts?.Cancel();
        dataCts?.Dispose();
        dataCts = new CancellationTokenSource();
        CancellationToken token = dataCts.Token;
        try
        {
            while ( !token.IsCancellationRequested )
            {
                while ( DataQueue.TryDequeue( out HitData hitData ) )
                {
                    HitDatas.Add( hitData );
                    OnHitNote?.Invoke( hitData );
                }

                await UniTask.Yield( PlayerLoopTiming.Update );
            }

        }
        catch ( OperationCanceledException )
        {
            Debug.Log( "InputSystem HitData Process Cancel" );
        }
    }

    public static ResultData CurrentResult => Results;
    public static bool CanBeHit( double _diff, bool _isTail = false )
    {
        double multiply = _isTail ? TailMultiply : 1d;
        return -HitRange.Bad * multiply <= _diff && _diff <= HitRange.Bad * multiply;
    }
    public static bool IsEarlyMiss( double _diff, bool _isTail = false )
    {
        double multiply = _isTail ? TailMultiply : 1d;
        return HitRange.Miss * multiply >= _diff && _diff > HitRange.Bad * multiply;
    }
    public static bool IsLateMiss( double _diff, bool _isTail = false )
    {
        double multiply = _isTail ? TailMultiply : 1d;
        return -HitRange.Bad * multiply > _diff;
    }

    public static HitResult UpdateResult( int _lane, double _playback, double _diff, bool _isSlider, KeyState _keyState )
    {
        bool   isTail     = _isSlider && _keyState == KeyState.Up;
        bool   isHeadMiss = _isSlider && _keyState == KeyState.Down;
        double diffAbs    = Math.Abs( _diff );

        // 판정 카운팅
        HitResult hitResult = GetHitResult( _diff, diffAbs, isTail );
        if ( hitResult == HitResult.None )
            Debug.LogError( "??SFDfasfdaf? " );
        switch ( hitResult ) 
        {
            case HitResult.Maximum: Results.Maximum += 1;                  break;
            case HitResult.Perfect: Results.Perfect += 1;                  break;
            case HitResult.Great:   Results.Great   += 1;                  break;
            case HitResult.Good:    Results.Good    += 1;                  break;
            case HitResult.Bad:     Results.Bad     += 1;                  break;
            case HitResult.None:                                           
            case HitResult.Miss:    Results.Miss    += isHeadMiss ? 2 : 1; break;
        }

        Results.Combo    = hitResult == HitResult.Miss ? 0 : Results.Combo += 1;
        Results.MaxCombo = Results.Combo > Results.MaxCombo ? Results.Combo : Results.MaxCombo;

        // Great ~ Bad까지만 집계
        double multiply = isTail ? TailMultiply : 1d;
        if ( HitRange.Perfect * multiply < diffAbs && diffAbs <= HitRange.Bad * multiply )
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
        
        DataQueue.Enqueue( new HitData( _lane, _playback, _diff, isTail, hitResult, _keyState ) );
        return hitResult;
    }

    private static HitResult GetHitResult( double _diff, double _diffAbs, bool _isTail = false )
    {
        double multiply = _isTail ? TailMultiply : 1d;
        if ( HitRange.Miss * multiply >= _diff && _diff > HitRange.Bad * multiply )
        {
            Debug.LogWarning( "EarlyMiss" );
            return HitResult.Miss; // Early Miss
        }
        if ( _isTail && _diff > HitRange.Bad * multiply )
        {
            Debug.LogWarning( "LN EarlyMiss" );
            return HitResult.Bad;
        }
        if ( _diffAbs <= HitRange.Maximum * multiply ) return HitResult.Maximum; // ----------------------
        if ( _diffAbs <= HitRange.Perfect * multiply ) return HitResult.Perfect; //
        if ( _diffAbs <= HitRange.Great   * multiply ) return HitResult.Great;   //        정상 판정
        if ( _diffAbs <= HitRange.Good    * multiply ) return HitResult.Good;    //
        if ( _diffAbs <= HitRange.Bad     * multiply ) return HitResult.Bad;     // ---------------------- // 늦은 Bad는 Hit 불가
        if ( _diff    < -HitRange.Bad     * multiply ) return HitResult.Miss;    // Late Miss
        //if ( _diffAbs <= HitRange.Miss    ) return HitResult.Miss;    // 이르거나 늦은 판정 ( 롱노트 처리 중 Up을 빨리한 경우 )
        return HitResult.None; // _isTail ? HitResult.Bad : HitResult.Miss;                                        // -Bad ~ : 노트를 처리하지 못함
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