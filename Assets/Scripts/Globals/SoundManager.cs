using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sound
{ 
    public enum LoadType { Default, Stream }
    public enum Mode { Default, Loop }
    public enum ChannelType { MasterGroup, sfxGroup, InterfaceGroup, BackgroundGroup };
}

public class SoundManager : SingletonUnity<SoundManager>
{
    #region variables
    private FMOD.System       system;
    private FMOD.ChannelGroup masterChannelGroup, sfxChannelGroup;
    private FMOD.Channel      bgmChannel;
    private FMOD.Sound?       bgmSound;
    //private FMOD.Channel[]   sfxChannels     = new FMOD.Channel[sfxChannelCount];
    //private const int        sfxChannelCount = 100;

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

        if ( soundDrivers.Count <= _index ||
             curIndex == _index )
        {
            return;
        }

        ErrorCheck( system.setDriver( _index ) );
    }

    public void AddFFT( int _size, FMOD.DSP_FFT_WINDOW _type, out FMOD.DSP _dsp )
    {
        if ( FFT != null ) RemoveFFT();

        ErrorCheck( system.createDSPByType( FMOD.DSP_TYPE.FFT, out _dsp ) );
        ErrorCheck( _dsp.setParameterInt( ( int )FMOD.DSP_FFT.WINDOWSIZE, _size ) );
        ErrorCheck( _dsp.setParameterInt( ( int )FMOD.DSP_FFT.WINDOWTYPE, ( int )_type ) );
        ErrorCheck( masterChannelGroup.addDSP( FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, _dsp ) );
        FFT = _dsp;
    }

    public void RemoveFFT()
    {
        if ( FFT != null )
        {
            ErrorCheck( masterChannelGroup.removeDSP( FFT.Value ) );
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
        ErrorCheck( system.init( 100, FMOD.INITFLAGS.NORMAL, extraDriverData ) );
        
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
        ErrorCheck( system.createChannelGroup( "MasterChannelGroup", out masterChannelGroup ) );
        ErrorCheck( system.createChannelGroup( "SfxChannelGroup", out sfxChannelGroup ) );

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

        ErrorCheck( masterChannelGroup.removeDSP( lowEffectEQ ) );
        ErrorCheck( lowEffectEQ.release() );

        ErrorCheck( masterChannelGroup.release() );
        ErrorCheck( system.release() ); // 내부에서 close 함.
    }

    #region customize functions
    public void Load( string _path, Sound.LoadType _type = Sound.LoadType.Default, Sound.Mode _mode = Sound.Mode.Default )
    {
        FMOD.MODE mode;
        switch( _mode )
        {
            case Sound.Mode.Default: { mode = FMOD.MODE.CREATESAMPLE | FMOD.MODE.ACCURATETIME; } break;
            case Sound.Mode.Loop:    { mode = FMOD.MODE.LOOP_NORMAL  | FMOD.MODE.ACCURATETIME; } break;
            default:                 { mode = FMOD.MODE.CREATESAMPLE | FMOD.MODE.ACCURATETIME; } break;
        }

        FMOD.Sound sound;
        switch ( _type )
        {
            case Sound.LoadType.Default: { ErrorCheck( system.createSound(  _path, mode, out sound ) ); } break;
            case Sound.LoadType.Stream:  { ErrorCheck( system.createStream( _path, mode, out sound ) ); } break;
            default:                     { ErrorCheck( system.createSound(  _path, mode, out sound ) ); } break;
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

        ErrorCheck( system.playSound( bgmSound.Value, masterChannelGroup, false, out bgmChannel ) );
        // DOTween.To( () => 0, x => ErrorCheck( bgmChannel.setVolume( x ) ), volume, 1.5f );



        // ErrorCheck( bgmChannel.setChannelGroup( masterChannelGroup ) );

        //int numChannels;
        //ErrorCheck( masterChannelGroup.getNumChannels( out numChannels ) );
        //Debug.Log( $"number of channels in the masterchannelgroup : {numChannels}" );
    }

    public void AllStop()
    {
        if ( IsPlaying( Sound.ChannelType.MasterGroup ) ) ErrorCheck( masterChannelGroup.stop() );
        if ( IsPlaying( Sound.ChannelType.sfxGroup ) )    ErrorCheck( sfxChannelGroup.stop() );
    }

    public bool IsPlaying( Sound.ChannelType _type = Sound.ChannelType.MasterGroup )
    {
        bool isPlay = false;
        switch ( _type )
        {
            case Sound.ChannelType.MasterGroup: { ErrorCheck( masterChannelGroup.isPlaying( out isPlay ) ); } break;
            case Sound.ChannelType.sfxGroup:    { ErrorCheck( sfxChannelGroup.isPlaying( out isPlay ) );    } break;
            default:                            { ErrorCheck( masterChannelGroup.isPlaying( out isPlay ) ); } break;
        }

        return isPlay;
    }

    public float GetVolume( Sound.ChannelType _type = Sound.ChannelType.MasterGroup )
    {
        float volume = 0f;
        switch( _type )
        {
            case Sound.ChannelType.MasterGroup: { ErrorCheck( masterChannelGroup.getVolume( out volume ) ); } break;
            case Sound.ChannelType.sfxGroup:    { ErrorCheck( sfxChannelGroup.getVolume( out volume ) );    } break;
            default:                            { ErrorCheck( masterChannelGroup.getVolume( out volume ) ); } break;
        }
        return volume;
    }

    public void SetVolume( float _value, Sound.ChannelType _type = Sound.ChannelType.MasterGroup )
    {
        volume = _value;
        if ( _value < 0f ) volume = 0f;
        if ( _value > 1f ) volume = 1f;

        switch ( _type )
        {
            case Sound.ChannelType.MasterGroup: { ErrorCheck( masterChannelGroup.setVolume( volume ) ); } break;
            case Sound.ChannelType.sfxGroup:    { ErrorCheck( sfxChannelGroup.setVolume( volume ) );    } break;
            default:                            { ErrorCheck( masterChannelGroup.setVolume( volume ) ); } break;
        }
    }

    public void SetPosition( uint _position )
    {
        if ( IsPlaying( Sound.ChannelType.MasterGroup ) )
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

        if ( IsPlaying( Sound.ChannelType.MasterGroup ) )
        {
            masterChannelGroup.setPitch( _value );
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
        ErrorCheck( masterChannelGroup.getNumDSPs( out numDsp ) );
        for ( int i = 0; i < numDsp; i++ )
        {
            FMOD.DSP dsp;
            ErrorCheck( masterChannelGroup.getDSP( i, out dsp ) );

            bool isEquals = Equals( dsp, lowEffectEQ );
            if ( isEquals && _isUse == true ) // 이미 적용된 상태
            {
                return;
            }
            else if ( isEquals && _isUse == false )
            {
                ErrorCheck( masterChannelGroup.removeDSP( lowEffectEQ ) );
                return;
            }
        }

        // 적용된 dsp가 없어서 추가함.
        if ( _isUse == true )
             ErrorCheck( masterChannelGroup.addDSP( FMOD.CHANNELCONTROL_DSP_INDEX.TAIL, lowEffectEQ ) );
    }
    #endregion
}
