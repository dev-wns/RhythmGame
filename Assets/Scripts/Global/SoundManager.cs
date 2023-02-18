using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Windows;
using UnityEngine.UI;

public enum SoundBuffer { _64, _128, _256, _512, _1024, Count, }
public enum SoundSfxType 
{ 
    MainSelect, MainClick, MainHover, Slider,
    MenuSelect, MenuClick, MenuHover,
    Clap,
}


public enum ChannelType : byte { Master, Clap, BGM, SFX, Count, };
public struct Music
{
    public FMOD.Sound sound;
    public FMOD.Channel channel;
    public Music( FMOD.Sound _sound, FMOD.Channel _channel )
    {
        sound = _sound;
        channel = _channel;
    }
}

public class SoundManager : Singleton<SoundManager>
{
    #region variables
    private static readonly int MaxSoftwareChannel = 256;
    private static readonly int MaxVirtualChannel  = 1000;
    private Dictionary<ChannelType, FMOD.ChannelGroup>   groups    = new Dictionary<ChannelType, FMOD.ChannelGroup>();
    private Dictionary<SoundSfxType, FMOD.Sound>         sfxSounds = new Dictionary<SoundSfxType, FMOD.Sound>();
    private Dictionary<string/* 키음 이름 */, FMOD.Sound> keySounds = new Dictionary<string, FMOD.Sound>();
    private Dictionary<FMOD.DSP_TYPE, FMOD.DSP>          dsps      = new Dictionary<FMOD.DSP_TYPE, FMOD.DSP>();
    private FMOD.System system;
    public FMOD.Sound MainSound { get; private set; }
    public FMOD.Channel MainChannel { get; private set; }
    private int curDriverIndex = -1;
    public event Action OnReload;
    public struct SoundDriver : IEquatable<SoundDriver>
    {
        public int index; // OUTPUTTYPE에 해당하는 출력장치 인덱스
        public FMOD.OUTPUTTYPE outputType;
        public Guid guid;
        public string name;
        public int systemRate, speakModeChannels;
        public FMOD.SPEAKERMODE mode;

