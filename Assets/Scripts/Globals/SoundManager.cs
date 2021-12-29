using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SOUND_LOAD_TYPE { DEFAULT, STREAM }
public enum SOUND_PLAY_MODE { DEFAULT, LOOP }
public enum SOUND_GROUP_TYPE { MASTER, SFX, INTERFACE, BACKGROUND };

public class SoundManager : SingletonUnity<SoundManager>
{
    #region variables
    private FMOD.System       system;
    private FMOD.ChannelGroup masterGroup, sfxGroup, interfaceGroup, backgroundGroup;
    private FMOD.Channel      bgmChannel;
    private FMOD.Channel[]    interfaceChannels = new FMOD.Channel[100];
    private FMOD.Sound?       bgmSound, move, move2;
    //private FMOD.Channel[]   sfxChannels     = new FMOD.Channel[sfxChannelCount];
    //private const int        sfxChannelCount = 100;

    private readonly int maxChannelSize = 100;

    public  FMOD.DSP? FFT { get; private set; }
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
    private int driverCount, currentDriverIndex;
    private uint version;

    public uint? Length { get; private set; }
    public float Pitch  { get; private set; } = 1f;

    private float volume;
    #endregion

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

    public void AddFFT( int _size, FMOD.DSP_FFT_WINDOW _type, out FMOD.DSP _dsp )
    {
        if ( FFT != null ) RemoveFFT();

        ErrorCheck( system.createDSPByType( FMOD.DSP_TYPE.FFT, out _dsp ) );
        ErrorCheck( _dsp.setParameterInt( ( int )FMOD.DSP_FFT.WINDOWSIZE, _size ) );
        ErrorCheck( _dsp.setParameterInt( ( int )FMOD.DSP_FFT.WINDOWTYPE, ( int )_type ) );
        ErrorCheck( masterGroup.addDSP( FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, _dsp ) );
        FFT = _dsp;
    }

    public void RemoveFFT()
    {
        if ( FFT != null )
        {
            ErrorCheck( masterGroup.removeDSP( FFT.Value ) );
            ErrorCheck( FFT.Value.release() );
            FFT = null;
        }
    }

    //public void Update() => system.update();
    private void Update() => system.update();

    //public void Initialize()
    private void Awake()
    {
        // System Init
        ErrorCheck( FMOD.Factory.System_Create( out system ) );
        ErrorCheck( system.setOutput( FMOD.OUTPUTTYPE.AUTODETECT ) );

        // to do before system initialize
        //ErrorCheck( system.setSoftwareFormat( 48000, FMOD.SPEAKERMODE.MONO, 4 ) );
        //ErrorCheck( system.setDSPBufferSize( 1024, 4 ) );

        System.IntPtr extraDriverData = new System.IntPtr();
        ErrorCheck( system.init( maxChannelSize, FMOD.INITFLAGS.NORMAL, extraDriverData ) );
        
        ErrorCheck( system.getVersion( out version ) );
        if ( version < FMOD.VERSION.number )
             Debug.LogError( "using the old version." );

        // Sound Drivers
        ErrorCheck( system.getNumDrivers( out driverCount ) );
        for ( int i = 0; i < driverCount; i++ )
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

        // Channel Initialize
        ErrorCheck( system.createChannelGroup( "MasterGroup", out masterGroup ) );
        ErrorCheck( system.createChannelGroup( "SfxGroup", out sfxGroup ) );
        ErrorCheck( system.createChannelGroup( "InterfaceGroup", out interfaceGroup ) );
        ErrorCheck( system.createChannelGroup( "BackgroundGroup", out backgroundGroup ) );

        ErrorCheck( masterGroup.addGroup( sfxGroup ) );
        ErrorCheck( masterGroup.addGroup( interfaceGroup ) );
        ErrorCheck( masterGroup.addGroup( backgroundGroup ) );

        // DSP Setting
        CreateLowEffectDsp();

        // Details
        SetVolume( .1f );
        Debug.Log( "SoundManager Initizlize Successful." );
    }


