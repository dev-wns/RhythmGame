using FMOD;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public enum SoundBuffer { _64, _128, _256, _512, _1024, Count, }
public enum SFX
{
    MainSelect, MainClick, MainHover, Slider,
    MenuSelect, MenuClick, MenuHover, MenuExit,
    //Clap,
}


public struct Music
{
    public Sound sound;
    public Channel channel;
    public Music( Sound _sound, Channel _channel )
    {
        sound = _sound;
        channel = _channel;
    }
}

public enum ChannelType : byte { Master, Clap, BGM, SFX, Count, };
public class AudioManager : Singleton<AudioManager>
{
    private static readonly int MaxSoftwareChannel = 128;
    private static readonly int MaxVirtualChannel  = 1000;
    private FMOD.System system;
    private Dictionary<ChannelType, ChannelGroup> groups    = new Dictionary<ChannelType, ChannelGroup>();
    private Dictionary<SFX, Sound>                sfxSounds = new Dictionary<SFX, Sound>();
    private Dictionary<DSP_TYPE, DSP>             dsps      = new Dictionary<DSP_TYPE, DSP>();
    public static event Action OnReload;
    public static event Action<float> OnUpdatePitch;

    public ReadOnlyCollection<SoundDriver> Drivers { get; private set; }
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

    #region Properties
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
    private int curDriverIndex = -1;
    public Sound MainSound { get; private set; }
    public Channel MainChannel { get; private set; }
    public uint Length => MainSound.getLength( out uint length, TIMEUNIT.MS ) == RESULT.OK ? length : uint.MaxValue;
    public int TotalKeySoundCount { get; private set; }
    public uint Position
    {
        get
        {
            if ( groups[ChannelType.BGM].isPlaying( out bool isPlaying ) != RESULT.OK )
            {
                Debug.LogError( "bgm is not playing" );
                return 0;
            }

            ErrorCheck( MainChannel.getPosition( out uint pos, TIMEUNIT.MS ) );
            return pos;
        }

        set
        {
            if ( groups[ChannelType.BGM].isPlaying( out bool isPlaying ) != RESULT.OK )
            {
                Debug.LogError( "bgm is not playing" );
                return;
            }

            ErrorCheck( MainChannel.setPosition( value, TIMEUNIT.MS ) );
        }
    }
    public int ChannelsInUse
    {
        get
        {
            ErrorCheck( system.getChannelsPlaying( out int channels ) );
            return channels;
        }
    }
    public bool IsStop { get; private set; }
    public float Volume { get; set; }
    #endregion
    #region System
    private Coroutine corVolumeEffect;
    private ADVANCEDSETTINGS advancedSettings;

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
            Debug.LogWarning( "using the old version." );

        // Sound Driver
        List<SoundDriver> drivers = new List<SoundDriver>();
        CreateDriverInfo( OUTPUTTYPE.WASAPI, drivers );
        //MakeDriverInfomation( OUTPUTTYPE.ASIO,   drivers );
        Drivers = new ReadOnlyCollection<SoundDriver>( drivers );
        if ( curDriverIndex < 0 )
        {
            ErrorCheck( system.getDriver( out int driverIndex ) );
            ErrorCheck( system.getDriverInfo( driverIndex, out string name, 256, out Guid guid, out int rate,
                                              out SPEAKERMODE driverSpeakMode, out int channels ) );
            for ( int i = 0; i < Drivers.Count; i++ )
            {
                if ( guid.CompareTo( Drivers[i].guid ) == 0 )
                {
                    curDriverIndex = i;
                    break;
                }
            }
        }

        // ChannelGroup
        for ( int i = 0; i < ( int )ChannelType.Count; i++ )
        {
            ChannelType type = ( ChannelType )i;
            ErrorCheck( system.createChannelGroup( type.ToString(), out ChannelGroup group ) );
            if ( type != ChannelType.Master )
                ErrorCheck( groups[ChannelType.Master].addGroup( group ) );

            groups.Add( type, group );
        }

        #region Details
        // DSP
        CreateDPS();

