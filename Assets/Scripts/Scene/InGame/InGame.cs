using DG.Tweening;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

public class InGame : Scene
{
    [Header( "InGame" )]
    public GameObject loadingCanvas;
    public OptionController pause, gameOver;
    public event Action OnLoadEnd;

    [Header( "Fill Timer" )]
    public Image timeImage;
    private double length;

    [Header( "Asynchronous" )]
    private CancellationTokenSource playCts;


    protected override void Awake()
    {
        base.Awake();

        IsInputLock = true;
        int antiAliasing = ( int )SystemSetting.CurrentAntiAliasing;
        QualitySettings.antiAliasing = antiAliasing == 1 ? 2 :
                                       antiAliasing == 2 ? 4 :
                                       antiAliasing == 3 ? 8 :
                                       antiAliasing == 4 ? 16 : 0;


        length  = NowPlaying.CurrentSong.totalTime / GameSetting.CurrentPitch;
        playCts = new CancellationTokenSource();

        NowPlaying.OnGameOver += GameOver;
        NowPlaying.Inst.Initialize();
    }

    protected override async void Start()
    {
        base.Start();

        await NowPlaying.Inst.Load();

        try { await Play(); } // 플레이 도중 일시정지하여 로비로 이동할 경우 ( 비동기 작업 취소 )
        catch ( OperationCanceledException ) { }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        playCts?.Cancel();
        playCts?.Dispose();
        playCts = null;

        NowPlaying.OnGameOver -= GameOver;
    }

    private void Update()
    {
        timeImage.fillAmount = ( float )Global.Math.Clamp( ( NowPlaying.Playback / length ), 0d, 1d );
    }

    public override void Connect()
    {
        AudioManager.Inst.Pitch = GameSetting.CurrentPitch;
        if ( GameSetting.CurrentPitchType != PitchType.None )
             AudioManager.Inst.AddDSP( FMOD.DSP_TYPE.PITCHSHIFT, ChannelType.BGM );
    }

    public override void Disconnect()
    {
        if ( GameSetting.CurrentPitchType != PitchType.None )
             AudioManager.Inst.RemoveDSP( FMOD.DSP_TYPE.PITCHSHIFT, ChannelType.BGM );
    }

    private async UniTask Play()
    {
        CancellationToken token = playCts.Token;

        // 로딩 완료 체크
        await UniTask.WaitUntil( () => NowPlaying.IsLoaded, PlayerLoopTiming.Update, token );
        OnLoadEnd?.Invoke();

        // 로딩 후 대기시간
        await UniTask.WaitForSeconds( 3.5f, false, PlayerLoopTiming.Update, token );
        if ( loadingCanvas.TryGetComponent( out CanvasGroup loadingGroup ) )
        {
            await DOTween.To( () => 1f, x => loadingGroup.alpha = x, 0f, Global.Const.CanvasFadeDuration ).ToUniTask( TweenCancelBehaviour.Kill, token );

            // yield return new WaitUntil( () => loadingGroup.alpha <= 0f );
            loadingCanvas.SetActive( false );
        }

        // 게임 시작
        NowPlaying.Inst.GameStart();
        await UniTask.WaitUntil( () => NowPlaying.Playback > 0, PlayerLoopTiming.Update, token );
        IsInputLock = false;

        // 게임 종료
        await UniTask.WaitUntil( () => NowPlaying.TotalJudge <= Judgement.CurrentResult.Count , PlayerLoopTiming.Update, token);
        Debug.Log( $"All lanes are empty ( {Judgement.CurrentResult.Count} Judgements )" );

        await UniTask.WaitForSeconds( 5f, false, PlayerLoopTiming.Update, token );

        await NowPlaying.Inst.Release();
        DataStorage.Inst.Release();

        await LoadScene( SceneType.Result );
    }

    public void BackToLobby()
    {
        UniTask.Void( async () =>
        {
            await NowPlaying.Inst.Release();
            DataStorage.Inst.Release();

            await LoadScene( SceneType.FreeStyle );
        } );
    }

    public void Restart()
    {
        UniTask.Void( async () =>
        {
            IsInputLock = true;
            await FadeOut();

            ImmediateDisableCanvas( ActionType.Main, pause );
            ImmediateDisableCanvas( ActionType.Main, gameOver );

            Disconnect();
            Connect();

            NowPlaying.Inst.Clear();

            await FadeIn();

            NowPlaying.Inst.GameStart();
            await UniTask.WaitUntil( () => NowPlaying.Playback > 0 );

            IsInputLock = false;
        } );
    }

    public void Pause( bool _isPause )
    {
        UniTask.Void( async () =>
        {
            if ( NowPlaying.TotalJudge <= Judgement.CurrentResult.Count )
            {
                playCts?.Cancel();
                playCts?.Dispose();
                playCts = null;

                await NowPlaying.Inst.Release();
                DataStorage.Inst.Release();

                await LoadScene( SceneType.Result );
            }
            else
            {
                if ( _isPause ) EnableCanvas(  ActionType.Pause, pause );
                else            DisableCanvas( ActionType.Main,  pause );
                await NowPlaying.Inst.Pause( _isPause );
            }
        } );
    }

    public void GameOver() => EnableCanvas( ActionType.GameOver, gameOver, false );

    public override void KeyBind()
    {
        // Main
        // Scroll Speed Down
        Bind( ActionType.Main, KeyState.Down, KeyCode.Alpha1, () => SpeedControlProcess( false ) );
        Bind( ActionType.Main, KeyState.Hold, KeyCode.Alpha1, () => PressedSpeedControl( false ) );
        Bind( ActionType.Main, KeyState.Up,   KeyCode.Alpha1, () => UpedSpeedControl() );
        // Scroll Speed Up                               
        Bind( ActionType.Main, KeyState.Down, KeyCode.Alpha2, () => SpeedControlProcess( true ) );
        Bind( ActionType.Main, KeyState.Hold, KeyCode.Alpha2, () => PressedSpeedControl( true ) );
        Bind( ActionType.Main, KeyState.Up,   KeyCode.Alpha2, () => UpedSpeedControl() );

        // Pause
        Bind( ActionType.Main,  KeyCode.Escape,    () => { Pause( true ); } );
        Bind( ActionType.Pause, KeyCode.Escape,    () => { Pause( false ); } );
        Bind( ActionType.Pause, KeyCode.DownArrow, () => { MoveToNextOption( pause ); } );
        Bind( ActionType.Pause, KeyCode.UpArrow,   () => { MoveToPrevOption( pause ); } );
        // Scroll Speed Down
        Bind( ActionType.Pause, KeyState.Down, KeyCode.Alpha1, () => SpeedControlProcess( false ) );
        Bind( ActionType.Pause, KeyState.Hold, KeyCode.Alpha1, () => PressedSpeedControl( false ) );
        Bind( ActionType.Pause, KeyState.Up,   KeyCode.Alpha1, () => UpedSpeedControl() );
        // Scroll Speed Up
        Bind( ActionType.Pause, KeyState.Down, KeyCode.Alpha2, () => SpeedControlProcess( true ) );
        Bind( ActionType.Pause, KeyState.Hold, KeyCode.Alpha2, () => PressedSpeedControl( true ) );
        Bind( ActionType.Pause, KeyState.Up,   KeyCode.Alpha2, () => UpedSpeedControl() );

        // GameOver
        Bind( ActionType.GameOver, KeyCode.DownArrow, () => { MoveToNextOption( gameOver ); } );
        Bind( ActionType.GameOver, KeyCode.UpArrow,   () => { MoveToPrevOption( gameOver ); } );
    }
}