using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class FreeStyle : Scene
{
    [SerializeField] private FreeStyleMainScroll mainScroll;
    [SerializeField] private SoundPitchOption    pitchOption;

    private Song song;
    private double totalTime;
    private double startTick;
    private double saveTime;
    public  static double Playback;

    private readonly float FadeDuration = 2.5f;
    private readonly float SoundOffset  = -50f;
    private readonly int   InputDelay   = 150;
    private bool canRestart = true;

    // Event
    public event Func<Song, CancellationToken, UniTask> OnLoadAsync;
    public event SongHandler OnStart;
    public event SongHandler OnRestart;

    // Synchronize
    private CancellationTokenSource loadCts;

    private Coroutine corFadeVolume;
    private bool isStart;

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

        OnKeyBind -= KeyBind;
        mainScroll.OnSelectNotify  -= UpdateSong;
        pitchOption.OnUpdatePitch  -= UpdatePitch;
        AudioManager.Inst.OnReload -= Connect;
        AudioManager.Inst.OnReload -= UpdateAudioBuffer;

    }

    protected override void Start()
    {
        base.Start();

        if ( DataStorage.Inst.LoadSongs() )
        {
            //Searcher.Initialize();
        }
    }

    private void Update()
    {
        if ( !isStart ) return;

        Playback = saveTime + ( ( Time.realtimeSinceStartupAsDouble - startTick ) * GameSetting.SoundPitch * 1000d );
        if ( canRestart && totalTime < Playback )
        {
            if ( corFadeVolume is not null )
            {
                AudioManager.Inst.StopCoroutine( corFadeVolume );
                corFadeVolume = null;
            }

            canRestart = false;
            corFadeVolume = AudioManager.Inst.Fade( AudioManager.Inst.MainChannel, AudioManager.Inst.Volume, 0f, FadeDuration * .5f, () =>
            {
                AudioManager.Inst.Play( AudioManager.Inst.MainSound, 0f );
                AudioManager.Inst.Position = ( uint ) song.previewTime;
                corFadeVolume = AudioManager.Inst.Fade( AudioManager.Inst.MainChannel, 0f, song.volume, FadeDuration * .5f );

                startTick = Time.realtimeSinceStartupAsDouble;
                saveTime = song.previewTime + SoundOffset;
                Playback = saveTime;
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
        CancellationTokenSource currentCts = loadCts;
        try 
        {
            // 연타로 인한 불필요한 로딩 완화
            await UniTask.Delay( InputDelay, true, PlayerLoopTiming.Update, currentCts.Token );

            if ( !AudioManager.Inst.Load( _song.audioPath, out FMOD.Sound curSound, true/* Stream */ ) )
                 return; // Finally 실행

            song = _song;
            if ( song.previewTime <= 0 || song.previewTime > song.totalTime )
                 song.previewTime = ( int )( song.totalTime * .35f );

            await AwaitUniTaskHandlers( OnLoadAsync, song, currentCts.Token );

            // 이전 음악 페이드아웃 + 사운드 리소스 해제
            if ( corFadeVolume != null )
            {
                AudioManager.Inst.StopCoroutine( corFadeVolume );
                corFadeVolume = null;
            }
            AudioGroup prevAudio = new AudioGroup( AudioManager.Inst.MainSound, AudioManager.Inst.MainChannel );
            AudioManager.Inst.Fade( AudioManager.Inst.MainChannel, AudioManager.Inst.Volume, 0f, .5f, () => { prevAudio.Release(); } );
            
            // 현재 비동기 흐름에서 Cancel 됐는지 확인( 최신 곡이 아닐 수도 있기에 캐싱된 Cts로 확인 )
            if ( currentCts.IsCancellationRequested )
                 return; // Finally 실행

            // 최신 음악 재생 및 페이드인
            AudioManager.Inst.Play( curSound, 0f );
            AudioManager.Inst.Position = ( uint )song.previewTime;
            AudioManager.Inst.Fade( AudioManager.Inst.MainChannel, 0f, song.volume, .5f );
            
            startTick = Time.realtimeSinceStartupAsDouble;
            saveTime  = song.previewTime + SoundOffset;
            Playback  = saveTime;
            
            float duration   = FadeDuration * 1000f * .5f;
            float diff       = AudioManager.Inst.Length - song.totalTime;
            totalTime = diff > duration ? song.totalTime : AudioManager.Inst.Length + 1000f - duration;
            
            OnStart?.Invoke( in song );
            canRestart = true;
            isStart    = true;
        }
        catch( OperationCanceledException ) { /* Debug.LogWarning( _error ); */ } // 정상
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
            try                 { await handler( _song, _token ); } // 구독자 전부 대기
            catch ( Exception ) { /* Debug.LogWarning( _error ); */ }
        }
    }

    private async void SelectChart()
    {
        AudioManager.Inst.Play( SFX.MainClick );
        await NowPlaying.CurrentScene.LoadScene( SceneType.Game );
    }

    private void UpdateAudioBuffer()
    {
        if ( AudioManager.Inst.Load( song.audioPath, out FMOD.Sound curSound, true ) )
        {
            Playback = saveTime + ( ( Time.realtimeSinceStartupAsDouble - startTick ) * GameSetting.SoundPitch * 1000d );
            AudioManager.Inst.Play( curSound );
            AudioManager.Inst.Position = ( uint )Playback;
        }
    }

    private void UpdatePitch( float _pitch )
    {
        saveTime  = AudioManager.Inst.Position;
        startTick = Time.realtimeSinceStartupAsDouble;
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