        // Sfx Sound
        Load( SFX.MainClick, @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\MainClick.wav" );
        Load( SFX.MenuClick, @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\MenuClick.wav" );

        Load( SFX.MainSelect, @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\MainSelect.wav" );
        Load( SFX.MenuSelect, @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\MenuSelect.wav" );

        Load( SFX.MainHover, @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\MainHover.wav" );
        Load( SFX.MenuHover, @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\MenuHover.wav" );

        Load( SFX.MenuExit, @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\MenuExit.wav" );

        Load( SFX.Slider, @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\Slider.wav" );
        //Load( SFX.Clap,   @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\Clap.wav" );

        // Details
        SetVolume( 1f, ChannelType.Master );
        SetVolume( .15f, ChannelType.BGM );
        SetVolume( .3f, ChannelType.SFX );
        SetVolume( .8f, ChannelType.Clap );
        #endregion

        Debug.Log( $"AudioManager Initialization" );
        Debug.Log( $"Sound Device : {Drivers[curDriverIndex].name}" );
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

        // Sounds
        foreach ( var sfx in sfxSounds.Values )
        {
            if ( sfx.hasHandle() )
            {
                ErrorCheck( sfx.release() );
                sfx.clearHandle();
            }
        }
        sfxSounds.Clear();

        // ChannelGroup
        for ( int i = 1; i < ( int )ChannelType.Count; i++ )
        {
            foreach ( var dsp in dsps.Values )
            {
                ErrorCheck( groups[( ChannelType )i].removeDSP( dsp ) );
            }

            ErrorCheck( groups[( ChannelType )i].getNumChannels( out int numChannels ) );
            for ( int j = 0; j < numChannels; j++ )
            {
                ErrorCheck( groups[( ChannelType )i].getChannel( j, out Channel channel ) );
                ErrorCheck( channel.getCurrentSound( out Sound sound ) );
                ErrorCheck( channel.stop() );
                ErrorCheck( sound.release() );
                sound.clearHandle();
            }

            ErrorCheck( groups[( ChannelType )i].release() );
        }

        ErrorCheck( groups[ChannelType.Master].release() );
        groups.Clear();

        // DSP
        foreach ( var dsp in dsps.Values )
        {
            ErrorCheck( dsp.release() );
            dsp.clearHandle();
        }
        dsps.Clear();

        // System
        ErrorCheck( system.release() ); // 내부에서 close 함.
        system.clearHandle();

        Debug.Log( "AudioManager Release" );
    }

    public void ReLoad()
    {
        IsStop = true;
        AllStop();

        // caching
        float[] volumes = new float[groups.Count];
        int groupCount = 0;
        foreach ( var group in groups.Values )
            ErrorCheck( group.getVolume( out volumes[groupCount++] ) );

        // reload
        Release();
        Initialize();
        OnReload?.Invoke();

        // rollback
        ErrorCheck( system.setOutput( Drivers[curDriverIndex].outputType ) );
        ErrorCheck( system.setDriver( Drivers[curDriverIndex].index ) );
        groupCount = 0;
        foreach ( var group in groups.Values )
            ErrorCheck( group.setVolume( volumes[groupCount++] ) );

        IsStop = false;
    }
    #endregion

    #region Unity Event Function
    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    public void SystemUpdate()
    {
        if ( !IsStop )
             system.update();
    }

    private void OnApplicationQuit()
    {
        IsStop = true;
    }

    private void OnDestroy()
    {
        // OnApplicationQuit -> OnDisable -> OnDestroy 순으로 호출 되기 때문에
        // 타 클래스에서 OnDisable, OnApplicationQuit로 사운드 관련 처리를 마친 후
        // AudioManager OnDestroy가 실행될 수 있도록 한다.
        Release();
    }
    #endregion

    #region Load
    /// <summary> Load Main BGM </summary>
    public void Load( string _path, bool _isLoop, bool _isStream )
    {
        MODE mode = _isStream ? MODE.CREATESTREAM : MODE.CREATESAMPLE;
        mode = _isLoop ? mode |= MODE.LOOP_NORMAL : mode |= MODE.LOOP_OFF;
        mode |= MODE.ACCURATETIME | MODE.LOWMEM;// | MODE.IGNORETAGS;

        ErrorCheck( system.createSound( _path, mode, out Sound sound ) );

        MainSound = sound;
    }

    /// <summary> Load Interface SFX </summary>
    private void Load( SFX _type, string _path )
    {
        if ( sfxSounds.ContainsKey( _type ) )
        {
            Debug.LogError( $"sfxSound[{_type}] is duplicate loaded." );
            return;
        }

        ErrorCheck( system.createSound( _path, MODE.LOOP_OFF | MODE.CREATESAMPLE | MODE.VIRTUAL_PLAYFROMSTART, out Sound sound ) );
        sfxSounds.Add( _type, sound );
    }

    public bool Load( string _path, out Sound _sound )
    {
        _sound = new Sound();
        
        if ( !System.IO.File.Exists( @_path ) )
             return false;

        ErrorCheck( system.createSound( _path, MODE.LOOP_OFF | MODE.CREATESAMPLE | MODE.LOWMEM, out _sound ) );
        return true;
    }
    #endregion
    #region Play
    /// <summary> Play Background Music </summary>
    public void Play( float _volume = 1f )
    {
        ErrorCheck( system.playSound( MainSound, groups[ChannelType.BGM], false, out Channel channel ) );
        ErrorCheck( channel.setVolume( _volume ) );
        MainChannel = channel;
    }

    /// <summary> Play Sound Special Effects </summary>
    public void Play( SFX _type )
    {
        if ( !sfxSounds.ContainsKey( _type ) )
        {
            Debug.LogError( $"sfxSound[{_type}] is not loaded." );
            return;
        }

        ErrorCheck( system.playSound( sfxSounds[_type], groups[ChannelType.SFX], false, out Channel channel ) );
    }

    /// <summary> Play Key Sound Effects </summary>

    public void Play( FMOD.Sound _sound, float _volume )
    {
        ErrorCheck( system.playSound( _sound, groups[ChannelType.BGM], true, out Channel channel ) );
        ErrorCheck( channel.setVolume( _volume ) );
        ErrorCheck( channel.setPaused( false ) );
    }
    #endregion

    #region Effect
    public void FadeVolume( float _start, float _end, float _t )
    {
        StopFadeEffect();
        corVolumeEffect = StartCoroutine( Fade( _start, _end, _t ) );
    }

    public void StopFadeEffect()
    {
        if ( !ReferenceEquals( corVolumeEffect, null ) )
        {
            StopCoroutine( corVolumeEffect );
            corVolumeEffect = null;
        }
    }

    public Coroutine FadeVolume( Music _music, float _start, float _end, float _t, Action _OnCompleted = null, float _OnCompletedDelay = 0f )
    {
        return StartCoroutine( Fade( _music.channel, _start, _end, _t, _OnCompleted, _OnCompletedDelay ) );
    }

    public IEnumerator Fade( Channel _channel, float _start, float _end, float _t, Action _OnCompleted, float _OnCompletedDelay )
    {
        // https://qa.fmod.com/t/fmod-isplaying-question-please-help/11481
        // isPlaying이 INVALID_HANDLE을 반환할 때 false와 동일하게 취급한다.
        if ( _channel.isPlaying( out bool isPlaying ) != RESULT.OK )
             yield break;

        // 같은 값일 때 계산 없이 종료
        if ( Global.Math.Abs( _start - _end ) < float.Epsilon )
        {
            ErrorCheck( _channel.setVolume( _end ) );
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
            ErrorCheck( _channel.setVolume( elapsedVolume ) );
        }

        // 페이드 인 기준으로 반복문이 끝난 시점에서 볼륨이 _end 값을 넘어가기 때문에 초기화해준다.
        ErrorCheck( _channel.setVolume( _end ) );

        yield return YieldCache.WaitForSeconds( _OnCompletedDelay );
        _OnCompleted?.Invoke();
    }

    /// <summary> BGM ChannelGroup Volume Fade </summary>
    private IEnumerator Fade( float _start, float _end, float _t )
    {
        if ( Global.Math.Abs( _start - _end ) < float.Epsilon )
        {
            ErrorCheck( groups[ChannelType.BGM].setVolume( _end ) );
            yield break;
        }

        float elapsedVolume = _start;
        float offset = _end - _start;
        ErrorCheck( groups[ChannelType.BGM].setVolume( _start ) );
        while ( _start < _end ? elapsedVolume < _end :
                                elapsedVolume > _end )
        {
            elapsedVolume += ( offset * Time.deltaTime ) / _t;
            ErrorCheck( groups[ChannelType.BGM].setVolume( elapsedVolume ) );
            yield return null;
        }

        ErrorCheck( groups[ChannelType.BGM].setVolume( _end ) );
    }

    #endregion
    #region ChannelGroup
    public bool IsPlaying( ChannelType _type )
    {
        ErrorCheck( groups[_type].isPlaying( out bool isPlaying ) );

        return isPlaying;
    }

    public void SetPitch( float _pitch, ChannelType _type )
    {
        ErrorCheck( groups[_type].setPitch( _pitch ) );
        UpdatePitchShift();

        OnUpdatePitch?.Invoke( _pitch );
    }

    public void PitchReset()
    {
        foreach ( var group in groups.Values )
        {
            ErrorCheck( group.setPitch( 1f ) );
            UpdatePitchShift();
        }
    }

    public void SetPaused( bool _isPause, ChannelType _type ) => ErrorCheck( groups[_type].setPaused( _isPause ) );

    public float GetVolume( ChannelType _type )
    {
        ErrorCheck( groups[_type].getVolume( out float chlVolume ) );
        return chlVolume;
    }

    public void SetVolume( float _value, ChannelType _type )
    {
        float chlVolume = _value;
        if ( _value < 0f ) chlVolume = 0f;
        if ( _value > 1f ) chlVolume = 1f;
        if ( _type == ChannelType.BGM )
            Volume = chlVolume;

        ErrorCheck( groups[_type].setVolume( chlVolume ) );
    }

    public void Release( Music _music )
    {
        if ( _music.channel.isPlaying( out bool isPlaying ) == RESULT.OK )
             ErrorCheck( _music.channel.stop() );

        if ( _music.sound.hasHandle() )
        {
            ErrorCheck( _music.sound.release() );
            _music.sound.clearHandle();
        }
    }

    public void Release( FMOD.Sound _sound )
    {
        if ( _sound.hasHandle() )
        {
            ErrorCheck( _sound.release() );
            _sound.clearHandle();
        }
    }

    public void AllStop()
    {
        foreach ( var group in groups )
        {
            if ( groups[group.Key].isPlaying( out bool isPlaying ) == RESULT.OK ) 
                 ErrorCheck( group.Value.stop() );
        }
    }
    #endregion

    #region DSP
    public string GetAppliedDSPName()
    {
        StringBuilder text = new StringBuilder();
        ErrorCheck( groups[ChannelType.BGM].getNumDSPs( out int num ) );
        for ( int i = 0; i < num; i++ )
        {
            ErrorCheck( groups[ChannelType.BGM].getDSP( i, out DSP dsp ) );
            ErrorCheck( dsp.getType( out DSP_TYPE type ) );
            text.Append( type ).Append( $" " );
        }

        return text.ToString();
    }

    public bool GetDSP( DSP_TYPE _type, out DSP _dsp )
    {
        if ( !dsps.ContainsKey( _type ) )
        {
            Debug.LogError( "DSP is not loaded." );
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

    public void AllRemoveDSP()
    {
        for ( int i = 1; i < ( int )ChannelType.Count; i++ )
        {
            ErrorCheck( groups[( ChannelType )i].getNumDSPs( out int oldNum ) );
            foreach ( var dsp in dsps.Values )
            {
                ErrorCheck( groups[( ChannelType )i].removeDSP( dsp ) );
            }

            ErrorCheck( groups[( ChannelType )i].getNumDSPs( out int newNum ) );
            Debug.Log( $"{( ChannelType )i} DSP Count : {oldNum} -> {newNum}" );
        }
    }

    private void CreateDPS()
    {
        CreateFFTWindow();
        CreatePitchShiftDSP();
    }

    private void CreateFFTWindow()
    {
        if ( dsps.ContainsKey( DSP_TYPE.FFT ) )
            return;

        ErrorCheck( system.createDSPByType( DSP_TYPE.FFT, out DSP dsp ) );
        ErrorCheck( dsp.setParameterInt( ( int )DSP_FFT.WINDOWSIZE, 4096 ) );
        ErrorCheck( dsp.setParameterInt( ( int )DSP_FFT.WINDOWTYPE, ( int )DSP_FFT_WINDOW.BLACKMANHARRIS ) );
        dsps.Add( DSP_TYPE.FFT, dsp );
    }

    private void CreatePitchShiftDSP()
    {
        if ( dsps.ContainsKey( FMOD.DSP_TYPE.PITCHSHIFT ) )
            return;

        ErrorCheck( system.createDSPByType( FMOD.DSP_TYPE.PITCHSHIFT, out FMOD.DSP dsp ) );
        ErrorCheck( dsp.setParameterFloat( ( int )FMOD.DSP_PITCHSHIFT.MAXCHANNELS, 2 ) );
        ErrorCheck( dsp.setParameterFloat( ( int )FMOD.DSP_PITCHSHIFT.FFTSIZE, 1024 ) );
        dsps.Add( FMOD.DSP_TYPE.PITCHSHIFT, dsp );
    }

    public void UpdatePitchShift()
    {
        if ( !dsps.ContainsKey( DSP_TYPE.PITCHSHIFT ) )
            return;

        float offset = GameSetting.CurrentPitchType == PitchType.Normalize ? 1f    / GameSetting.CurrentPitch :
                       GameSetting.CurrentPitchType == PitchType.Nightcore ? 1.1f  / GameSetting.CurrentPitch : 1f;

        ErrorCheck( dsps[DSP_TYPE.PITCHSHIFT].setParameterFloat( ( int )DSP_PITCHSHIFT.PITCH, offset ) );
    }
    #endregion

    private bool ErrorCheck( RESULT _result )
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
