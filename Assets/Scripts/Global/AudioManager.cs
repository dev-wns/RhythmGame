using FMOD;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using UnityEngine;
using Cysharp.Threading.Tasks;

public enum SFX
{
    MainSelect, MainClick, MainHover, Slider,
    MenuSelect, MenuClick, MenuHover, MenuExit,
    keyboard_Input, Keyboard_Backspace,
}

public struct AudioGroup
{
    public FMOD.Sound   sound;
    public FMOD.Channel channel;

    public void Release()
    {
        channel.stop();
        AudioManager.Inst.Release( sound );
    }

    public AudioGroup( FMOD.Sound _sound, FMOD.Channel _channel )
    {
        sound = _sound;
        channel = _channel;
    }
}

public struct SoundDriver : IEquatable<SoundDriver>
    {
        public int index; // OUTPUTTYPE에 해당하는 출력장치 인덱스
        public OUTPUTTYPE outputType;
        public Guid guid;
        public string name;
        public int systemRate, speakModeChannels;
        public SPEAKERMODE mode;

        public bool Equals( SoundDriver _other ) => index == _other.index;
        public override bool Equals( object _obj ) => Equals( ( SoundDriver )_obj );
        public override int GetHashCode() => base.GetHashCode();
    }

public enum ChannelType : byte { Master, Clap, BGM, SFX, Count, };
public class AudioManager : Singleton<AudioManager>
{
    private static readonly int MaxSoftwareChannel = 128;
    private static readonly int MaxVirtualChannel  = 1000;
    
    private FMOD.System system;
    private Dictionary<ChannelType, ChannelGroup> groups        = new ();
    private Dictionary<DSP_TYPE, DSP>             dsps          = new ();
    private Dictionary<SFX, Sound>                sfxSounds     = new ();

    public ReadOnlyCollection<SoundDriver> Drivers { get; private set; }
    public FMOD.Sound   MainSound                { get; private set; }
    public FMOD.Channel MainChannel              { get; private set; }

    public static event Action OnReload;
    public static event Action<float> OnUpdatePitch;

    // Thread
    private UniTask systemTask;
    private CancellationTokenSource breakPoint;
    private readonly long TargetFrame = 1000;
    public static Action OnUpdateThread;
    public static int AudioFPS { get; private set; }
    public static double DeltaTime { get; private set; }

    [DllImport( "Kernel32.dll" )]
    private static extern bool QueryPerformanceCounter( out long lpPerformanceCount );

    [DllImport( "Kernel32.dll" )]
    private static extern bool QueryPerformanceFrequency( out long lpFrequency );

    #region Property
    public uint  Length => MainSound.getLength( out uint length, TIMEUNIT.MS ) == RESULT.OK ? length : uint.MaxValue;
    public uint  Position
    {
        get
        {
            ErrorCheck( MainChannel.getPosition( out uint pos, TIMEUNIT.MS ) );
            return pos;
        }

        set => ErrorCheck( MainChannel.setPosition( value, TIMEUNIT.MS ) );
    }
    public float Volume
    {
        get
        {
            if ( MainChannel.isPlaying( out bool isPlaying ) != RESULT.OK )
                 return 0f;
                
            ErrorCheck( MainChannel.getVolume( out float volume ) );
            return volume;
        }

        set => ErrorCheck( MainChannel.setVolume( value ) );
    }
    public bool  Pause
    {
        get
        {
            ErrorCheck( groups[ChannelType.BGM].getPaused( out bool isPause ) );
            return isPause;
        }

        set => ErrorCheck( groups[ChannelType.BGM].setPaused( value ) );
    }
    public float Pitch
    {
        set
        {
            ErrorCheck( groups[ChannelType.BGM].setPitch( value ) );
            UpdatePitchShift();

            OnUpdatePitch?.Invoke( value );
        }
    }
    public int   ChannelsInUse
    {
        get
        {
            ErrorCheck( system.getChannelsPlaying( out int channels ) );
            return channels;
        }
    }
    public bool  IsStop { get; private set; }
    public int CurrentDriverIndex
    {
        get => curDriverIndex;
        set
        {
            if ( Drivers.Count <= value || curDriverIndex == value )
            {
                Debug.LogWarning( "SoundDriver Index is Out of Range or Duplicated Value" );
                return;
            }

            ErrorCheck( system.setOutput( Drivers[value].outputType ) );
            ErrorCheck( system.setDriver( Drivers[value].index ) );
            curDriverIndex = value;
        }
    }
    private int curDriverIndex = 0;
    #endregion

