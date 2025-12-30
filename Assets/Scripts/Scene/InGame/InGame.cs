using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class InGame : Scene
{
    [Header( "Canvas Activator" )]
    [SerializeField] private LoadingActivator  loadingActivator;
    [SerializeField] private PauseActivator    pauseActivator;
    [SerializeField] private GameOverActivator gameOverActivator;

    [Header( "Fill Timer" )]
    public Image timeImage;
    private double length;

    [Header( "Asynchronous" )]
    private CancellationTokenSource playCts;

    public event Action OnLoadEnd;

    protected override void Awake()
    {
        base.Awake();

        IsInputLock = true;

        length  = NowPlaying.CurrentSong.totalTime / GameSetting.Pitch;
        playCts = new CancellationTokenSource();

        NowPlaying.OnGameOver += GameOver;
        OnKeyBind += KeyBind;
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
        OnKeyBind -= KeyBind;
    }

    private void Update()
    {
        timeImage.fillAmount = ( float )Global.Math.Clamp( ( NowPlaying.Playback / length ), 0d, 1d );
    }

    protected override void Connect()
    {
        AudioManager.Inst.Pitch = GameSetting.Pitch;
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
        await loadingActivator.DisableCanvasAsync( token );

        // if ( loadingCanvas.TryGetComponent( out CanvasGroup loadingGroup ) )
        // {
        //     await DOTween.To( () => 1f, x => loadingGroup.alpha = x, 0f, CanvasFadeDuration ).ToUniTask( TweenCancelBehaviour.Kill, token );
        //     loadingCanvas.SetActive( false );
        // }

        // 게임 시작
        NowPlaying.Inst.GameStart();
        await UniTask.WaitUntil( () => NowPlaying.Playback > 0, PlayerLoopTiming.Update, token );
        IsInputLock = false;

        // 게임 종료
        await UniTask.WaitUntil( () => NowPlaying.TotalJudge <= Judgement.CurrentResult.Count , PlayerLoopTiming.Update, token );
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

                pauseActivator.ImmediateDisableCanvas();
                gameOverActivator.ImmediateDisableCanvas();

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
                if ( _isPause ) pauseActivator.EnableCanvas();
                else            pauseActivator.DisableCanvas();

                await NowPlaying.Inst.Pause( _isPause );
            }
        } );
    }

    public void GameOver() => gameOverActivator.EnableCanvas();

    public void KeyBind( UnityInputAction _input )
    {
        _input.Bind( SceneAction.Main,  KeyCode.Escape, () => { Pause( true  ); } );
        _input.Bind( SceneAction.Pause, KeyCode.Escape, () => { Pause( false ); } );

        _input.Bind( SceneAction.Main, KeyCode.F3, new InputRepeater( () => { GameSetting.ScrollSpeed -= 1; } ) );
        _input.Bind( SceneAction.Main, KeyCode.F4, new InputRepeater( () => { GameSetting.ScrollSpeed += 1; } ) );
    }
}