using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundLoadType { Default, Stream }
public enum SoundPlayMode { Default, Loop }
public enum ChannelGroupType { Master, BGM, KeySound, Sfx, Count };
public enum SoundSfxType { Move, Return, Escape, Increase, Decrease }

public class SoundManager : SingletonUnity<SoundManager>
{
    #region variables
    private FMOD.System system;

    private readonly int maxChannelSize = 1000;
    private Dictionary<ChannelGroupType, FMOD.ChannelGroup> Groups = new Dictionary<ChannelGroupType, FMOD.ChannelGroup>();
    private FMOD.Channel bgmChannel, sfxChannel, keyChannel;

    private FMOD.Sound bgmSound;
    private Dictionary<SoundSfxType, FMOD.Sound> sfxSound = new Dictionary<SoundSfxType, FMOD.Sound>();

    public FMOD.DSP? FFT { get; private set; }
    private FMOD.DSP lowEffectEQ;

    public struct SoundDriver
    {
        public System.Guid guid;
        public int index;
        public string name;
        public int systemRate, speakModeChannels;
        public FMOD.SPEAKERMODE mode;
    }
    public List<SoundDriver> soundDrivers { get; private set; } = new List<SoundDriver>();
    public int CurrentDriverIndex { get { return currentDriverIndex; } }
    private int currentDriverIndex;
    private int numDriver;
    private uint version;

    public uint Length { get { return length; } }
    private uint length;

    public float Pitch { get; private set; } = 1f;
    public readonly float minPitch = .7f, maxPitch = 1.3f;

    private float volume;
    #endregion

    #region Unity Callback
    private void Awake()
    {
        // System
        ErrorCheck( FMOD.Factory.System_Create( out system ) );
        ErrorCheck( system.setOutput( FMOD.OUTPUTTYPE.AUTODETECT ) );

        // to do before system initialize
        int samplerRate, numRawSpeakers;
        FMOD.SPEAKERMODE mode;
        ErrorCheck( system.getSoftwareFormat( out samplerRate, out mode, out numRawSpeakers ) );
        ErrorCheck( system.setSoftwareFormat( samplerRate, FMOD.SPEAKERMODE.MONO, numRawSpeakers ) );

        ErrorCheck( system.getSoftwareFormat( out samplerRate, out mode, out numRawSpeakers ) );
        Debug.Log( $"SampleRate : {samplerRate} Mode : {mode} numRawSpeakers : {numRawSpeakers}" );

        ErrorCheck( system.setDSPBufferSize( 64, 4 ) );
        uint bufferSize;
        int numbuffers;
        ErrorCheck( system.getDSPBufferSize( out bufferSize, out numbuffers ) );
        Debug.Log( $"buffer size : {bufferSize} numbuffers : {numbuffers}" );

        System.IntPtr extraDriverData = new System.IntPtr();
        ErrorCheck( system.init( maxChannelSize, FMOD.INITFLAGS.NORMAL, extraDriverData ) );

        ErrorCheck( system.getVersion( out version ) );
        if ( version < FMOD.VERSION.number )
             Debug.LogError( "using the old version." );

        // Sound Driver
        ErrorCheck( system.getNumDrivers( out numDriver ) );
        for ( int i = 0; i < numDriver; i++ )
        {
            SoundDriver driver;
            if ( ErrorCheck( system.getDriverInfo( i, out driver.name, 256, out driver.guid, out driver.systemRate, out driver.mode, out driver.speakModeChannels ) ) )
            {
                driver.index = i;
                soundDrivers.Add( driver );
            }
        }
        ErrorCheck( system.getDriver( out currentDriverIndex ) );
        Debug.Log( $"Current Sound Device : {soundDrivers[currentDriverIndex].name}" );

        // ChannelGroup
        for ( int i = 0; i < ( int )ChannelGroupType.Count; i++ )
        {
            FMOD.ChannelGroup group;
            ChannelGroupType type = ( ChannelGroupType )i;

            ErrorCheck( system.createChannelGroup( type.ToString(), out group ) );
            if ( type != ChannelGroupType.Master )
                 ErrorCheck( Groups[ChannelGroupType.Master].addGroup( group ) );

            Groups.Add( type, group );
        }

        // Sfx Sound
        LoadSfx( SoundSfxType.Move,     "Assets/Sounds/Sfxs/confirm_style_2_001.wav" );
        LoadSfx( SoundSfxType.Return,   "Assets/Sounds/Sfxs/confirm_style_2_003.wav" );
        LoadSfx( SoundSfxType.Escape,   "Assets/Sounds/Sfxs/confirm_style_2_004.wav" );
        LoadSfx( SoundSfxType.Increase, "Assets/Sounds/Sfxs/confirm_style_2_005.wav" );
        LoadSfx( SoundSfxType.Decrease, "Assets/Sounds/Sfxs/confirm_style_2_006.wav" );

        // DSP
        CreateLowEffectDsp();

        // Details
        SetVolume( .1f, ChannelGroupType.Master );
        SetVolume( .1f, ChannelGroupType.BGM);
        Debug.Log( "SoundManager Initizlize Successful." );
    }

