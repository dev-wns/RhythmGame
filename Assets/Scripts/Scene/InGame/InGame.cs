using Cysharp.Threading.Tasks;
using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
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
    private UniTask playTask;
    private CancellationTokenSource playCts;

    private Song song;
    // Load Event
    public delegate void SongHandler( in Song _song );
    public event SongHandler   OnInitialize;   // Main  Thread
    public event SongHandler   OnLoadInThread; // Other Thread
    public event Func<UniTask> OnLoadAsync;    // Main  Thread
    public event Action        OnLoadEnd;
    public bool IsLoaded { get; private set; }
    
    // GameFlow Event
    public event Action       OnGameStart;
    public event Action       OnGameOver;
    public event Action       OnClear;   // 변수만 초기화하고 재사용할 때
    public event Action       OnRelease; // 리소스까지 제거
    public event Action<bool> OnPause;
    public event Action<double/* Playback */, double/* Distance */> OnUpdate;
    public bool IsStart  { get; private set; }

    private int totalJudge;

    // External Library
    [DllImport( "Assistant" )]
    private static extern void SetTimerResolution( bool isHighQuality );

    protected override void Awake()
    {
        base.Awake();

        IsInputLock = true;

        length  = Browser.Inst.SelectedSong.totalTime / GameSetting.SoundPitch;
        playCts = new CancellationTokenSource();

        //NowPlaying.OnGameOver += GameOver;
        OnKeyBind += KeyBind;

        SetTimerResolution( true );
    }

    protected override async void Start()
    {
        base.Start();

        await Load();

        playTask = Play( playCts.Token ); // 작업취소 확인을 위한 Task 캐싱
        await playTask;
    }

    private async UniTask Load()
    {
        NowPlaying.Inst.Initialize( Browser.Inst.SelectedSong /* Origin   Song */ );
        song       = NowPlaying.Inst.CurrentSong;             /* Playable Song */
        totalJudge = song.noteCount + song.sliderCount;
        OnInitialize?.Invoke( song );

        await UniTask.WhenAll( UniTask.RunOnThreadPool( () => OnLoadInThread?.Invoke( in song ) ),
                               AwaitUniTaskHandlers( OnLoadAsync ) );
        OnLoadEnd?.Invoke();

        // 로딩 후 대기시간
        await UniTask.WaitForSeconds( 3.5f, false, PlayerLoopTiming.Update );
        await loadingActivator.DisableCanvasAsync();
        IsLoaded = true;
    }

    private async UniTask AwaitUniTaskHandlers( Func<UniTask> _action )
    {
        if ( _action == null )
             return;

        foreach ( Func<UniTask> handler in _action.GetInvocationList() )
        {
            try                        { await handler(); } // 구독자 전부 대기
            catch ( Exception _error ) { Debug.LogWarning( _error ); }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        //playCts?.Cancel();
        //playCts?.Dispose();
        //playCts = null;

        //NowPlaying.OnGameOver -= GameOver;
        OnKeyBind -= KeyBind;
        SetTimerResolution( false );
    }

    private void Update()
    {
        timeImage.fillAmount = ( float )Global.Math.Clamp( ( NowPlaying.Playback / length ), 0d, 1d );
        if ( IsLoaded && NowPlaying.IsStart )
             OnUpdate?.Invoke( NowPlaying.Playback, NowPlaying.Distance );
    }

    protected override void Connect()
    {
        AudioManager.Inst.Pitch = GameSetting.SoundPitch;
        if ( GameSetting.CurrentPitchType != PitchType.None )
             AudioManager.Inst.AddDSP( FMOD.DSP_TYPE.PITCHSHIFT, ChannelType.BGM );
    }

    protected override void Disconnect()
    {
        if ( GameSetting.CurrentPitchType != PitchType.None )
             AudioManager.Inst.RemoveDSP( FMOD.DSP_TYPE.PITCHSHIFT, ChannelType.BGM );
    }

    protected override async UniTask Release()
    {
        if ( playCts != null )
        {
            playCts?.Cancel();

            try { await playTask; }
            catch ( OperationCanceledException )
            {
                Debug.Log( "InGame PlayTask Cancel" );
            }
            finally
            {
                playCts?.Dispose();
                playCts = null;
            }
        }

        OnRelease?.Invoke();
        await NowPlaying.Inst.Release();
        DataStorage.Inst.Release();
    }

    private async UniTask Play( CancellationToken _token )
    {
        // 게임 시작
        OnGameStart?.Invoke();
        // NowPlaying.Inst.GameStart();
        await UniTask.WaitUntil( () => NowPlaying.Playback > 0, PlayerLoopTiming.Update, _token );
        IsInputLock = false;

        // 게임 종료
        await UniTask.WaitUntil( () => totalJudge <= Judgement.Inst.Count , PlayerLoopTiming.Update, _token );
        Debug.Log( $"All lanes are empty ( {Judgement.Inst.Count} / {totalJudge} )" );

        await UniTask.WaitForSeconds( 5f, false, PlayerLoopTiming.Update, _token );

        await LoadScene( SceneType.Result );
    }

    public void BackToLobby()
    {
        // Async Forget( Unity Event )
        UniTask.Void( async () =>
        {
            //IsInputLock = true;
            //await Stop();
            await LoadScene( SceneType.FreeStyle );
        } );
    }

    public void Restart()
    {
        // Async Forget( Unity Event )
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

                OnClear?.Invoke();
                // NowPlaying.Inst.Clear();

                await FadeIn();

                OnGameStart?.Invoke();
                // NowPlaying.Inst.GameStart();
                await UniTask.WaitUntil( () => NowPlaying.Playback > 0 );
            }
            catch ( Exception _error ) { Debug.LogError( _error ); }
            finally                    { IsInputLock = false; }
        } );
    }

    public void Pause( bool _isPause )
    {
        // Async Forget( Unity Event )
        UniTask.Void( async () =>
        {
            if ( totalJudge <= Judgement.Inst.Count )
            {
                //IsInputLock = true;
                //await Stop();
                await LoadScene( SceneType.Result );
            }
            else
            {
                if ( _isPause ) pauseActivator.EnableCanvas();
                else            pauseActivator.DisableCanvas();

                //await NowPlaying.Inst.Pause( _isPause );
                OnPause?.Invoke( _isPause );
            }
        } );
    }

    public void GameOver()
    {
        gameOverActivator.EnableCanvas();
        OnGameOver?.Invoke();
    }

    public void KeyBind( UnityInputAction _input )
    {
        _input.Bind( SceneAction.Main,  KeyCode.Escape, () => { Pause( true  ); } );
        _input.Bind( SceneAction.Pause, KeyCode.Escape, () => { Pause( false ); } );

        _input.Bind( SceneAction.Main, KeyCode.F3, new InputRepeater( () => { GameSetting.ScrollSpeed -= 1; } ) );
        _input.Bind( SceneAction.Main, KeyCode.F4, new InputRepeater( () => { GameSetting.ScrollSpeed += 1; } ) );
    }
}