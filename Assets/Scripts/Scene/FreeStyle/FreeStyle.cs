using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class FreeStyle : Scene
{
    [SerializeField] private FreeStyleMainScroll mainScroll;
    [SerializeField] private SoundPitchOption    pitchOption;

    private Song song;
    private double totalTime;
    private int    saveTime;
    public static int Playback { get; private set; }
    private GameTimer timer = new();

    private readonly float FadeDuration = 1.5f;
    private readonly int   SoundOffset  = -50;
    private bool canRestart = true;

    // Event
    public event SongHandler OnStart;
    public event SongHandler OnRestart;
    public event Func<Song, CancellationToken, UniTask> OnLoadAsync;

    // Synchronize
    private CancellationTokenSource loadCts;

    private Coroutine corFadeVolume;
    private bool isStart;

    // UI
    [SerializeField] private Slider posSlider;

    protected override void Awake()
    {
        base.Awake();

        OnKeyBind += KeyBind;
        mainScroll.OnSelectNotify  += UpdateSong;
        pitchOption.OnUpdatePitch  += UpdatePitch;
        AudioManager.Inst.OnReload += Connect;
        AudioManager.Inst.OnReload += UpdateAudioBuffer;
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();

        loadCts?.Cancel();
        loadCts?.Dispose();

        OnKeyBind -= KeyBind;
        mainScroll.OnSelectNotify  -= UpdateSong;
        pitchOption.OnUpdatePitch  -= UpdatePitch;
        AudioManager.Inst.OnReload -= Connect;
        AudioManager.Inst.OnReload -= UpdateAudioBuffer;

    }

    protected override void Start()
    {
        base.Start();

        DataStorage.Inst.LoadSongs();
    }

    private void Update()
    {
        if ( !isStart ) return;

        // double diff = Time.realtimeSinceStartupAsDouble - startTick;
        Playback = ( int )Math.Round( saveTime + ( timer.Milliseconds * GameSetting.SoundPitch ) );
        //OnUpdate?.Invoke( Playback );
        posSlider.value = ( float )( Playback / totalTime );

        if ( canRestart && totalTime < Playback )
        {
            if ( corFadeVolume is not null )
            {
                AudioManager.Inst.StopCoroutine( corFadeVolume );
                corFadeVolume = null;
            }

            canRestart = false;
            corFadeVolume = AudioManager.Inst.Fade( AudioManager.Inst.MainChannel, AudioManager.Inst.Volume, 0f, FadeDuration, () =>
            {
                AudioManager.Inst.Play( AudioManager.Inst.MainSound, 0f );
                AudioManager.Inst.Position = ( uint )song.previewTime;
                corFadeVolume = AudioManager.Inst.Fade( AudioManager.Inst.MainChannel, 0f, song.volume, FadeDuration );

                timer.Restart();
                saveTime  = song.previewTime + SoundOffset;
                Playback  = saveTime;
                OnRestart?.Invoke( in song );
                canRestart = true;
            } );
        }
    }

    private async void UpdateSong( Song _song )
    {
        // 이전 비동기 취소
        loadCts?.Cancel();
        loadCts?.Dispose();
        loadCts = new CancellationTokenSource();
        CancellationTokenSource currentCts = loadCts; // 이 비동기 흐름은 최신 곡이 아닐 수도 있기에 캐싱된 Cts 사용
        try 
        {
            if ( !AudioManager.Inst.Load( _song.audioPath, out FMOD.Sound curSound, true/* Stream */ ) )
                 return;

            song = _song;
            song.previewTime = GetPreviewTime( song );

            DataStorage.Inst.LoadChart( in song );
            await AwaitUniTaskHandlers( OnLoadAsync, song, currentCts.Token );

            // 이전 음악 페이드아웃 + 사운드 리소스 해제
            if ( corFadeVolume != null )
            {
                AudioManager.Inst.StopCoroutine( corFadeVolume );
                corFadeVolume = null;
            }
            AudioGroup prevAudio = new AudioGroup( AudioManager.Inst.MainSound, AudioManager.Inst.MainChannel );
            AudioManager.Inst.Fade( AudioManager.Inst.MainChannel, AudioManager.Inst.Volume, 0f, .5f, () => { prevAudio.Release(); } );
            
            if ( currentCts.IsCancellationRequested )
                 return; // Finally 진입

            // 최신 음악 재생 및 페이드인
            AudioManager.Inst.Play( curSound, 0f );

            AudioManager.Inst.Position = ( uint ) song.previewTime;
            AudioManager.Inst.Fade( AudioManager.Inst.MainChannel, 0f, song.volume, .5f );

            timer.Restart();
            saveTime  = song.previewTime + SoundOffset;
            Playback  = saveTime;
            
            float duration   = FadeDuration * 1000f;
            float diff       = AudioManager.Inst.Length - song.totalTime;
            totalTime = diff > duration ? song.totalTime : AudioManager.Inst.Length + 1000f - duration;
            
            OnStart?.Invoke( in song );
            canRestart = true;
            isStart    = true;
        }
        finally
        {
            // 이전 비동기 흐름이 최신 Cts를 해제하지않도록 방지
            if ( loadCts == currentCts )
            {
                loadCts?.Dispose();
                loadCts = null;
            }
        }
    }

    private async UniTask AwaitUniTaskHandlers( Func<Song, CancellationToken, UniTask> _action, Song _song, CancellationToken _token )
    {
        if ( _action == null )
             return;

        foreach ( Func<Song, CancellationToken, UniTask> handler in _action.GetInvocationList() )
        {
            // 리소스 로딩이 실패하더라도 있는만큼만 보여줌( 다른 Task 작업은 계속 진행 )
            try                        { await handler( _song, _token ); } // 구독자 전부 대기
            catch ( Exception _error ) { Debug.LogWarning( _error ); }
        }
    }

    private async void SelectChart()
    {
        AudioManager.Inst.Play( SFX.MainClick );
        await NowPlaying.CurrentScene.LoadScene( SceneType.Game );
    }

    private int GetPreviewTime( in Song _song ) => _song.previewTime <= 0 || _song.previewTime > _song.totalTime ? ( int )( _song.totalTime * .35f ) : _song.previewTime;

    private void UpdateAudioBuffer()
    {
        if ( AudioManager.Inst.Load( song.audioPath, out FMOD.Sound curSound, true ) )
        {
            Playback = ( int )Math.Round( saveTime + ( timer.Milliseconds * GameSetting.SoundPitch ) );
            AudioManager.Inst.Play( curSound );
            AudioManager.Inst.Position = ( uint )Playback;
        }
    }

    private void UpdatePitch( float _pitch )
    {
        saveTime = ( int )AudioManager.Inst.Position;
        timer.Restart();
    }

    protected override void Connect()
    {
        AudioManager.Inst.Pitch = GameSetting.SoundPitch;
        AudioManager.Inst.AddDSP( FMOD.DSP_TYPE.PITCHSHIFT, ChannelType.BGM );
        AudioManager.Inst.AddDSP( FMOD.DSP_TYPE.FFT,        ChannelType.BGM );
    }

    protected override void Disconnect()
    {
        AudioManager.Inst.RemoveDSP( FMOD.DSP_TYPE.PITCHSHIFT, ChannelType.BGM );
        AudioManager.Inst.RemoveDSP( FMOD.DSP_TYPE.FFT,        ChannelType.BGM );
    }

    public void KeyBind( UnityInputAction _input )
    {
        _input.Bind( SceneAction.Main, KeyCode.Return, SelectChart );

        _input.Bind( SceneAction.Main, KeyCode.F3, new InputRepeater( () => { GameSetting.ScrollSpeed -= 1; } ) );
        _input.Bind( SceneAction.Main, KeyCode.F4, new InputRepeater( () => { GameSetting.ScrollSpeed += 1; } ) );
    }
}