    private void Update() => system.update();

    private void OnApplicationQuit()
    {
        // Sound
        foreach ( var sfx in sfxSound.Values )
        {
            if ( sfx.hasHandle() )
            {
                ErrorCheck( sfx.release() );
            }
        }

        if ( bgmSound.hasHandle() )
        {
            ErrorCheck( bgmSound.release() );
        }

        // DSP
        RemoveFFT();

        ErrorCheck( Groups[ChannelGroupType.BGM].removeDSP( lowEffectEQ ) );
        ErrorCheck( lowEffectEQ.release() );

        // ChannelGroup
        for ( int i = 1; i < ( int )ChannelGroupType.Count; i++ )
        {
            ErrorCheck( Groups[( ChannelGroupType )i].release() );
        }
        ErrorCheck( Groups[ChannelGroupType.Master].release() );

        // System
        ErrorCheck( system.release() ); // 내부에서 close 함.
    }
    #endregion

    #region System
    public void SetDriver( int _index )
    {
        int curIndex;
        ErrorCheck( system.getDriver( out curIndex ) );

        if ( soundDrivers.Count <= _index || curIndex == _index )
        {
            return;
        }

        ErrorCheck( system.setDriver( _index ) );
        currentDriverIndex = _index;
    }
    #endregion

    #region Load
    public void LoadBgm( string _path, SoundLoadType _type = SoundLoadType.Default, SoundPlayMode _mode = SoundPlayMode.Default )
    {
        FMOD.MODE mode;
        switch ( _mode )
        {
            case SoundPlayMode.Default: { mode = FMOD.MODE.CREATESAMPLE | FMOD.MODE.ACCURATETIME; } break;
            case SoundPlayMode.Loop:    { mode = FMOD.MODE.LOOP_NORMAL | FMOD.MODE.ACCURATETIME; }  break;
            default:                    { mode = FMOD.MODE.CREATESAMPLE | FMOD.MODE.ACCURATETIME; } break;
        }

        FMOD.Sound sound;
        switch ( _type )
        {
            case SoundLoadType.Default: { ErrorCheck( system.createSound( _path, mode, out sound ) ); }  break;
            case SoundLoadType.Stream:  { ErrorCheck( system.createStream( _path, mode, out sound ) ); } break;
            default:                    { ErrorCheck( system.createSound( _path, mode, out sound ) ); }  break;
        }

        ErrorCheck( sound.getLength( out length, FMOD.TIMEUNIT.MS ) );

        if ( bgmSound.hasHandle() )
        {
            ErrorCheck( bgmSound.release() );
            bgmSound.clearHandle();
        }
        bgmSound = sound;
    }

    private void LoadSfx( SoundSfxType _type, string _path )
    {
        if ( sfxSound.ContainsKey( _type ) )
        {
            Debug.Log( $"sfxSound[{_type}] is duplicate loaded." );
            return;
        }

        FMOD.Sound sound;
        ErrorCheck( system.createSound( _path, FMOD.MODE.CREATESAMPLE, out sound ) );
        sfxSound.Add( _type, sound );
    }
    #endregion

    #region Sound
    public void PlaySfx( SoundSfxType _type )
    {
        if ( !sfxSound.ContainsKey( _type ) )
        {
            Debug.Log( $"sfxSound[{_type}] is not loaded." );
            return;
        }

        ErrorCheck( system.playSound( sfxSound[_type], Groups[ChannelGroupType.Sfx], false, out sfxChannel ) );
        ErrorCheck( sfxChannel.setPriority( 0 ) );
    }

    public void PlayBgm( bool _isPause = false )
    {
        if ( !bgmSound.hasHandle() )
        {
            Debug.Log( "Bgm is not loaded." );
            return;
        }

        Stop( ChannelGroupType.BGM );

        ErrorCheck( system.playSound( bgmSound, Groups[ChannelGroupType.BGM], _isPause, out bgmChannel ) );
    }

    public void PauseBgm( bool _isPause )
    {
        if ( !bgmSound.hasHandle() || !IsPlaying( ChannelGroupType.BGM ) )
        {
            Debug.Log( "bgm is not loaded or is not Playing." );
            return;
        }

        ErrorCheck( bgmChannel.setPaused( _isPause ) );
    }

    public void SetPosition( uint _pos )
    {
        if ( !IsPlaying( ChannelGroupType.BGM ) )
        {
            Debug.Log( "bgm is not playing" );
            return;
        }

        ErrorCheck( bgmChannel.setPosition( _pos, FMOD.TIMEUNIT.MS ) );
    }

