using DG.Tweening;
using System;
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
    }

    protected override async void Start()
    {
        base.Start();

        NowPlaying.Inst.Initialize();
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

    protected override void Connect()
    {
        AudioManager.Inst.Pitch = GameSetting.CurrentPitch;
        if ( GameSetting.CurrentPitchType != PitchType.None )
             AudioManager.Inst.AddDSP( FMOD.DSP_TYPE.PITCHSHIFT, ChannelType.BGM );
    }

    protected override void Disconnect()
    {
        if ( GameSetting.CurrentPitchType != PitchType.None )
             AudioManager.Inst.RemoveDSP( FMOD.DSP_TYPE.PITCHSHIFT, ChannelType.BGM );
    }

    protected override async UniTask OnRelease()
    {
        playCts?.Cancel();
        playCts?.Dispose();
        playCts = null;
    
        await NowPlaying.Inst.Release();
        DataStorage.Inst.Release();
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
            await DOTween.To( () => 1f, x => loadingGroup.alpha = x, 0f, CanvasFadeDuration ).ToUniTask( TweenCancelBehaviour.Kill, token );
            loadingCanvas.SetActive( false );
        }

        // 게임 시작
        NowPlaying.Inst.GameStart();
        await UniTask.WaitUntil( () => NowPlaying.Playback > 0, PlayerLoopTiming.Update, token );
        IsInputLock = false;

        // 게임 종료
        await UniTask.WaitUntil( () => NowPlaying.TotalJudge <= Judgement.CurrentResult.Count , PlayerLoopTiming.Update, token);
        Debug.Log( $"All lanes are empty ( {Judgement.CurrentResult.Count} / {NowPlaying.TotalJudge} )" );

        await UniTask.WaitForSeconds( 5f, false, PlayerLoopTiming.Update, token );

        await LoadScene( SceneType.Result );
    }

    public void BackToLobby()
    {
        UniTask.Void( async () => await LoadScene( SceneType.FreeStyle ) );
    }

    public void Restart()
    {
        UniTask.Void( async () =>
        {
            try
            {
                IsInputLock = true;
                await FadeOut();

                ImmediateDisableCanvas( SceneAction.Main, pause );
                ImmediateDisableCanvas( SceneAction.Main, gameOver );

                Disconnect();
                Connect();

                NowPlaying.Inst.Clear();

                await FadeIn();

                NowPlaying.Inst.GameStart();
                await UniTask.WaitUntil( () => NowPlaying.Playback > 0 );
            }
            catch ( Exception _error ) { Debug.LogError( _error ); }
            finally                    { IsInputLock = false; }
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

                await LoadScene( SceneType.Result );
            }
            else
            {
                if ( _isPause ) EnableCanvas( SceneAction.Pause, pause );
                else            DisableCanvas( SceneAction.Main, pause );

                await NowPlaying.Inst.Pause( _isPause );
            }
        } );
    }

    public void GameOver() => EnableCanvas( SceneAction.GameOver, gameOver, false );

    public override void KeyBind()
    {
        // Main
        // Scroll Speed Down
        Bind( SceneAction.Main, KeyState.Down, KeyCode.F3, () => SpeedControlProcess( false ) );
        Bind( SceneAction.Main, KeyState.Hold, KeyCode.F3, () => PressedSpeedControl( false ) );
        Bind( SceneAction.Main, KeyState.Up,   KeyCode.F3, () => UpedSpeedControl() );
        // Scroll Speed Up                               
        Bind( SceneAction.Main, KeyState.Down, KeyCode.F4, () => SpeedControlProcess( true ) );
        Bind( SceneAction.Main, KeyState.Hold, KeyCode.F4, () => PressedSpeedControl( true ) );
        Bind( SceneAction.Main, KeyState.Up,   KeyCode.F4, () => UpedSpeedControl() );

        // Pause
        Bind( SceneAction.Main,  KeyCode.Escape,    () => { Pause( true ); } );
        Bind( SceneAction.Pause, KeyCode.Escape,    () => { Pause( false ); } );
        Bind( SceneAction.Pause, KeyCode.DownArrow, () => { MoveToNextOption( pause ); } );
        Bind( SceneAction.Pause, KeyCode.UpArrow,   () => { MoveToPrevOption( pause ); } );
        // Scroll Speed Down
        Bind( SceneAction.Pause, KeyState.Down, KeyCode.F3, () => SpeedControlProcess( false ) );
        Bind( SceneAction.Pause, KeyState.Hold, KeyCode.F3, () => PressedSpeedControl( false ) );
        Bind( SceneAction.Pause, KeyState.Up,   KeyCode.F3, () => UpedSpeedControl() );
        // Scroll Speed Up
        Bind( SceneAction.Pause, KeyState.Down, KeyCode.F4, () => SpeedControlProcess( true ) );
        Bind( SceneAction.Pause, KeyState.Hold, KeyCode.F4, () => PressedSpeedControl( true ) );
        Bind( SceneAction.Pause, KeyState.Up,   KeyCode.F4, () => UpedSpeedControl() );

        // GameOver
        Bind( SceneAction.GameOver, KeyCode.DownArrow, () => { MoveToNextOption( gameOver ); } );
        Bind( SceneAction.GameOver, KeyCode.UpArrow,   () => { MoveToPrevOption( gameOver ); } );
    }
}