    #region System
    public void Initialize()
    {
        // System
        ErrorCheck( Factory.System_Create( out system ) );
        ErrorCheck( system.setOutput( OUTPUTTYPE.AUTODETECT ) );

        // To do Before System Initialize
        ErrorCheck( system.getSoftwareFormat( out int sampleRate, out SPEAKERMODE mode, out int numRawSpeakers ) );
        ErrorCheck( system.setSoftwareFormat( sampleRate, SPEAKERMODE.STEREO, numRawSpeakers ) );
        ErrorCheck( system.setSoftwareChannels( MaxSoftwareChannel ) );
        ErrorCheck( system.setDSPBufferSize( uint.Parse( SystemSetting.CurrentSoundBufferString ), 4 ) );

        // System Initialize
        IntPtr extraDriverData = new IntPtr();
        ErrorCheck( system.init( MaxVirtualChannel, INITFLAGS.NORMAL, extraDriverData ) );

        ErrorCheck( system.getVersion( out uint version ) );
        if ( version < VERSION.number )
             Debug.LogWarning( "Using the old version" );

        // Sound Driver
        List<SoundDriver> drivers = new List<SoundDriver>();
        CreateDriverInfo( OUTPUTTYPE.WASAPI, drivers );
        //MakeDriverInfomation( OUTPUTTYPE.ASIO,   drivers );
        Drivers = new ReadOnlyCollection<SoundDriver>( drivers );
        ErrorCheck( system.setOutput( Drivers[curDriverIndex].outputType ) );
        ErrorCheck( system.setDriver( Drivers[curDriverIndex].index ) );

        // ChannelGroup
        for ( int i = 0; i < ( int )ChannelType.Count; i++ )
        {
            ChannelType type = ( ChannelType )i;
            ErrorCheck( system.createChannelGroup( type.ToString(), out ChannelGroup group ) );
            if ( type != ChannelType.Master )
                ErrorCheck( groups[ChannelType.Master].addGroup( group ) );

            groups.Add( type, group );
        }

        // DSP
        if ( !dsps.ContainsKey( DSP_TYPE.FFT ) )
        {
            ErrorCheck( system.createDSPByType( DSP_TYPE.FFT, out DSP dsp ) );
            ErrorCheck( dsp.setParameterInt( ( int ) DSP_FFT.WINDOWSIZE, 4096 ) );
            ErrorCheck( dsp.setParameterInt( ( int ) DSP_FFT.WINDOWTYPE, ( int ) DSP_FFT_WINDOW.BLACKMANHARRIS ) );
            dsps.Add( DSP_TYPE.FFT, dsp );
        }

        //if ( !dsps.ContainsKey( DSP_TYPE.COMPRESSOR ) )
        //{
        //    ErrorCheck( system.createDSPByType( DSP_TYPE.COMPRESSOR, out DSP dsp ) );
        //    ErrorCheck( dsp.setParameterInt( ( int ) DSP_FFT.WINDOWSIZE, 4096 ) );
        //    ErrorCheck( dsp.setParameterInt( ( int ) DSP_FFT.WINDOWTYPE, ( int ) DSP_FFT_WINDOW.BLACKMANHARRIS ) );
        //    dsps.Add( DSP_TYPE.FFT, dsp );
        //}

        if ( !dsps.ContainsKey( DSP_TYPE.PITCHSHIFT ) )
        {
            ErrorCheck( system.createDSPByType( DSP_TYPE.PITCHSHIFT, out DSP dsp ) );
            ErrorCheck( dsp.setParameterFloat( ( int ) DSP_PITCHSHIFT.MAXCHANNELS, 0 ) );
            ErrorCheck( dsp.setParameterFloat( ( int ) DSP_PITCHSHIFT.FFTSIZE, 2048 ) );
            dsps.Add( DSP_TYPE.PITCHSHIFT, dsp );
        }

        // Load SFX Sounds
        Load( SFX.MainClick,          @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\MainClick.wav"  );
        Load( SFX.MenuClick,          @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\MenuClick.wav"  );
        Load( SFX.MainSelect,         @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\MainSelect.wav" );
        Load( SFX.MenuSelect,         @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\MenuSelect.wav" );
        Load( SFX.MainHover,          @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\MainHover.wav"  );
        Load( SFX.MenuHover,          @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\MenuHover.wav"  );
        Load( SFX.MenuExit,           @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\MenuExit.wav"   );
        Load( SFX.Slider,             @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\Slider.wav"     );
        Load( SFX.keyboard_Input,     @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\KeyInput.mp3" );
        Load( SFX.Keyboard_Backspace, @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\KeyBack.mp3" );

        // Details
        if ( Config.Inst.Read( ConfigType.Master, out float masterVolume ) ) SetVolume( masterVolume, ChannelType.Master );
        else                                                                 SetVolume( 1f,           ChannelType.Master );
        if ( Config.Inst.Read( ConfigType.BGM,    out float bgmVolume    ) ) SetVolume( bgmVolume,    ChannelType.BGM );
        else                                                                 SetVolume( 1f,           ChannelType.BGM );
        if ( Config.Inst.Read( ConfigType.SFX,    out float sfxVolume    ) ) SetVolume( sfxVolume,    ChannelType.SFX );
        else                                                                 SetVolume( 2f,           ChannelType.SFX );

        // Thread
        breakPoint ??= new CancellationTokenSource();
        systemTask = UniTask.RunOnThreadPool( () => { SystemUpdate( TargetFrame, breakPoint.Token ); } );

        Debug.Log( $"AudioManager Initialization" );
        //Debug.Log( $"Sound Device : {Drivers[curDriverIndex].name}" );
    }

    private void CreateDriverInfo( OUTPUTTYPE _type, List<SoundDriver> _list )
    {
        ErrorCheck( system.getOutput( out OUTPUTTYPE prevType ) );
        ErrorCheck( system.setOutput( _type ) );

        ErrorCheck( system.getNumDrivers( out int numDriver ) );
        for ( int i = 0; i < numDriver; i++ )
        {
            SoundDriver driver;
            if ( ErrorCheck( system.getDriverInfo( i, out driver.name, 256, out driver.guid, out driver.systemRate, out driver.mode, out driver.speakModeChannels ) ) )
            {
                driver.index = i;
                driver.outputType = _type;
                _list.Add( driver );
            }
        }

        ErrorCheck( system.setOutput( prevType ) );
    }

    public void Release()
    {
        IsStop = true;
        AllStop();

        // 그룹에 연결된 재생중인 채널 모두 끊김( 사운드 Release는 사용한 클래스에서 해주기 )
        for ( int i = 1; i < ( int )ChannelType.Count; i++ )
        {
            ChannelType type = ( ChannelType )i;
            foreach ( var dsp in dsps.Values )
            {
                ErrorCheck( groups[type].removeDSP( dsp ) );
            }

            //if ( ErrorCheck( groups[type].getNumChannels( out int numChannels ) ) )
            //{
            //    for ( int j = 0; j < numChannels; j++ )
            //    {
            //        ErrorCheck( groups[type].getChannel( j, out Channel channel ) );
            //        if ( ErrorCheck( channel.getCurrentSound( out Sound sound ) ) )
            //        {
            //            ErrorCheck( channel.stop() );
            //            ErrorCheck( sound.release() );
            //            sound.clearHandle();
            //        }
            //    }
            //}
            ErrorCheck( groups[( ChannelType )i].release() );
        }

        // 마스터 그룹은 하위 그룹 처리 후 제거
        ErrorCheck( groups[ChannelType.Master].release() );
        groups.Clear();

        // DSP
        foreach ( var dsp in dsps.Values )
        {
            ErrorCheck( dsp.release() );
            dsp.clearHandle();
        }
        dsps.Clear();

        foreach ( var sfx in sfxSounds.Values )
        {
            Release( sfx );
        }
        sfxSounds.Clear();

        // System
        ErrorCheck( system.release() ); // 내부에서 close 함
        system.clearHandle();

        Debug.Log( "AudioManager Release" );
    }

    public async void ReLoad()
    {
        IsStop = true;
        // caching
        float[] volumes = new float[groups.Count];
        int groupCount = 0;
        foreach ( var group in groups.Values )
            ErrorCheck( group.getVolume( out volumes[groupCount++] ) );

        // reload
        await ThreadCancel();
        Release();
        Initialize();

        // rollback
        groupCount = 0;
        foreach ( var group in groups.Values )
            ErrorCheck( group.setVolume( volumes[groupCount++] ) );

        OnReload?.Invoke();
        IsStop = false;
    }
    #endregion

    #region Unity Event Function
    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    private async void OnApplicationQuit()
    {
        IsStop = true;
        await ThreadCancel();
    }

    private void OnDestroy()
    {
        // OnApplicationQuit -> OnDisable -> OnDestroy 순으로 호출 되기 때문에
        // 타 클래스에서 OnDisable, OnApplicationQuit로 사운드 관련 처리를 마친 후
        // AudioManager OnDestroy가 실행될 수 있도록 한다.
        Release();
    }
    #endregion

    #region Thread
    public void SystemUpdate( long _targetFrame, CancellationToken _token )
    {

        QueryPerformanceFrequency( out long frequency );
        QueryPerformanceCounter( out long start );
        QueryPerformanceCounter( out long time );

        int  fps = 0;
        long end = 0;
        long targetTicks = frequency / _targetFrame; // 1 seconds = 10,000,000 ticks
        SpinWait spinner = new SpinWait();

        Debug.Log( $"AudioManager Thread Start( {_targetFrame} Frame )" );
        while ( !_token.IsCancellationRequested )
        {
            QueryPerformanceCounter( out end );
            if ( targetTicks <= ( end - start ) )
            {
                if ( !IsStop && system.hasHandle() )
                     system.update();

                OnUpdateThread?.Invoke();
                DeltaTime = ( double )( end - start ) / frequency;
                QueryPerformanceCounter( out start );
                fps++;
            }
            else
            {
                //Task.Delay( 1 );
                spinner.SpinOnce();
                spinner.Reset();
            }

            if ( frequency < ( end - time ) )
            {
                QueryPerformanceCounter( out time );
                AudioFPS = fps;
                fps = 0;
            }
        }

        _token.ThrowIfCancellationRequested();
    }

    private async UniTask ThreadCancel()
    {
        if ( breakPoint is null )
             return;

        breakPoint?.Cancel();
        try
        {
            if ( !systemTask.Status.IsCompleted() )
                 await systemTask;
        }
        catch ( OperationCanceledException )
        {
            Debug.Log( "AudioManager Thread Cancel Completed" );
        }
        finally
        {
            breakPoint?.Dispose();
            breakPoint = null;
            //systemTask = null;
        }
    }
    #endregion

    public bool Load( string _path, out Sound _sound, bool _isStream = false )
    {
        _sound = new Sound();
        if ( !System.IO.File.Exists( @_path ) )
              return false;

        MODE mode  = _isStream     ? MODE.CREATESTREAM : MODE.CREATESAMPLE;
             mode |= MODE.LOOP_OFF | MODE.ACCURATETIME | MODE.LOWMEM; // | MODE.VIRTUAL_PLAYFROMSTART

        ErrorCheck( system.createSound( _path, mode, out _sound ) );
        return true;
    }

    private void Load( SFX _type, string _path )
    {
        if ( !System.IO.File.Exists( @_path ) || sfxSounds.ContainsKey( _type ) )
        {
            Debug.LogWarning( $"SFX sound file does not exist or is already loaded" );
            return;
        }

        MODE mode = MODE.CREATESAMPLE | MODE.LOOP_OFF | MODE.LOWMEM | MODE.VIRTUAL_PLAYFROMSTART;
        if ( ErrorCheck( system.createSound( _path, mode, out Sound sound ) ) )
             sfxSounds.Add( _type, sound );
    }

    public void Play( Sound _sound, float _volume = 1f )
    {
        if ( !_sound.hasHandle() )
             Debug.LogWarning( "Sound Handle is not Alived" );

        if ( ErrorCheck( system.playSound( _sound, groups[ChannelType.BGM], false, out FMOD.Channel channel ) ) )
        {
            ErrorCheck( channel.setVolume( _volume ) );
            MainSound   = _sound;
            MainChannel = channel;
        }
    }

    public void Play( SFX _type )
    {
        if ( IsStop )
             return;

        if ( sfxSounds.TryGetValue( _type, out Sound sound ) )
             ErrorCheck( system.playSound( sound, groups[ChannelType.SFX], false, out FMOD.Channel channel ) );
    }

    public Coroutine Fade( FMOD.Channel _channel, float _start, float _end, float _t, Action _OnCompleted = null )
    {
        return StartCoroutine( FadeVolume( _channel, _start, _end, _t, _OnCompleted ) );
    }

    public IEnumerator FadeVolume( FMOD.Channel _channel, float _start, float _end, float _t, Action _OnCompleted )
    {
        // https://qa.fmod.com/t/fmod-isplaying-question-please-help/11481
        // isPlaying이 INVALID_HANDLE을 반환할 때 false와 동일하게 취급한다.
        if ( _channel.isPlaying( out bool isPlaying ) != RESULT.OK )
        {
            _OnCompleted?.Invoke();
            yield break;
        }

        // 같은 값일 때 계산 없이 종료
        if ( Global.Math.Abs( _start - _end ) < float.Epsilon )
        {
            ErrorCheck( _channel.setVolume( _end ) );
            _OnCompleted?.Invoke();
            yield break;
        }

        float elapsedVolume = _start;
        float offset = _end - _start;
        ErrorCheck( _channel.setVolume( _start ) );
        while ( _start < _end ? elapsedVolume < _end : // FADEIN
                                elapsedVolume > _end ) // FADEOUT
        {
            yield return YieldCache.WaitForEndOfFrame;
            elapsedVolume += ( offset * Time.deltaTime ) / _t;
            if ( _channel.isPlaying( out isPlaying ) != RESULT.OK )
            {
                _OnCompleted?.Invoke();
                yield break;
            }

            ErrorCheck( _channel.setVolume( elapsedVolume ) );
        }

        // 페이드 인 기준으로 반복문이 끝난 시점에서 볼륨이 _end 값을 넘어가기 때문에 초기화해준다.
        ErrorCheck( _channel.setVolume( _end ) );

        _OnCompleted?.Invoke();
    }

    public void PitchReset()
    {
        foreach ( var group in groups.Values )
        {
            ErrorCheck( group.setPitch( 1f ) );
            UpdatePitchShift();
        }
    }

    public float GetVolume( ChannelType _type )
    {
        ErrorCheck( groups[_type].getVolume( out float chlVolume ) );
        return chlVolume;
    }

    public void SetVolume( float _value, ChannelType _type )
    {
        float chlVolume = _value < 0f ? 0f :
                          _value > 1f ? 1f : _value;

        ErrorCheck( groups[_type].setVolume( chlVolume ) );
    }

    public void Release( Sound _sound )
    {
        if ( _sound.hasHandle() )
        {
            _sound.release();
            _sound.clearHandle();
        }
    }

    public void AllStop()
    {
        foreach ( var group in groups )
        {
            ErrorCheck( group.Value.stop() );
        }
    }

    #region DSP
    public bool GetDSP( DSP_TYPE _type, out DSP _dsp )
    {
        if ( !dsps.ContainsKey( _type ) )
        {
            Debug.LogError( "DSP is not loaded" );
            _dsp = new DSP();
            return false;
        }

        _dsp = dsps[_type];
        return true;
    }

    public void AddDSP( DSP_TYPE _dspType, ChannelType _channelType )
    {
        if ( !dsps.ContainsKey( _dspType ) )
             return;

        ErrorCheck( groups[_channelType].addDSP( CHANNELCONTROL_DSP_INDEX.TAIL, dsps[_dspType] ) );
    }

    public void RemoveDSP( DSP_TYPE _dspType, ChannelType _channelType )
    {
        if ( !dsps.ContainsKey( _dspType ) )
            return;

        ErrorCheck( groups[_channelType].removeDSP( dsps[_dspType] ) );
    }

    public void UpdatePitchShift()
    {
        if ( dsps.ContainsKey( DSP_TYPE.PITCHSHIFT ) )
        {
            float offset = GameSetting.CurrentPitchType == PitchType.Normalize ? 1f    / GameSetting.CurrentPitch :
                           GameSetting.CurrentPitchType == PitchType.Nightcore ? 1.1f  / GameSetting.CurrentPitch : 1f;

            ErrorCheck( dsps[DSP_TYPE.PITCHSHIFT].setParameterFloat( ( int ) DSP_PITCHSHIFT.PITCH, offset ) );
        }
    }
    #endregion

    private static bool ErrorCheck( RESULT _result )
    {
        #if UNITY_EDITOR
        if ( RESULT.OK != _result )
        {
            Debug.LogError( FMOD.Error.String( _result ) );
            return false;
        }

        return true;
        #else
        // 임시 로직.
        return FMOD.RESULT.OK == _result;
        // 에러일시 로그 파일 생성 후 파일에 쓰도록 구현하기
        #endif
    }
}