    //public void Release()
    private void OnApplicationQuit()
    {
        // 생성한 역순으로 release
        if ( bgmSound != null )
        {
            ErrorCheck( bgmSound.Value.release() );
            bgmSound = null;
        }

        RemoveFFT();

        ErrorCheck( backgroundGroup.removeDSP( lowEffectEQ ) );
        ErrorCheck( lowEffectEQ.release() );

        ErrorCheck( backgroundGroup.release() );
        ErrorCheck( interfaceGroup.release() );
        ErrorCheck( sfxGroup.release() );
        ErrorCheck( masterGroup.release() );

        ErrorCheck( system.release() ); // 내부에서 close 함.
    }

    #region customize functions

    public void InterfaceSfxLoad( string _path, int _index )
    {
        FMOD.Sound sound;
        ErrorCheck( system.createSound( _path, FMOD.MODE.CREATESAMPLE | FMOD.MODE.ACCURATETIME, out sound ) );

        if ( _index == 0 )
            move = sound;
        else if ( _index == 1 )
            move2 = sound;
    }

    public void PlayInterfaceSfx( int _index )
    {
        if ( _index == 0 )
            ErrorCheck( system.playSound( move.Value, interfaceGroup, false, out interfaceChannels[0] ) );
        else if ( _index == 1 )
            ErrorCheck( system.playSound( move2.Value, interfaceGroup, false, out interfaceChannels[0] ) );
    }


    public void Load( string _path, SOUND_LOAD_TYPE _type = SOUND_LOAD_TYPE.DEFAULT, SOUND_PLAY_MODE _mode = SOUND_PLAY_MODE.DEFAULT )
    {
        FMOD.MODE mode;
        switch( _mode )
        {
            case SOUND_PLAY_MODE.DEFAULT: { mode = FMOD.MODE.CREATESAMPLE | FMOD.MODE.ACCURATETIME; } break;
            case SOUND_PLAY_MODE.LOOP:    { mode = FMOD.MODE.LOOP_NORMAL  | FMOD.MODE.ACCURATETIME; } break;
            default:                      { mode = FMOD.MODE.CREATESAMPLE | FMOD.MODE.ACCURATETIME; } break;
        }

        FMOD.Sound sound;
        switch ( _type )
        {
            case SOUND_LOAD_TYPE.DEFAULT: { ErrorCheck( system.createSound(  _path, mode, out sound ) ); } break;
            case SOUND_LOAD_TYPE.STREAM:  { ErrorCheck( system.createStream( _path, mode, out sound ) ); } break;
            default:                      { ErrorCheck( system.createSound(  _path, mode, out sound ) ); } break;
        }

        if ( bgmSound != null )
        {
            ErrorCheck( bgmSound.Value.release() );
            bgmSound = null;
        }

        uint length;
        ErrorCheck( sound.getLength( out length, FMOD.TIMEUNIT.MS ) );
        Length   = length;
        
        bgmSound = sound;
    }
    
    public void Play()
    {
        if ( bgmSound == null ) return;
        // AllStop();

        ErrorCheck( system.playSound( bgmSound.Value, backgroundGroup, false, out bgmChannel ) );
        // DOTween.To( () => 0, x => ErrorCheck( bgmChannel.setVolume( x ) ), volume, 1.5f );

        // ErrorCheck( bgmChannel.setChannelGroup( masterChannelGroup ) );

        //int numChannels;
        //ErrorCheck( masterChannelGroup.getNumChannels( out numChannels ) );
        //Debug.Log( $"number of channels in the masterchannelgroup : {numChannels}" );
    }

    public void AllStop()
    {
        if ( IsPlaying( SOUND_GROUP_TYPE.MASTER ) )      ErrorCheck( masterGroup.stop() );
        if ( IsPlaying( SOUND_GROUP_TYPE.SFX ) )         ErrorCheck( sfxGroup.stop() );
        if ( IsPlaying( SOUND_GROUP_TYPE.INTERFACE ) )   ErrorCheck( interfaceGroup.stop() );
        if ( IsPlaying( SOUND_GROUP_TYPE.BACKGROUND  ) ) ErrorCheck( backgroundGroup.stop() );
    }