        public bool Equals( SoundDriver _other ) => index == _other.index;
        public override bool Equals( object _obj ) => Equals( ( SoundDriver )_obj );
        public override int GetHashCode() => base.GetHashCode();
    }
    public ReadOnlyCollection<SoundDriver> Drivers { get; private set; } 
    /// <summary>
    /// The accuratetime flag is required.
    /// </summary>
    public uint Length {
        get {
            //if ( !hasAccurateFlag || !IsPlaying( ChannelType.BGM ) ) {
            //    Debug.LogWarning( $"No AccurateTime flag or BGM Sound." );
            //    return uint.MaxValue;
            //}

            ErrorCheck( MainSound.getLength( out uint length, FMOD.TIMEUNIT.MS ) );
            return length;
        }
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
            Debug.Log( curDriverIndex );
        }
    }
    public int KeySoundCount => keySounds.Count;
    public int TotalKeySoundCount { get; private set; }
    /// <summary>
    /// BGM Position
    /// </summary>
    public uint Position
    {
        get
        {
            if ( !IsPlaying( ChannelType.BGM ) )
            {
                Debug.LogError( "bgm is not playing" );
                return 0;
            }

            uint pos;
            ErrorCheck( MainChannel.getPosition( out pos, FMOD.TIMEUNIT.MS ) );
            return pos;
        }

        set
        {
            if ( !IsPlaying( ChannelType.BGM ) )
            {
                Debug.LogError( "bgm is not playing" );
                return;
            }
            
            ErrorCheck( MainChannel.setPosition( value, FMOD.TIMEUNIT.MS ) );
        }
    }
    public int UseChannelCount
    {
        get
        {
            ErrorCheck( system.getChannelsPlaying( out int channels ) );
            return channels;
        }
    }
    public bool IsLoad { get; private set; }
    public float Volume { get; private set; }
    #endregion
    #endregion
    #region System
    public void Initialize()
    {
        // System
        ErrorCheck( FMOD.Factory.System_Create( out system ) );
        ErrorCheck( system.setOutput( FMOD.OUTPUTTYPE.AUTODETECT ) );
        // ErrorCheck( system.setOutput( FMOD.OUTPUTTYPE.AUTODETECT ) );
        //ErrorCheck( system.setOutput( FMOD.OUTPUTTYPE.ASIO ) );

        // To do Before System Initialize
        ErrorCheck( system.getSoftwareFormat( out int sampleRate, out FMOD.SPEAKERMODE mode, out int numRawSpeakers ) );
        ErrorCheck( system.setSoftwareFormat( sampleRate, FMOD.SPEAKERMODE.STEREO, numRawSpeakers ) );
        ErrorCheck( system.setSoftwareChannels( MaxSoftwareChannel ) );
        ErrorCheck( system.setDSPBufferSize( uint.Parse( SystemSetting.CurrentSoundBufferString ), 4 ) );

        // System Initialize
        IntPtr extraDriverData = new IntPtr();
        ErrorCheck( system.init( MaxVirtualChannel, FMOD.INITFLAGS.NORMAL, extraDriverData ) );
        ErrorCheck( system.getVersion( out uint version ) );
        if ( version < FMOD.VERSION.number )
             Debug.LogError( "using the old version." );

        // Sound Driver
        List<SoundDriver> drivers = new List<SoundDriver>();
        MakeDriverInfomation( FMOD.OUTPUTTYPE.WASAPI, drivers );
        MakeDriverInfomation( FMOD.OUTPUTTYPE.ASIO,   drivers );
        Drivers = new ReadOnlyCollection<SoundDriver>( drivers );
        if ( curDriverIndex < 0 )
        {
            ErrorCheck( system.getDriver( out int driverIndex ) );
            ErrorCheck( system.getDriverInfo( driverIndex, out string name, 256, out Guid guid, out int rate, out FMOD.SPEAKERMODE driverSpeakMode, out int channels ) );
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
            FMOD.ChannelGroup group;
            ChannelType type = ( ChannelType )i;

            ErrorCheck( system.createChannelGroup( type.ToString(), out group ) );
            if ( type != ChannelType.Master )
                ErrorCheck( groups[ChannelType.Master].addGroup( group ) );

            groups.Add( type, group );
        }

        #if UNITY_EDITOR
        PrintSystemSetting();
        #endif

        #region Details
        // DSP
        CreateDPS();

        // Sfx Sound
        Load( SoundSfxType.MainClick, @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\MainClick.wav" );
        Load( SoundSfxType.MenuClick, @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\MenuClick.wav" );
        
        Load( SoundSfxType.MainSelect, @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\MainSelect.wav" );
        Load( SoundSfxType.MenuSelect, @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\MenuSelect.wav" );

        Load( SoundSfxType.MainHover, @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\MainHover.wav" );
        Load( SoundSfxType.MenuHover, @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\MenuHover.wav" );

        Load( SoundSfxType.Slider, @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\Slider.wav" );
        Load( SoundSfxType.Clap,   @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\Clap.wav" );

        // Details
        SetVolume( .5f, ChannelType.Master );
        SetVolume( .1f, ChannelType.BGM );
        SetVolume( .3f, ChannelType.SFX );
        SetVolume( .075f, ChannelType.Clap );
        #endregion
        Debug.Log( "SoundManager initialization completed" );
    }

    private void PrintSystemSetting()
    {
        ErrorCheck( system.getSoftwareFormat( out int sampleRate, out FMOD.SPEAKERMODE mode, out int numRawSpeakers ) );
        Debug.Log( $"SampleRate : {sampleRate}  Mode : {mode}  RawSpeakers : {numRawSpeakers}" );
        ErrorCheck( system.getSoftwareChannels( out int softwareChannels ) );
        Debug.Log( $"SoftwareChannel {softwareChannels}" );
        ErrorCheck( system.getDSPBufferSize( out uint bufferSize, out int numbuffers ) );
        Debug.Log( $"Buffers : {numbuffers}  BufferSize : {bufferSize}" );
        Debug.Log( $"Current Sound Device : {Drivers[curDriverIndex].name}" );
    }

    private void MakeDriverInfomation( FMOD.OUTPUTTYPE _type, List<SoundDriver> _list )
    {
        ErrorCheck( system.getOutput( out FMOD.OUTPUTTYPE prevType ) );
        ErrorCheck( system.setOutput( _type ) );

        ErrorCheck( system.getNumDrivers( out int numDriver ) );
        for ( int i = 0; i < numDriver; i++ )
        {
            SoundDriver driver;
            if ( ErrorCheck( system.getDriverInfo( i, out driver.name, 256, out driver.guid, out driver.systemRate, out driver.mode, out driver.speakModeChannels ) ) )
            {
                driver.index      = i;
                driver.outputType = _type;
                _list.Add( driver );
            }
        }

        ErrorCheck( system.setOutput( prevType ) );
    }

    public void KeyRelease()
    {
        TotalKeySoundCount = 0;
        foreach ( var keySound in keySounds )
        {
            var sound = keySound.Value;
            if ( sound.hasHandle() )
            {
                ErrorCheck( sound.release() );
                sound.clearHandle();
            }
        }
        keySounds.Clear();
    }

    public void Release()
    {
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

        foreach ( var keySound in keySounds )
        {
            var sound = keySound.Value;
            if ( sound.hasHandle() )
            {
                ErrorCheck( sound.release() );
                sound.clearHandle();
            }
        }
        keySounds.Clear();

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
                ErrorCheck( groups[( ChannelType )i].getChannel( j, out FMOD.Channel channel ) );
                ErrorCheck( channel.stop() );
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

        Debug.Log( "SoundManager release" );
    }

    public void ReLoad()
    {
        IsLoad = true;
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
        Debug.Log( Drivers[curDriverIndex].outputType );
        Debug.Log( Drivers[curDriverIndex].index );
        groupCount = 0;
        foreach ( var group in groups.Values )
            ErrorCheck( group.setVolume( volumes[groupCount++] ) );

        IsLoad = false;
    }
    #endregion
    #region Unity Event Function
    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    private void Update()
    {
        if ( !IsLoad ) 
             system.update();
    }

    private void OnDestroy()
    {
        // 매니저격 클래스라 가장 마지막에 제거되어야 한다.
        // OnApplicationQuit -> OnDisable -> OnDestroy 순으로 호출 되기 때문에
        // 타 클래스에서 OnDisable, OnApplicationQuit로 사운드 관련 처리를 마친 후
        // SoundManager OnDestroy가 실행될 수 있도록 한다.
        Release();
    }
    #endregion
    #region Load
    /// <summary> Load Main BGM </summary>
    public void Load( string _path, bool _isLoop, bool _isStream )
    {
        FMOD.MODE mode = _isStream ? FMOD.MODE.CREATESTREAM : FMOD.MODE.CREATESAMPLE;
        mode           = _isLoop   ? mode |= FMOD.MODE.LOOP_NORMAL  : mode |= FMOD.MODE.LOOP_OFF;
        mode           |=  FMOD.MODE.ACCURATETIME;

        ErrorCheck( system.createSound( _path, mode, out FMOD.Sound sound ) );
        MainSound = sound;
    }

    /// <summary> Load Interface SFX </summary>
    private void Load( SoundSfxType _type, string _path )
    {
        if ( sfxSounds.ContainsKey( _type ) )
        {
            Debug.LogError( $"sfxSound[{_type}] is duplicate loaded." );
            return;
        }

        ErrorCheck( system.createSound( _path, FMOD.MODE.LOOP_OFF | FMOD.MODE.CREATESAMPLE, out FMOD.Sound sound ) );
        sfxSounds.Add( _type, sound );
    }

    /// <summary> Load KeySound </summary>
    public bool Load( string _path )
    {
        var name = System.IO.Path.GetFileName( _path );
        if ( keySounds.ContainsKey( name ) )
        {
            ++TotalKeySoundCount;
            //_sound = keySounds[name];
        }
        else if ( System.IO.File.Exists( @_path ) )
        {
            ErrorCheck( system.createSound( _path, FMOD.MODE.LOOP_OFF | FMOD.MODE.CREATESAMPLE, out FMOD.Sound sound ) );
            keySounds.Add( name, sound );
        }
        //   if ( !System.IO.File.Exists( @_path ) )
        else
        {
            // throw new Exception( $"File Exists  {_path}" );
            //_sound = new FMOD.Sound();
            return false;
        }

        return true;
    }
    #endregion
    #region Play
    /// <summary> Play Background Music </summary>
    public void Play( float _volume = 1f )
    {
        //if ( !MainSound.hasHandle() )
        //{
        //    Debug.LogError( "Bgm is not loaded." );
        //    return;
        //}

        ErrorCheck( system.playSound( MainSound, groups[ChannelType.BGM], false, out FMOD.Channel channel ) );
        ErrorCheck( channel.setVolume( _volume ) );
        MainChannel = channel;
    }

    /// <summary> Play Sound Special Effects </summary>
    public void Play( SoundSfxType _type )
    {
        if ( !sfxSounds.ContainsKey( _type ) )
        {
            Debug.LogError( $"sfxSound[{_type}] is not loaded." );
            return;
        }

        if ( _type == SoundSfxType.Clap ) ErrorCheck( system.playSound( sfxSounds[_type], groups[ChannelType.Clap], false, out FMOD.Channel channel ) );
        else                              ErrorCheck( system.playSound( sfxSounds[_type], groups[ChannelType.SFX],  false, out FMOD.Channel channel ) );
    }

    /// <summary> Play Key Sound Effects </summary>
    public void Play( KeySound _sound )
    {
        if ( !keySounds.ContainsKey( _sound.name ) )
        {
            //Debug.LogWarning( $"keySound[{_keySound.name}] is not loaded." );
            return;
        }
        
        ErrorCheck( system.playSound( keySounds[_sound.name], groups[ChannelType.BGM], false, out FMOD.Channel channel ) );
        ErrorCheck( channel.setVolume( _sound.volume ) );
    }
    #endregion
    #region Effect

    private Coroutine volumeEffectCoroutine;

    /// <summary>
    /// FadeIn when _end is greater than _start. <br/>
    /// FadeOut in the opposite case.
    /// </summary>
    public void FadeVolume( float _start, float _end, float _t )
    {
        if ( !ReferenceEquals( volumeEffectCoroutine, null ) )
        {
            StopCoroutine( volumeEffectCoroutine );
            volumeEffectCoroutine = null;
        }

        volumeEffectCoroutine = StartCoroutine( Fade( _start, _end, _t ) );
    }

    public void FadeVolume( Music _music, float _start, float _end, float _t, Action _OnCompleted = null )
    {
        StartCoroutine( Fade( _music, _start, _end, _t, _OnCompleted ) );
    }

    public IEnumerator Fade( Music _music, float _start, float _end, float _t, Action _OnCompleted )
    {
        // 플레이 중이 아니면 Channel의 대부분의 함수는 사용할 수 없다.
        // https://qa.fmod.com/t/fmod-isplaying-question-please-help/11481
        // isPlaying이 INVALID_HANDLE을 반환할때 false와 동일하게 취급한다.
        _music.channel.isPlaying( out bool isPlaying );
        if ( !isPlaying ) yield break;

        // 같은 값일 때 계산 없이 종료.
        if ( Global.Math.Abs( _start - _end ) < float.Epsilon )
        {
            ErrorCheck( _music.channel.setVolume( _end ) );
            yield break;
        }

        float elapsedVolume = _start;
        float offset = _end - _start;
        ErrorCheck( _music.channel.setVolume( _start ) );
        while ( _start < _end ? elapsedVolume < _end : // FADEIN
                                elapsedVolume > _end ) // FADEOUT
        {
            // _start 초기화 후 다음 프레임 부터 증가.
            yield return YieldCache.WaitForEndOfFrame;
            elapsedVolume += ( offset * Time.deltaTime ) / _t;
            ErrorCheck( _music.channel.setVolume( elapsedVolume ) );
        }

        // 볼륨이 _end 보다 크기 때문에 ( 페이드인 기준 ) 프레임 넘어가기 전 _end 값으로 초기화.
        ErrorCheck( _music.channel.setVolume( _end ) );
        yield return YieldCache.WaitForEndOfFrame;
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
        ErrorCheck( groups[_type].isPlaying( out bool isPlay ) );
        
        return isPlay;
    }

    public void SetPitch( float _pitch, ChannelType _type )
    {
        ErrorCheck( groups[_type].setPitch( _pitch ) );
        UpdatePitchShift();
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
        if ( _type == ChannelType.BGM ) Volume = chlVolume;

        ErrorCheck( groups[_type].setVolume( chlVolume ) );
    }

    public void Stop( Music _music )
    {
        _music.channel.isPlaying( out bool isPlaying );
        if ( isPlaying ) ErrorCheck( _music.channel.stop() );
        ErrorCheck( _music.sound.release() );
        _music.sound.clearHandle();
    }

    public void AllStop()
    {
        foreach ( var group in groups )
        {
            if ( IsPlaying( group.Key ) )
                ErrorCheck( group.Value.stop() );
        }

        //MainChannel.isPlaying( out bool isPlaying );
        //if ( isPlaying ) ErrorCheck( MainChannel.stop() );
        //MainSound.release();
        //MainSound.clearHandle();
    }
    #endregion
    #region DSP
    private void PrintDSPDesc( FMOD.DSP_TYPE _type )
    {
        FMOD.DSP dsp;
        ErrorCheck( system.createDSPByType( FMOD.DSP_TYPE.FFT, out dsp ) );

        int num = 0;
        ErrorCheck( dsp.getNumParameters( out num ) );
        FMOD.DSP_PARAMETER_DESC[] descs = new FMOD.DSP_PARAMETER_DESC[num];
        for ( int i = 0; i < num; i++ )
        {
            ErrorCheck( dsp.getParameterInfo( i, out descs[i] ) );
            Debug.Log( $"Desc[{i}] : { System.Text.Encoding.Default.GetString( descs[i].name ) } { descs[i].type } { System.Text.Encoding.Default.GetString( descs[i].label ) } { descs[i].description }" );
        }

        dsp.release();
    }

    public bool GetDSP( FMOD.DSP_TYPE _type, out FMOD.DSP _dsp )
    {
        if ( !dsps.ContainsKey( _type ) )
        {
            Debug.LogError( "DSP is not loaded." );
            _dsp = new FMOD.DSP();
            return false;
        }

        _dsp = dsps[_type];
        return true;
    }
    
    public void AddDSP( FMOD.DSP_TYPE _dspType, ChannelType _channelType )
    {
        if ( !dsps.ContainsKey( _dspType ) )
             return;
            
        ErrorCheck( groups[_channelType].addDSP( FMOD.CHANNELCONTROL_DSP_INDEX.TAIL, dsps[_dspType] ) );
    }

    public void RemoveDSP( FMOD.DSP_TYPE _dspType, ChannelType _channelType )
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

    #region DSP Create
    private void CreateDPS()
    {
        //CreateMultiBandDSP();
        CreateFFTWindow();
        CreatePitchShiftDSP();
    }

    private void CreateFFTWindow()
    {
        if ( dsps.ContainsKey( FMOD.DSP_TYPE.FFT ) )
             return;

        FMOD.DSP dsp;
        ErrorCheck( system.createDSPByType( FMOD.DSP_TYPE.FFT, out dsp ) );
        ErrorCheck( dsp.setParameterInt( ( int )FMOD.DSP_FFT.WINDOWSIZE, 4096 ) );
        ErrorCheck( dsp.setParameterInt( ( int )FMOD.DSP_FFT.WINDOWTYPE, ( int )FMOD.DSP_FFT_WINDOW.BLACKMANHARRIS ) );
        dsps.Add( FMOD.DSP_TYPE.FFT, dsp );
    }

    private void CreatePitchShiftDSP()
    {
        if ( dsps.ContainsKey( FMOD.DSP_TYPE.PITCHSHIFT ) )
            return;

        FMOD.DSP dsp;
        ErrorCheck( system.createDSPByType( FMOD.DSP_TYPE.PITCHSHIFT, out dsp ) );
        ErrorCheck( dsp.setParameterFloat( ( int )FMOD.DSP_PITCHSHIFT.MAXCHANNELS, 2 ) );
        ErrorCheck( dsp.setParameterFloat( ( int )FMOD.DSP_PITCHSHIFT.FFTSIZE, 1024 ) );
        dsps.Add( FMOD.DSP_TYPE.PITCHSHIFT, dsp );
    }

    /// A ~ E  5 bands 
    /// 1. filter( int ) Default = FMOD_DSP_MULTIBAND_EQ_FILTER.LOWPASS_12DB
    /// 2. frequency( float ) Default = 8000, Range = 20 ~ 22000
    ///    대역이 있는 주파수
    /// 3. quality factor( float ) Default = 0.707, Range = 0.1 ~ 10
    ///    대역폭 품질
    ///    resonance (low/high pass), bandwidth (notch, peaking, band-pass), phase transition sharpness (all-pass), unused (low/high shelf)
    /// 4. gain( float ) Default = 0, Range = -30 ~ 30, Unit = Decibels( dB )
    ///    선택한 대역의 증폭, 감소
    ///    Boost or attenuation [high/low shelf and peaking only]
    private void CreateMultiBandDSP()
    {
        if ( dsps.ContainsKey( FMOD.DSP_TYPE.MULTIBAND_EQ ) )
             return;

        FMOD.DSP dsp;
        ErrorCheck( system.createDSPByType( FMOD.DSP_TYPE.MULTIBAND_EQ, out dsp ) );
        dsps.Add( FMOD.DSP_TYPE.MULTIBAND_EQ, dsp );

        ErrorCheck( dsp.setParameterInt( ( int )FMOD.DSP_MULTIBAND_EQ.A_FILTER, ( int )FMOD.DSP_MULTIBAND_EQ_FILTER_TYPE.LOWSHELF ) );
        ErrorCheck( dsp.setParameterInt( ( int )FMOD.DSP_MULTIBAND_EQ.B_FILTER, ( int )FMOD.DSP_MULTIBAND_EQ_FILTER_TYPE.LOWPASS_12DB ) );
        ErrorCheck( dsp.setParameterInt( ( int )FMOD.DSP_MULTIBAND_EQ.C_FILTER, ( int )FMOD.DSP_MULTIBAND_EQ_FILTER_TYPE.LOWPASS_12DB ) );
        ErrorCheck( dsp.setParameterInt( ( int )FMOD.DSP_MULTIBAND_EQ.D_FILTER, ( int )FMOD.DSP_MULTIBAND_EQ_FILTER_TYPE.LOWPASS_12DB ) );
        ErrorCheck( dsp.setParameterInt( ( int )FMOD.DSP_MULTIBAND_EQ.E_FILTER, ( int )FMOD.DSP_MULTIBAND_EQ_FILTER_TYPE.LOWPASS_12DB ) );

        ErrorCheck( dsp.setParameterFloat( ( int )FMOD.DSP_MULTIBAND_EQ.A_FREQUENCY, 320f ) );
        ErrorCheck( dsp.setParameterFloat( ( int )FMOD.DSP_MULTIBAND_EQ.B_FREQUENCY, 5000f ) );
        ErrorCheck( dsp.setParameterFloat( ( int )FMOD.DSP_MULTIBAND_EQ.C_FREQUENCY, 6000f ) );
        ErrorCheck( dsp.setParameterFloat( ( int )FMOD.DSP_MULTIBAND_EQ.D_FREQUENCY, 7000f ) );
        ErrorCheck( dsp.setParameterFloat( ( int )FMOD.DSP_MULTIBAND_EQ.E_FREQUENCY, 8000f ) );

        //ErrorCheck( dsp.setParameterFloat( ( int )FMOD.DSP_MULTIBAND_EQ.A_Q, .1f ) );
        ErrorCheck( dsp.setParameterFloat( ( int )FMOD.DSP_MULTIBAND_EQ.B_Q, .11f ) );
        ErrorCheck( dsp.setParameterFloat( ( int )FMOD.DSP_MULTIBAND_EQ.C_Q, .11f ) );
        ErrorCheck( dsp.setParameterFloat( ( int )FMOD.DSP_MULTIBAND_EQ.D_Q, .11f ) );
        ErrorCheck( dsp.setParameterFloat( ( int )FMOD.DSP_MULTIBAND_EQ.E_Q, .11f ) );
                    
        ErrorCheck( dsp.setParameterFloat( ( int )FMOD.DSP_MULTIBAND_EQ.A_GAIN, 10f ) );
        //ErrorCheck( dsp.setParameterFloat( ( int )FMOD.DSP_MULTIBAND_EQ.B_GAIN, 4f ) );
        //ErrorCheck( dsp.setParameterFloat( ( int )FMOD.DSP_MULTIBAND_EQ.C_GAIN, 4f ) );
        //ErrorCheck( dsp.setParameterFloat( ( int )FMOD.DSP_MULTIBAND_EQ.D_GAIN, 4f ) );
        //ErrorCheck( dsp.setParameterFloat( ( int )FMOD.DSP_MULTIBAND_EQ.E_GAIN, 4f ) );
    }
    #endregion
    
    #region DSP Update
    public void UpdatePitchShift()
    {
        if ( !dsps.ContainsKey( FMOD.DSP_TYPE.PITCHSHIFT ) )
            return;

        float offset = GameSetting.CurrentPitchType == PitchType.Normalize ? 1f    / GameSetting.CurrentPitch :
                       GameSetting.CurrentPitchType == PitchType.Nightcore ? 1.1f  / GameSetting.CurrentPitch : 1f;

        ErrorCheck( dsps[FMOD.DSP_TYPE.PITCHSHIFT].setParameterFloat( ( int )FMOD.DSP_PITCHSHIFT.PITCH, offset ) );
    }
    #endregion

    #endregion
    private bool ErrorCheck( FMOD.RESULT _result )
    {
        if ( FMOD.RESULT.OK != _result )
        {
            Debug.LogError( FMOD.Error.String( _result ) );
            return false;
        }

        return true;
    }
}
