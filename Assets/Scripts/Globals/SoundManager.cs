using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using DG.Tweening;

public enum SoundBuffer { _64, _128, _256, _512, _1024, Count, }
public enum SoundSfxType 
{ 
    MainSelect, MainClick, MainHover, Slider,
    MenuSelect, MenuClick, MenuHover
}


public enum ChannelType : byte { Master, BGM, KeySound, SFX, Count, };
public class SoundManager : Singleton<SoundManager>
{
    #region variables
    private static readonly int MaxSoftwareChannel = 128;
    private static readonly int MaxVirtualChannel  = 1000;
    private Dictionary<ChannelType, FMOD.ChannelGroup>   groups    = new Dictionary<ChannelType, FMOD.ChannelGroup>();
    private Dictionary<SoundSfxType, FMOD.Sound>         sfxSounds = new Dictionary<SoundSfxType, FMOD.Sound>();
    private Dictionary<string/* Ű�� �̸� */, FMOD.Sound> keySounds = new Dictionary<string, FMOD.Sound>();
    private Dictionary<FMOD.DSP_TYPE, FMOD.DSP>          dsps      = new Dictionary<FMOD.DSP_TYPE, FMOD.DSP>();
    private FMOD.System system;
    private FMOD.Sound bgmSound;
    private FMOD.Channel bgmChannel;
    private Tweener volumeTweener;
    private int curDriverIndex;
    private bool hasAccurateFlag;
    public event Action OnReLoad, OnRelease;
    public struct SoundDriver : IEquatable<SoundDriver>
    #region SoundDriver Body
    {
        public Guid guid;
        public int index;
        public string name;
        public int systemRate, speakModeChannels;
        public FMOD.SPEAKERMODE mode;

        public bool Equals( SoundDriver _other ) => index == _other.index;
        public override bool Equals( object _obj ) => Equals( ( SoundDriver )_obj );
        public override int GetHashCode() => base.GetHashCode();
    }
    #endregion
    public ReadOnlyCollection<SoundDriver> Drivers { get; private set; } 
    /// <summary>
    /// AccurateTime flag is required.
    /// </summary>
    public uint Length {
        get {
            //if ( !hasAccurateFlag || !bgmSound.hasHandle() ) {
            //    Debug.LogWarning( $"No AccurateTime flag or BGM Sound." );
            //    return uint.MaxValue;
            //}
            
            uint length;
            ErrorCheck( bgmSound.getLength( out length, FMOD.TIMEUNIT.MS ) );
            return length;
        }
    }
    #region Properties
    public int CurrentDriverIndex 
    {
        get => curDriverIndex;
        set
        {
            int curIndex;
            ErrorCheck( system.getDriver( out curIndex ) );

            if ( Drivers.Count <= value || curIndex == value )
            {
                Debug.LogWarning( "SoundDriver Index is Out of Range or Duplicated Value" );
                return;
            }

            ErrorCheck( system.setDriver( value ) );
            curDriverIndex = value;
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
            ErrorCheck( bgmChannel.getPosition( out pos, FMOD.TIMEUNIT.MS ) );
            return pos;
        }

        set
        {
            if ( !IsPlaying( ChannelType.BGM ) )
            {
                Debug.LogError( "bgm is not playing" );
                return;
            }
            
            ErrorCheck( bgmChannel.setPosition( value, FMOD.TIMEUNIT.MS ) );
        }
    }
    public int UseChannelCount
    {
        get
        {
            int channels = 0;
            ErrorCheck( system.getChannelsPlaying( out channels ) );
            return channels;
        }
    }
    public bool IsLoad { get; private set; }
    public float volume { get; private set; }
    #endregion
    #endregion
    #region System
    public void Initialize()
    {
        // System
        ErrorCheck( FMOD.Factory.System_Create( out system ) );
        ErrorCheck( system.setOutput( FMOD.OUTPUTTYPE.AUTODETECT ) );
        
        // To do Before System Initialize
        int sampleRate, numRawSpeakers;
        FMOD.SPEAKERMODE mode;
        ErrorCheck( system.getSoftwareFormat( out sampleRate, out mode, out numRawSpeakers ) );
        ErrorCheck( system.setSoftwareFormat( sampleRate, FMOD.SPEAKERMODE.STEREO, numRawSpeakers ) );
        ErrorCheck( system.setSoftwareChannels( MaxSoftwareChannel ) );
        ErrorCheck( system.setDSPBufferSize( uint.Parse( SystemSetting.CurrentSoundBufferString ), 4 ) );

        // System Initialize
        IntPtr extraDriverData = new IntPtr();
        ErrorCheck( system.init( MaxVirtualChannel, FMOD.INITFLAGS.NORMAL, extraDriverData ) );
        uint version;
        ErrorCheck( system.getVersion( out version ) );
        if ( version < FMOD.VERSION.number )
             Debug.LogError( "using the old version." );

        // Sound Driver
        int numDriver;
        ErrorCheck( system.getNumDrivers( out numDriver ) );
        List<SoundDriver> drivers = new List<SoundDriver>();
        for ( int i = 0; i < numDriver; i++ )
        {
            SoundDriver driver;
            if ( ErrorCheck( system.getDriverInfo( i, out driver.name, 256, out driver.guid, out driver.systemRate, out driver.mode, out driver.speakModeChannels ) ) )
            {
                driver.index = i;
                drivers.Add( driver );
            }
        }
        Drivers = new ReadOnlyCollection<SoundDriver>( drivers );

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

        #region Details
        // DSP
        CreateDPS();

        // Sfx Sound
        LoadSfx( SoundSfxType.MainClick, @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\MainClick.wav" );
        LoadSfx( SoundSfxType.MenuClick, @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\MenuClick.wav" );
        
        LoadSfx( SoundSfxType.MainSelect, @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\MainSelect.wav" );
        LoadSfx( SoundSfxType.MenuSelect, @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\MenuSelect.wav" );

        LoadSfx( SoundSfxType.MainHover, @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\MainHover.wav" );
        LoadSfx( SoundSfxType.MenuHover, @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\MenuHover.wav" );

        LoadSfx( SoundSfxType.Slider, @$"{Application.streamingAssetsPath}\\Default\\Sounds\\Sfx\\Slider.wav" );

        // Logs
        //ErrorCheck( system.getSoftwareFormat( out sampleRate, out mode, out numRawSpeakers ) );
        //Debug.Log( $"SampleRate : {sampleRate} Mode : {mode} numRawSpeakers : {numRawSpeakers}" );
        //int softwareChannels;
        //ErrorCheck( system.getSoftwareChannels( out softwareChannels ) );
        //Debug.Log( $"SoftwareChannel {softwareChannels}" );
        //int numbuffers;
        //ErrorCheck( system.getDSPBufferSize( out bufferSize, out numbuffers ) );
        //Debug.Log( $"buffer size : {bufferSize} numbuffers : {numbuffers}" );
        //ErrorCheck( system.getDriver( out curDriverIndex ) );
        //Debug.Log( $"Current Sound Device : {SoundDrivers[curDriverIndex].name}" );

        // Details
        SetVolume( 1f, ChannelType.Master );
        SetVolume( .1f, ChannelType.BGM );
        SetVolume( .1f, ChannelType.KeySound );
        SetVolume( .3f, ChannelType.SFX );
        #endregion
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

        if ( bgmSound.hasHandle() )
        {
            ErrorCheck( bgmSound.release() );
            bgmSound.clearHandle();
        }

        // DSP
        OnRelease?.Invoke();
        AllRemoveDSP();

        foreach ( var dsp in dsps.Values )
        {
            ErrorCheck( dsp.release() );
            dsp.clearHandle();
        }
        dsps.Clear();

        // ChannelGroup
        for ( int i = 1; i < ( int )ChannelType.Count; i++ )
        {
            ErrorCheck( groups[( ChannelType )i].release() );
        }
        ErrorCheck( groups[ChannelType.Master].release() );
        groups.Clear();

        // System
        if ( system.hasHandle() )
        {
            ErrorCheck( system.release() ); // ���ο��� close ��.
            system.clearHandle();
        }

        Debug.Log( "SoundManager Release" );
    }

    public void ReLoad()
    {
        AllStop();
        IsLoad = true;

        int prevDriverIndex;
        ErrorCheck( system.getDriver( out prevDriverIndex ) );

        Release();
        Initialize();

        OnReLoad?.Invoke();
        ErrorCheck( system.setDriver( prevDriverIndex ) );
        curDriverIndex = prevDriverIndex;

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
        // �Ŵ����� Ŭ������ ���� �������� ���ŵǾ�� �Ѵ�.
        // OnApplicationQuit -> OnDisable -> OnDestroy ������ ȣ�� �Ǳ� ������
        // Ÿ Ŭ�������� OnDisable, OnApplicationQuit�� ���� ���� ó���� ��ģ ��
        // SoundManager OnDestroy�� ����� �� �ֵ��� �Ѵ�.
        Release();
    }
    #endregion
    #region Load
    public void LoadBgm( string _path, bool _isLoop, bool _isStream, bool _hasAccurateTime )
    {
        hasAccurateFlag = _hasAccurateTime;

        FMOD.MODE mode = FMOD.MODE.CREATESAMPLE;
        mode = _hasAccurateTime ? mode |= FMOD.MODE.ACCURATETIME : mode;
        mode = _isLoop          ? mode |= FMOD.MODE.LOOP_NORMAL  : mode |= FMOD.MODE.LOOP_OFF;
        mode = _isStream        ? mode |= ( mode &= ~FMOD.MODE.CREATESAMPLE ) | FMOD.MODE.CREATESTREAM | FMOD.MODE.LOWMEM : mode;

        FMOD.Sound sound;
        ErrorCheck( system.createSound( _path, mode, out sound ) );

        if ( bgmSound.hasHandle() )
        {
            ErrorCheck( bgmSound.release() );
            bgmSound.clearHandle();
        }
        bgmSound = sound;
    }

    private void LoadSfx( SoundSfxType _type, string _path )
    {
        if ( sfxSounds.ContainsKey( _type ) )
        {
            Debug.LogError( $"sfxSound[{_type}] is duplicate loaded." );
            return;
        }

        FMOD.Sound sound;
        ErrorCheck( system.createSound( _path, FMOD.MODE.LOOP_OFF | FMOD.MODE.CREATESAMPLE, out sound ) );
        sfxSounds.Add( _type, sound );
    }

    public bool LoadKeySound( string _path, out FMOD.Sound _sound )
    {
        var name = System.IO.Path.GetFileName( _path );
        if ( keySounds.ContainsKey( name ) )
        {
            ++TotalKeySoundCount;
            _sound = keySounds[name];
        }
        else if ( System.IO.File.Exists( @_path ) )
        {
            ErrorCheck( system.createSound( _path, FMOD.MODE.LOOP_OFF | FMOD.MODE.CREATESAMPLE, out _sound ) );
            keySounds.Add( name, _sound );
            ++TotalKeySoundCount;
        }
        //   if ( !System.IO.File.Exists( @_path ) )
        else
        {
            // throw new Exception( $"File Exists  {_path}" );
            _sound = new FMOD.Sound();
            return false;
        }

        return true;
    }
    #endregion
    #region Play

    /// <summary> Play Background Music </summary>
    public void Play( bool _isPause )
    {
        if ( !bgmSound.hasHandle() )
        {
            Debug.LogError( "Bgm is not loaded." );
            return;
        }

        //Stop( ChannelType.BGM );
        //ErrorCheck( groups[ChannelType.BGM].setPitch( _pitch ) );

        SetPaused( _isPause, ChannelType.BGM );
        ErrorCheck( system.playSound( bgmSound, groups[ChannelType.BGM], false, out bgmChannel ) );
    }

    /// <summary> Play Sound Special Effects </summary>
    public void Play( SoundSfxType _type )
    {
        if ( !sfxSounds.ContainsKey( _type ) )
        {
            Debug.LogError( $"sfxSound[{_type}] is not loaded." );
            return;
        }

        FMOD.Channel channel;
        ErrorCheck( system.playSound( sfxSounds[_type], groups[ChannelType.SFX], false, out channel ) );
    }

    /// <summary> Play Key Sound Effects </summary>
    public void Play( KeySound _keySound )
    {
        if ( !_keySound.hasSound ) return;

        if ( !_keySound.sound.hasHandle() )
        {
            //Debug.LogWarning( $"keySound[{_keySound.name}] is not loaded." );
            return;
        }

        //ErrorCheck( groups[ChannelType.KeySound].setPitch( _pitch ) );
        
        FMOD.Channel channel;
        ErrorCheck( system.playSound( _keySound.sound, groups[ChannelType.KeySound], false, out channel ) );
        ErrorCheck( channel.setVolume( _keySound.volume ) );
    }
    #endregion
    #region Effect
    public void FadeIn( float _duration )
    {
        volumeTweener?.Kill();
        ErrorCheck( groups[ChannelType.BGM].setVolume( 0f ) );
        volumeTweener = DOTween.To( () => 0f, x => ErrorCheck( groups[ChannelType.BGM].setVolume( x ) ), volume, _duration );
    }
    
    public void FadeIn( float _startValue, float _duration )
    {
        volumeTweener?.Kill();
        ErrorCheck( groups[ChannelType.BGM].setVolume( 0f ) );
        volumeTweener = DOTween.To( () => _startValue, x => ErrorCheck( groups[ChannelType.BGM].setVolume( x ) ), volume, _duration );
    }

    public void FadeIn( float _duration, Action _callback )
    {
        volumeTweener?.Kill();
        ErrorCheck( groups[ChannelType.BGM].setVolume( 0f ) );
        volumeTweener = DOTween.To( () => 0f, x => ErrorCheck( groups[ChannelType.BGM].setVolume( x ) ), volume, _duration ).OnComplete( () => { _callback.Invoke(); } );
    }

    public void FadeOut( float _duration )
    {
        volumeTweener?.Kill();
        volumeTweener = DOTween.To( () => volume, x => ErrorCheck( groups[ChannelType.BGM].setVolume( x ) ), 0f, _duration );
    }

    public void FadeOut( float _endValue, float _duration )
    {
        volumeTweener?.Kill();
        volumeTweener = DOTween.To( () => volume, x => ErrorCheck( groups[ChannelType.BGM].setVolume( x ) ), _endValue, _duration );
    }

    public void FadeOut( float _duration, Action _callback )
    {
        volumeTweener?.Kill();
        volumeTweener = DOTween.To( () => volume, x => ErrorCheck( groups[ChannelType.BGM].setVolume( x ) ), 0f, _duration ).OnComplete( () => { _callback.Invoke(); } );
    }

    #endregion
    #region ChannelGroup
    public bool IsPlaying( ChannelType _type )
    {
        bool isPlay = false;
        ErrorCheck( groups[_type].isPlaying( out isPlay ) );
        
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
        float chlVolume = 0f;
        ErrorCheck( groups[_type].getVolume( out chlVolume ) );

        return chlVolume;
    }

    public void SetVolume( float _value, ChannelType _type )
    {
        float chlVolume = _value;
        if ( _value < 0f ) chlVolume = 0f;
        if ( _value > 1f ) chlVolume = 1f;
        if ( _type == ChannelType.BGM ) volume = chlVolume;

        ErrorCheck( groups[_type].setVolume( chlVolume ) );
    }

    public void Stop( ChannelType _type ) => ErrorCheck( groups[_type].stop() );

    public void AllStop()
    {
        foreach ( var group in groups )
        {
            if ( IsPlaying( group.Key ) )
                ErrorCheck( group.Value.stop() );
        }
    }
    #endregion
    #region DSP

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

    public void AddDSP( in FMOD.DSP _dsp, ChannelType _type )
    {
        if ( !_dsp.hasHandle() )
            return;

        ErrorCheck( groups[_type].addDSP( FMOD.CHANNELCONTROL_DSP_INDEX.TAIL, _dsp ) );
    }

    public void RemoveDSP( FMOD.DSP _dsp, ChannelType _type )
    {
        if ( !_dsp.hasHandle() )
             return;

        ErrorCheck( groups[_type].removeDSP( _dsp ) );
    }

    public void AllRemoveDSP()
    {
        for ( int i = 0; i < ( int )ChannelType.Count; i++ )
        {
            int numDSP = 0;
            ErrorCheck( groups[( ChannelType )i].getNumDSPs( out numDSP ) );
            for ( int j = 1; j < numDSP; j++ )
            {
                FMOD.DSP dsp;
                ErrorCheck( groups[( ChannelType )i].getDSP( j, out dsp ) );
                ErrorCheck( groups[( ChannelType )i].removeDSP( dsp ) );
            }
        }
    }

    public void PrintDSPCount()
    {
        for ( int i = 0; i < ( int )ChannelType.Count; i++ )
        {
            int numDSP = 0;
            ErrorCheck( groups[( ChannelType )i].getNumDSPs( out numDSP ) );

            Debug.Log( $"{( ChannelType )i} DSP : {numDSP}" );
        }
    }

    public void DeleteDSP( ref FMOD.DSP _dsp )
    {
        for ( int i = 0; i < ( int )ChannelType.Count; i++ )
        {
            groups[( ChannelType )i].removeDSP( _dsp );
        }

        ErrorCheck( _dsp.release() );
        _dsp.clearHandle();
    }

    #region DSP Create
    public void CreateDPS()
    {
        CreateMultiBandDSP();
        CreatePitchShiftDSP();
        CreateFFTWindow();
    }

    public void CreateFFTWindow()
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
        ErrorCheck( dsp.setParameterFloat( ( int )FMOD.DSP_PITCHSHIFT.MAXCHANNELS, 0 ) );
        ErrorCheck( dsp.setParameterFloat( ( int )FMOD.DSP_PITCHSHIFT.FFTSIZE, 1024 ) );

        dsps.Add( FMOD.DSP_TYPE.PITCHSHIFT, dsp );
    }

    /// A ~ E  5 bands 
    /// 1. filter( int ) Default = FMOD_DSP_MULTIBAND_EQ_FILTER.LOWPASS_12DB
    /// 2. frequency( float ) Default = 8000, Range = 20 ~ 22000
    ///    �뿪�� �ִ� ���ļ�
    /// 3. quality factor( float ) Default = 0.707, Range = 0.1 ~ 10
    ///    �뿪�� ǰ��
    ///    resonance (low/high pass), bandwidth (notch, peaking, band-pass), phase transition sharpness (all-pass), unused (low/high shelf)
    /// 4. gain( float ) Default = 0, Range = -30 ~ 30, Unit = Decibels( dB )
    ///    ������ �뿪�� ����, ����
    ///    Boost or attenuation [high/low shelf and peaking only]
    private void CreateMultiBandDSP()
    {
        if ( dsps.ContainsKey( FMOD.DSP_TYPE.MULTIBAND_EQ ) )
             return;

        FMOD.DSP dsp;
        ErrorCheck( system.createDSPByType( FMOD.DSP_TYPE.MULTIBAND_EQ, out dsp ) );
        dsps.Add( FMOD.DSP_TYPE.MULTIBAND_EQ, dsp );

        // multiband ����ü ���� Ȯ��
        //int numParameters = 0;
        //ErrorCheck( Multiband.getNumParameters( out numParameters ) );
        //FMOD.DSP_PARAMETER_DESC[] descs = new FMOD.DSP_PARAMETER_DESC[numParameters];
        //for ( int i = 0; i < numParameters; i++ )
        //{
        //    ErrorCheck( Multiband.getParameterInfo( i, out descs[i] ) );
        //    Debug.Log( $"Desc[{i}] Name        : { System.Text.Encoding.Default.GetString( descs[i].name ) }" );
        //    Debug.Log( $"Desc[{i}] Label       : { System.Text.Encoding.Default.GetString( descs[i].label ) }" );
        //    Debug.Log( $"Desc[{i}] Description : { descs[i].description }" );
        //    Debug.Log( $"Desc[{i}] Type        : { descs[i].type }" );
        //}

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
                       GameSetting.CurrentPitchType == PitchType.Nightcore ? 1.15f / GameSetting.CurrentPitch : 1f;

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