    public bool IsPlaying( SOUND_GROUP_TYPE _type = SOUND_GROUP_TYPE.MASTER )
    {
        bool isPlay = false;
        switch ( _type )
        {
            case SOUND_GROUP_TYPE.MASTER:     { ErrorCheck( masterGroup.isPlaying( out isPlay ) ); }     break;
            case SOUND_GROUP_TYPE.SFX:        { ErrorCheck( sfxGroup.isPlaying( out isPlay ) ); }        break;
            case SOUND_GROUP_TYPE.INTERFACE:  { ErrorCheck( interfaceGroup.isPlaying( out isPlay ) ); }  break;
            case SOUND_GROUP_TYPE.BACKGROUND: { ErrorCheck( backgroundGroup.isPlaying( out isPlay ) ); } break;
            default:                          { ErrorCheck( masterGroup.isPlaying( out isPlay ) ); }     break;
        }

        return isPlay;
    }

    public float GetVolume( SOUND_GROUP_TYPE _type = SOUND_GROUP_TYPE.MASTER )
    {
        float volume = 0f;
        switch( _type )
        {
            case SOUND_GROUP_TYPE.MASTER:     { ErrorCheck( masterGroup.getVolume( out volume ) ); }     break;
            case SOUND_GROUP_TYPE.SFX:        { ErrorCheck( sfxGroup.getVolume( out volume ) ); }        break;
            case SOUND_GROUP_TYPE.INTERFACE:  { ErrorCheck( interfaceGroup.getVolume( out volume ) ); }  break;
            case SOUND_GROUP_TYPE.BACKGROUND: { ErrorCheck( backgroundGroup.getVolume( out volume ) ); } break;
            default:                                { ErrorCheck( masterGroup.getVolume( out volume ) ); }     break;
        }
        return volume;
    }

    public float SetVolume( float _value, SOUND_GROUP_TYPE _type = SOUND_GROUP_TYPE.MASTER )
    {
        volume = _value;
        if ( _value < 0f ) volume = 0f;
        if ( _value > 1f ) volume = 1f;

        switch ( _type )
        {
            case SOUND_GROUP_TYPE.MASTER:     { ErrorCheck( masterGroup.setVolume( volume ) ); }     break;
            case SOUND_GROUP_TYPE.SFX:        { ErrorCheck( sfxGroup.setVolume( volume ) ); }        break;
            case SOUND_GROUP_TYPE.INTERFACE:  { ErrorCheck( interfaceGroup.setVolume( volume ) ); }  break;
            case SOUND_GROUP_TYPE.BACKGROUND: { ErrorCheck( backgroundGroup.setVolume( volume ) ); } break;
            default:                                   { ErrorCheck( masterGroup.setVolume( volume ) ); }     break;
        }

        return volume;
    }

    public void SetPosition( uint _position )
    {
        if ( IsPlaying( SOUND_GROUP_TYPE.MASTER ) )
        {
            ErrorCheck( bgmChannel.setPosition( _position, FMOD.TIMEUNIT.MS ) );
        }
    }

    public void SetPitch( float _value )
    {
        int value = Mathf.RoundToInt( _value * 10f );
        if ( value < 5f || value > 15f )
        {
            Debug.Log( "pitch range 0.9 ~ 1.3" );
            return;
        }

        if ( IsPlaying( SOUND_GROUP_TYPE.BACKGROUND ) )
        {
            backgroundGroup.setPitch( _value );
        }

        Pitch = _value;
    }

    private bool ErrorCheck( FMOD.RESULT _res )
    {
        if ( FMOD.RESULT.OK != _res )
        {
            Debug.LogError( FMOD.Error.String( _res ) );
            return false;
        }

        return true;
    }
    #endregion

    #region DSP
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
        int numDsp;
        ErrorCheck( backgroundGroup.getNumDSPs( out numDsp ) );
        for ( int i = 0; i < numDsp; i++ )
        {
            FMOD.DSP dsp;
            ErrorCheck( backgroundGroup.getDSP( i, out dsp ) );

            bool isEquals = Equals( dsp, lowEffectEQ );
            if ( isEquals && _isUse == true ) // 이미 적용된 상태
            {
                return;
            }
            else if ( isEquals && _isUse == false )
            {
                ErrorCheck( backgroundGroup.removeDSP( lowEffectEQ ) );
                return;
            }
        }

        // 적용된 dsp가 없어서 추가함.
        if ( _isUse == true )
             ErrorCheck( backgroundGroup.addDSP( FMOD.CHANNELCONTROL_DSP_INDEX.TAIL, lowEffectEQ ) );
    }
    #endregion
}