    public uint GetPosition()
    {
        if ( !IsPlaying( ChannelGroupType.BGM ) )
        {
            Debug.Log( "bgm is not playing" );
            return 0;
        }

        uint pos;
        ErrorCheck( bgmChannel.getPosition( out pos, FMOD.TIMEUNIT.MS ) );
        return pos;
    }

    public void SetPitch( float _value )
    {
        if ( !IsPlaying( ChannelGroupType.BGM ) )
        {
            Debug.Log( "bgm is not playing" );
            return;
        }

        int value = Mathf.RoundToInt( _value * 10f );
        if ( value < ( minPitch * 10 ) || 
             value > ( maxPitch * 10 ) )
        {
            Debug.Log( $"pitch range {minPitch} ~ {maxPitch}, param : {_value}" );
            return;
        }

        ErrorCheck( bgmChannel.setPitch( _value ) );
        Pitch = _value;
    }

    public void Stop( ChannelGroupType _type )
    {
        if ( !Groups.ContainsKey( _type ) )
        {
            Debug.Log( $"The channel group key could not be found. : {_type}" );
            return;
        }

        ErrorCheck( Groups[_type].stop() );
    }

    public void AllStop()
    {
        foreach( var group in Groups )
        {
            if ( IsPlaying( group.Key ) ) 
                 Stop( group.Key );
        }
    }
    #endregion

    #region ChannelGroup
    public bool IsPlaying( ChannelGroupType _type = ChannelGroupType.Master )
    {
        if ( !Groups.ContainsKey( _type ) )
        {
            Debug.Log( $"The channel group key could not be found. : {_type}" );
            return false;
        }

        bool isPlay = false;
        ErrorCheck( Groups[_type].isPlaying( out isPlay ) );
        
        return isPlay;
    }

    public float GetVolume( ChannelGroupType _type = ChannelGroupType.Master )
    {
        if ( !Groups.ContainsKey( _type ) )
        {
            Debug.Log( $"The channel group key could not be found. : {_type}" );
            return -1f;
        }

        float volume = 0f;
        ErrorCheck( Groups[_type].getVolume( out volume ) );

        return volume;
    }

    public void SetVolume( float _value, ChannelGroupType _type = ChannelGroupType.Master )
    {
        if ( !Groups.ContainsKey( _type ) )
        {
            Debug.Log( $"The channel group key could not be found. : {_type}" );
            return;
        }

        volume = _value;
        if ( _value < 0f ) volume = 0f;
        if ( _value > 1f ) volume = 1f;

        ErrorCheck( Groups[_type].setVolume( volume ) );
    }
    #endregion

    #region DSP
    public void AddFFT( int _size, FMOD.DSP_FFT_WINDOW _type, out FMOD.DSP _dsp )
    {
        if ( FFT != null ) RemoveFFT();

        ErrorCheck( system.createDSPByType( FMOD.DSP_TYPE.FFT, out _dsp ) );
        ErrorCheck( _dsp.setParameterInt( ( int )FMOD.DSP_FFT.WINDOWSIZE, _size ) );
        ErrorCheck( _dsp.setParameterInt( ( int )FMOD.DSP_FFT.WINDOWTYPE, ( int )_type ) );
        ErrorCheck( Groups[ChannelGroupType.Master].addDSP( FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, _dsp ) );
        FFT = _dsp;
    }

    public void RemoveFFT()
    {
        if ( FFT != null )
        {
            ErrorCheck( Groups[ChannelGroupType.Master].removeDSP( FFT.Value ) );
            ErrorCheck( FFT.Value.release() );
            FFT = null;
        }
    }

