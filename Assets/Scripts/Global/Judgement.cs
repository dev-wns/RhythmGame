using Cysharp.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public enum HitResult : int { None = -2, Miss, Maximum, Perfect, Great, Good, Bad }

public class Judgement : Singleton<Judgement>
{
    public static class HitRange
    {
        public static double OD = 0;
        public static double Maximum { get; private set; }
        public static double Perfect { get; private set; }
        public static double Great   { get; private set; }
        public static double Good    { get; private set; }
        public static double Bad     { get; private set; }
        public static double Miss    { get; private set; }

        public static void Initialize( double _OD )
        {
            OD = _OD;

            Maximum = 16.5d;
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
    public static bool CanBeHit( double _diff ) => -HitRange.Good <= _diff && _diff <= HitRange.Bad;
    public static bool IsEarlyMiss( double _diff ) => HitRange.Miss >= _diff && _diff > HitRange.Bad;
    public static bool IsLateMiss( double _diff ) => -HitRange.Good > _diff;

    //public static HitResult UpdateResult( int _lane, double _playback, double _diff, KeyState _keyState )
    //{
    //    //bool   isTail     = _isSlider && _keyState == KeyState.Up;
    //    //bool   isHeadMiss = _isSlider && _keyState == KeyState.Down;
    //    double diffAbs    = Math.Abs( _diff );

    //    // 판정 카운팅
    //    HitResult hitResult = GetHitResult( _diff, diffAbs );
    //    switch ( hitResult )
    //    {
    //        case HitResult.Maximum: Results.Maximum += 1; break;
    //        case HitResult.Perfect: Results.Perfect += 1; break;
    //        case HitResult.Great: Results.Great += 1; break;
    //        case HitResult.Good: Results.Good += 1; break;
    //        case HitResult.Bad: Results.Bad += 1; break;
    //        case HitResult.None:
    //        case HitResult.Miss: Results.Miss += 1; break;
    //    }

    //    Results.Combo = hitResult == HitResult.Miss ? 0 : Results.Combo += 1;
    //    Results.MaxCombo = Results.Combo > Results.MaxCombo ? Results.Combo : Results.MaxCombo;

    //    // Great ~ Bad까지만 집계
    //    if ( HitRange.Perfect < diffAbs && diffAbs <= HitRange.Bad )
    //    {
    //        Results.Fast += _diff > 0d ? 1 : 0;
    //        Results.Slow += _diff < 0d ? 1 : 0;
    //    }

    //    // 스코어
    //    int hitScore = GetHitScore( hitResult ); // 320, 300, 200, 100, 50, 0
    //    int hitBonus = GetHitBonus( hitResult ); // 2, 1, -8, -16, -44, -100
    //    Bonus = Math.Clamp( Bonus + hitBonus, 0, 100 ); // 판정에 따라 증감
    //    double bonusScore = Math.Sqrt( Bonus ) * Math.Clamp( 64 >> Convert.ToInt32( hitResult ), 0, HitScore.Maximum / 10 );

    //    // 기본 500,000, 보너스 500,000 => 최대 : ( 50만 / 전체판정수 ) * ( 320 + 320 ) / 320;
    //    Results.Score += MaxScore * ( hitScore + bonusScore ) / HitScore.Maximum;

    //    DataQueue.Enqueue( new HitData( _lane, _playback, _diff, hitResult, _keyState ) );
    //    return hitResult;
    //}

    private static void AddData( HitData _data ) => DataQueue.Enqueue( _data );
    
    public static void AddCombo()
    {
        UpdateCombo( HitResult.None );
        AddData( new HitData( HitResult.None ) );
    }

    private static void UpdateCombo( HitResult _hitResult )
    {
        Results.Combo    = _hitResult == HitResult.Miss ? 0 : Results.Combo += 1;
        Results.MaxCombo = Results.Combo > Results.MaxCombo ? Results.Combo : Results.MaxCombo;
    }

    private static void UpdateScore( HitResult _hitResult )
    {
                // 스코어
        int hitScore = GetHitScore( _hitResult ); // 320, 300, 200, 100, 50, 0
        int hitBonus = GetHitBonus( _hitResult ); // 2, 1, -8, -16, -44, -100
        Bonus = Math.Clamp( Bonus + hitBonus, 0, 100 ); // 판정에 따라 증감
        double bonusScore = Math.Sqrt( Bonus ) * Math.Clamp( 64 >> Convert.ToInt32( _hitResult ), 0, HitScore.Maximum / 10 );

        // 기본 500,000, 보너스 500,000 => 최대 : ( 50만 / 전체판정수 ) * ( 320 + 320 ) / 320;
        Results.Score += MaxScore * ( hitScore + bonusScore ) / HitScore.Maximum;
    }

    private static void UpdateCount( HitResult _hitResult, double _diff, double _diffAbs )
    {
        // 판정 카운팅
        switch ( _hitResult )
        {
            case HitResult.Maximum: Results.Maximum += 1; break;
            case HitResult.Perfect: Results.Perfect += 1; break;
            case HitResult.Great:   Results.Great   += 1; break;
            case HitResult.Good:    Results.Good    += 1; break;
            case HitResult.Bad:     Results.Bad     += 1; break;
            case HitResult.Miss:    Results.Miss    += 1; break;
            case HitResult.None:    Debug.LogError( "???" ); break;
        }

        // Great ~ Bad까지만 집계
        if ( HitRange.Perfect < _diffAbs && _diffAbs <= HitRange.Bad )
        {
            Results.Fast += _diff > 0d ? 1 : 0;
            Results.Slow += _diff < 0d ? 1 : 0;
        }
    }

    public static HitResult UpdateResult( Note _note, int _lane, double _playback, double _headDiff, double _tailDiff, KeyState _keyState )
    {
        bool isHead = _keyState == KeyState.Down;
        HitResult hitResult = GetHitResult( _headDiff, _tailDiff, isHead );

        double diff    = isHead ? _headDiff : _tailDiff;
        double diffAbs = Global.Math.Abs( diff );

        // 롱노트의 헤드 판정
        if ( isHead && _note.isSlider )
        {
            // 롱노트는 Head와 Tail의 타격 정보로 하나의 판정을 만든다.
            if ( hitResult == HitResult.Miss )
                 UpdateCount( hitResult, diff, diffAbs );

            UpdateCombo( hitResult ); // 롱노트 Head의 Late Miss도 갱신( Early Miss는 스킵 )
            AddData( new HitData( _lane, _playback, _headDiff, hitResult, _keyState ) );

            return HitResult.None;
        }

        // 하나의 판정이 완성되었을때 모든 정보 갱신
        UpdateCount( hitResult, diff, diffAbs );
        UpdateCombo( hitResult );
        UpdateScore( hitResult );
        AddData( new HitData( _lane, _playback, diff, hitResult, _keyState ) );

        return hitResult;
    }

    private static HitResult GetHitResult( double _headDiff, double _tailDiff, bool _isHead )
    {
        double headAbs  = Global.Math.Abs( _headDiff );
        if ( _isHead ) // 노트, 롱노트 Head 판정
        {
            if ( IsEarlyMiss( _headDiff )      ) return HitResult.Miss;
            if ( headAbs   <= HitRange.Maximum ) return HitResult.Maximum; // ----------------------
            if ( headAbs   <= HitRange.Perfect ) return HitResult.Perfect; //
            if ( headAbs   <= HitRange.Great   ) return HitResult.Great;   //        정상 판정
            if ( headAbs   <= HitRange.Good    ) return HitResult.Good;    //
            if ( IsLateMiss( _headDiff )       ) return HitResult.Miss;    // -Good ~ : 노트를 처리하지 못함( Late Miss )
            if ( headAbs   <= HitRange.Bad     ) return HitResult.Bad;     // ---------------------- // Late Bad는 Hit 불가
            
            return HitResult.None;                                         // 
        }
        else // 롱노트 Tail 판정
        {
            double tailAbs  = Global.Math.Abs( _tailDiff );
            double combined = headAbs + tailAbs;

            if ( _tailDiff >  HitRange.Bad ) /* Early Miss */                                  return HitResult.Miss;
            if ( headAbs   <= HitRange.Maximum * 1.2d && combined <= HitRange.Maximum * 2.4d ) return HitResult.Maximum; 
            if ( headAbs   <= HitRange.Perfect * 1.1d && combined <= HitRange.Perfect * 2.2d ) return HitResult.Perfect; 
            if ( headAbs   <= HitRange.Great          && combined <= HitRange.Great   * 2.0d ) return HitResult.Great;   
            if ( headAbs   <= HitRange.Good           && combined <= HitRange.Good    * 2.0d ) return HitResult.Good;
            if ( IsLateMiss( _tailDiff ) )                                                     return HitResult.Miss;

            return HitResult.Bad; // 위 판정을 제외한 최소 판정은 Bad
        }
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