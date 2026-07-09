using Cysharp.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Judgement : Singleton<Judgement>
{
    //private Results result;public int Maximum  { get; set; }
    public int Maximum  { get; private set; }
    public int Perfect  { get; private set; }
    public int Great    { get; private set; }
    public int Good     { get; private set; }
    public int Bad      { get; private set; }
    public int Miss     { get; private set; }
    public int Fast     { get; private set; }
    public int Slow     { get; private set; }
    public int Combo    { get; private set; }
    public int MaxCombo { get; private set; }
    public double Score { get; private set; }

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

    private double  maxScore;
    private int     bonus;

    private List<HitData>            hitDatas     = new ();
    private ConcurrentQueue<HitData> hitDataQueue = new ();
    //private CancellationTokenSource dataCts;

    public delegate void HitDataHandler( in HitData _data );
    public event HitDataHandler OnHitNote;

    // public static event Action<HitData> OnHitNote;

    protected override void Awake()
    {
        base.Awake();
        //NowPlaying.OnInitialize += Initialize; 
        //NowPlaying.OnRelease    += Release;
        //NowPlaying.OnClear      += Clear;
    }

    private void OnDestroy()
    {
        //NowPlaying.OnInitialize -= Initialize;
        //NowPlaying.OnRelease    -= Release;
        //NowPlaying.OnClear      -= Clear;
    }

    public void Initialize( in Song _song )
    {
        //result = NowPlaying.Inst.CurrentResult;
        maxScore = 500000d / ( _song.noteCount + _song.sliderCount );
        HitRange.Initialize( 0 );

        //DataProcess().Forget();
    }

    public void Clear()
    {
        bonus    = 100; // 최대 보너스
        Maximum  = 0;
        Perfect  = 0;
        Great    = 0;
        Good     = 0;
        Bad      = 0;
        Miss     = 0;
        Fast     = 0;
        Slow     = 0;
        Combo    = 0;
        MaxCombo = 0;
        Score    = 0d;

        hitDataQueue.Clear();
        hitDatas.Clear();
    }

    //private void Release()
    //{
    //    dataCts?.Cancel();
    //    dataCts?.Dispose();
    //    dataCts = null;
    //}

    private async UniTask DataProcess( CancellationToken _token )
    {
        //dataCts?.Cancel();
        //dataCts?.Dispose();
        //dataCts = new CancellationTokenSource();
        //CancellationToken token = dataCts.Token;
        try
        {
            while ( !_token.IsCancellationRequested )
            {
                while ( hitDataQueue.TryDequeue( out HitData hitData ) )
                {
                    hitDatas.Add( hitData );
                    OnHitNote?.Invoke( hitData );
                }

                await UniTask.Yield( PlayerLoopTiming.Update, _token );
            }
        }
        catch ( OperationCanceledException )
        {
            Debug.Log( "InputSystem HitData Process Cancel" );
        }
    }

    //public ResultData CurrentResult => Results;
    public bool CanBeHit( double _diff ) => -HitRange.Good <= _diff && _diff <= HitRange.Bad;
    public bool IsEarlyMiss( double _diff ) => HitRange.Miss >= _diff && _diff > HitRange.Bad;
    public bool IsLateMiss( double _diff ) => -HitRange.Good > _diff;
        
    public void AddCombo()
    {
        UpdateCombo( HitResult.None );
        hitDataQueue.Enqueue( new HitData( HitResult.None ) );
    }

    private void UpdateCombo( HitResult _hitResult )
    {
        Combo    = _hitResult == HitResult.Miss ? 0 : Combo += 1;
        MaxCombo = Combo > MaxCombo ? Combo : MaxCombo;
    }

    private void UpdateScore( HitResult _hitResult )
    {
        // 스코어
        int hitScore = GetHitScore( _hitResult ); // 320, 300, 200, 100, 50, 0
        int hitBonus = GetHitBonus( _hitResult ); // 2, 1, -8, -16, -44, -100
        bonus = Math.Clamp( bonus + hitBonus, 0, 100 ); // 판정에 따라 증감
        double bonusScore = Math.Sqrt( bonus ) * Math.Clamp( 64 >> Convert.ToInt32( _hitResult ), 0, 32/* HitScore.Maximum / 10 */ );

        // 기본 500,000, 보너스 500,000 => 최대 : ( 50만 / 전체판정수 ) * ( 320 + 320 ) / 320;
        Score += maxScore * ( hitScore + bonusScore ) / HitScore.Maximum;
    }

    private void UpdateCount( HitResult _hitResult, double _diff, double _diffAbs, bool _isHead )
    {
        // 판정 카운팅
        switch ( _hitResult )
        {
            case HitResult.Maximum: Maximum += 1; break;
            case HitResult.Perfect: Perfect += 1; break;
            case HitResult.Great:   Great   += 1; break;
            case HitResult.Good:    Good    += 1; break;
            case HitResult.Bad:     Bad     += 1; break;
            case HitResult.Miss:    Miss    += 1; break;
            case HitResult.None:    Debug.LogError( "???" ); break;
        }

        // Great ~ Bad까지만 집계
        if ( _isHead && ( HitRange.Perfect < _diffAbs && _diffAbs <= HitRange.Bad ) )
        {
            Fast += _diff > 0d ? 1 : 0;
            Slow += _diff < 0d ? 1 : 0;
        }
    }

    public HitResult UpdateResult( Note _note, int _lane, double _playback, double _headDiff, double _tailDiff, KeyState _keyState )
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
                 UpdateCount( hitResult, diff, diffAbs, isHead );

            UpdateCombo( hitResult ); // 롱노트 Head의 Late Miss도 갱신( Early Miss는 스킵 )
            hitDataQueue.Enqueue( new HitData( _lane, _playback, _headDiff, hitResult, _keyState ) );

            return HitResult.None;
        }

        // 하나의 판정이 완성되었을때 모든 정보 갱신
        UpdateCount( hitResult, diff, diffAbs, isHead );
        UpdateCombo( hitResult );
        UpdateScore( hitResult );
        hitDataQueue.Enqueue( new HitData( _lane, _playback, diff, hitResult, _keyState ) );

        return hitResult;
    }

    private HitResult GetHitResult( double _headDiff, double _tailDiff, bool _isHead )
    {
        if ( _isHead ) // 노트, 롱노트 Head 판정
        {
            if (  HitRange.Miss    >= _headDiff && _headDiff >  HitRange.Bad     ) return HitResult.Miss; // Early Miss
            if (  HitRange.Bad     >= _headDiff && _headDiff >  HitRange.Good    ) return HitResult.Bad;
            if (  HitRange.Good    >= _headDiff && _headDiff >  HitRange.Great   ) return HitResult.Good;
            if (  HitRange.Great   >= _headDiff && _headDiff >  HitRange.Perfect ) return HitResult.Great;
            if (  HitRange.Perfect >= _headDiff && _headDiff >  HitRange.Maximum ) return HitResult.Perfect;
            if (  HitRange.Maximum >= _headDiff && _headDiff > -HitRange.Maximum ) return HitResult.Maximum;
            if ( -HitRange.Maximum >= _headDiff && _headDiff > -HitRange.Perfect ) return HitResult.Perfect;
            if ( -HitRange.Perfect >= _headDiff && _headDiff > -HitRange.Great   ) return HitResult.Great;
            if ( -HitRange.Great   >= _headDiff && _headDiff > -HitRange.Good    ) return HitResult.Good;
            if ( -HitRange.Good    >  _headDiff )                                  return HitResult.Miss; // Late Miss

            // if ( headAbs   <= HitRange.Maximum ) return HitResult.Maximum; // ----------------------
            // if ( headAbs   <= HitRange.Perfect ) return HitResult.Perfect; //
            // if ( headAbs   <= HitRange.Great   ) return HitResult.Great;   //        정상 판정
            // if ( headAbs   <= HitRange.Good    ) return HitResult.Good;    //
            // if ( IsLateMiss( _headDiff )       ) return HitResult.Miss;    // -Good ~ : 노트를 처리하지 못함( Late Miss )
            // if ( headAbs   <= HitRange.Bad     ) return HitResult.Bad;     // ---------------------- // Late Bad는 Hit 불가

            return HitResult.None;                                         
        }
        else // 롱노트 Tail 판정
        {
            double headAbs  = Global.Math.Abs( _headDiff );
            double tailAbs  = Global.Math.Abs( _tailDiff );
            double combined = headAbs + tailAbs;

            if ( _tailDiff >  HitRange.Bad ) /* Early Miss */                                  return HitResult.Miss;
            if ( headAbs   <= HitRange.Maximum * 1.2d && combined <= HitRange.Maximum * 2.4d ) return HitResult.Maximum; 
            if ( headAbs   <= HitRange.Perfect * 1.1d && combined <= HitRange.Perfect * 2.2d ) return HitResult.Perfect; 
            if ( headAbs   <= HitRange.Great          && combined <= HitRange.Great   * 2.0d ) return HitResult.Great;   
            if ( headAbs   <= HitRange.Good           && combined <= HitRange.Good    * 2.0d ) return HitResult.Good;
            if ( _tailDiff > -HitRange.Good ) /* Late Miss */                                  return HitResult.Miss;

            return HitResult.Bad; // 위 판정을 제외한 최소 판정은 Bad
        }
    }

    private int GetHitScore( HitResult _result )
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

    private int GetHitBonus( HitResult _result )
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