    /// <summary>
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
    /// </summary>
    private void CreateLowEffectDsp()
    {
        ErrorCheck( system.createDSPByType( FMOD.DSP_TYPE.MULTIBAND_EQ, out lowEffectEQ ) );

        // multiband 구조체 정보 확인
        // int numParameters = 0;
        // ErrorCheck( multibandEQ.getNumParameters( out numParameters ) );
        // FMOD.DSP_PARAMETER_DESC[] descs = new FMOD.DSP_PARAMETER_DESC[numParameters];
        // for ( int i = 0; i < numParameters; i++ )
        // {
        //     ErrorCheck( multibandEQ.getParameterInfo( i, out descs[i] ) );
        //     Debug.Log( $"Desc[{i}] Name        : { System.Text.Encoding.Default.GetString( descs[i].name ) }" );
        //     Debug.Log( $"Desc[{i}] Label       : { System.Text.Encoding.Default.GetString( descs[i].label ) }" );
        //     Debug.Log( $"Desc[{i}] Description : { descs[i].description }" );
        //     Debug.Log( $"Desc[{i}] Type        : { descs[i].type }" );
        // }

        ErrorCheck( lowEffectEQ.setParameterInt( ( int )FMOD.DSP_MULTIBAND_EQ.A_FILTER, ( int )FMOD.DSP_MULTIBAND_EQ_FILTER_TYPE.LOWPASS_48DB ) );
        ErrorCheck( lowEffectEQ.setParameterInt( ( int )FMOD.DSP_MULTIBAND_EQ.B_FILTER, ( int )FMOD.DSP_MULTIBAND_EQ_FILTER_TYPE.LOWPASS_48DB ) );
        ErrorCheck( lowEffectEQ.setParameterInt( ( int )FMOD.DSP_MULTIBAND_EQ.C_FILTER, ( int )FMOD.DSP_MULTIBAND_EQ_FILTER_TYPE.LOWPASS_48DB ) );
        ErrorCheck( lowEffectEQ.setParameterInt( ( int )FMOD.DSP_MULTIBAND_EQ.D_FILTER, ( int )FMOD.DSP_MULTIBAND_EQ_FILTER_TYPE.LOWPASS_48DB ) );
        ErrorCheck( lowEffectEQ.setParameterInt( ( int )FMOD.DSP_MULTIBAND_EQ.E_FILTER, ( int )FMOD.DSP_MULTIBAND_EQ_FILTER_TYPE.LOWPASS_48DB ) );

        ErrorCheck( lowEffectEQ.setParameterFloat( ( int )FMOD.DSP_MULTIBAND_EQ.A_Q, .1f ) );
        ErrorCheck( lowEffectEQ.setParameterFloat( ( int )FMOD.DSP_MULTIBAND_EQ.B_Q, .1f ) );
        ErrorCheck( lowEffectEQ.setParameterFloat( ( int )FMOD.DSP_MULTIBAND_EQ.C_Q, .1f ) );
        ErrorCheck( lowEffectEQ.setParameterFloat( ( int )FMOD.DSP_MULTIBAND_EQ.D_Q, .1f ) );
        ErrorCheck( lowEffectEQ.setParameterFloat( ( int )FMOD.DSP_MULTIBAND_EQ.E_Q, .1f ) );

        ErrorCheck( lowEffectEQ.setParameterFloat( ( int )FMOD.DSP_MULTIBAND_EQ.A_GAIN, ( float )FMOD.DSP_MULTIBAND_EQ_FILTER_TYPE.LOWSHELF ) );
        ErrorCheck( lowEffectEQ.setParameterFloat( ( int )FMOD.DSP_MULTIBAND_EQ.B_GAIN, ( float )FMOD.DSP_MULTIBAND_EQ_FILTER_TYPE.LOWSHELF ) );
        ErrorCheck( lowEffectEQ.setParameterFloat( ( int )FMOD.DSP_MULTIBAND_EQ.C_GAIN, ( float )FMOD.DSP_MULTIBAND_EQ_FILTER_TYPE.LOWSHELF ) );
        ErrorCheck( lowEffectEQ.setParameterFloat( ( int )FMOD.DSP_MULTIBAND_EQ.D_GAIN, ( float )FMOD.DSP_MULTIBAND_EQ_FILTER_TYPE.LOWSHELF ) );
        ErrorCheck( lowEffectEQ.setParameterFloat( ( int )FMOD.DSP_MULTIBAND_EQ.E_GAIN, ( float )FMOD.DSP_MULTIBAND_EQ_FILTER_TYPE.LOWSHELF ) );
    }

    public void UseLowEqualizer( bool _isUse )
    {
        var bgmGroup = Groups[ChannelGroupType.BGM];
        int numDsp;
        ErrorCheck( bgmGroup.getNumDSPs( out numDsp ) );
        for ( int i = 0; i < numDsp; i++ )
        {
            FMOD.DSP dsp;
            ErrorCheck( bgmGroup.getDSP( i, out dsp ) );

            bool isEquals = Equals( dsp, lowEffectEQ );
            if ( isEquals && _isUse == true ) // 이미 적용된 상태
            {
                return;
            }
            else if ( isEquals && _isUse == false )
            {
                ErrorCheck( bgmGroup.removeDSP( lowEffectEQ ) );
                return;
            }
        }

        // 적용된 dsp가 없어서 추가함.
        if ( _isUse == true )
             ErrorCheck( bgmGroup.addDSP( FMOD.CHANNELCONTROL_DSP_INDEX.TAIL, lowEffectEQ ) );
    }
    #endregion

    private bool ErrorCheck( FMOD.RESULT _res )
    {
        if ( FMOD.RESULT.OK != _res )
        {
            Debug.LogError( FMOD.Error.String( _res ) );
            return false;
        }

        return true;
    }
}
