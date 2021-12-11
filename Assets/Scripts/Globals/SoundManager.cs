using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    public enum SoundLoadType { Default, Stream }

    #region variables
    private FMOD.System       system;
    private FMOD.ChannelGroup masterChannelGroup, sfxChannelGroup;
    private FMOD.Channel      bgmChannel;
    //private FMOD.Channel[]   sfxChannels     = new FMOD.Channel[sfxChannelCount];
    //private const int        sfxChannelCount = 100;

    private FMOD.Sound? bgmSound;

    public FMOD.DSP VisualizerDsp { get { return visualizerDsp; } }
    private FMOD.DSP visualizerDsp, lowEffectEQ;

    private struct SoundDriver
    {
        public System.Guid guid;
        public int index;
        public string name;
        public int systemRate, speakModeChannels;
        public FMOD.SPEAKERMODE mode;
    }
    private List<SoundDriver> soundDrivers = new List<SoundDriver>();
    private int driverCount, currentDriverIndex;
    private uint version;

    private uint bgmLength, bgmPosition;
    private float volume, pitch = 1f;
    private bool isPlaying;
    #endregion

    #region properties and getter setter
    public bool IsPlaying 
    {
        get
        {
            ErrorCheck( bgmChannel.isPlaying( out isPlaying ) );
            return isPlaying;
        }
    }
    public float Pitch
    {
        get { return pitch; }
        set
        {
            if ( value < .9f || value > 1.3f )
            {
                Debug.Log( "pitch range is 0.9 ~ 1.3" );
                return;
            }
            pitch = value;
        }
    }
    public float Volume
    {
        get 
        {
            if ( !ErrorCheck( masterChannelGroup.getVolume( out volume ) ) ) return float.NaN;
            return volume; 
        }
        set
        {
            if ( value < 0f || value > 1f )
            {
                Debug.Log( "out of range configurable values. value from 0 ~ 1 are allowed." );
                return;
            }

            if ( !ErrorCheck( masterChannelGroup.setVolume( value ) ) ) return;
        }
    }
    public uint Position
    {
        get
        {
            if ( !IsPlaying || 
                 !ErrorCheck( bgmChannel.getPosition( out bgmPosition, FMOD.TIMEUNIT.MS ) ) )
            {
                return uint.MaxValue;
            }

            return bgmPosition;
        }
        set
        {
            if ( !IsPlaying ||
                 !ErrorCheck( bgmChannel.setPosition( value, FMOD.TIMEUNIT.MS ) ) )
            {
                return;
            }
        }
    }
    public uint Length
    {
        get
        {
            if ( !IsPlaying || 
                 !ErrorCheck( bgmSound.Value.getLength( out bgmLength, FMOD.TIMEUNIT.MS ) ) )
            {
                return uint.MaxValue;
            }

            return bgmLength;
        }
    }
    public uint GetLength( FMOD.Sound _sound )
    {
        if ( !ErrorCheck( _sound.getLength( out bgmLength, FMOD.TIMEUNIT.MS ) ) )
            return uint.MaxValue;

        return bgmLength;
    }
    #endregion


    #region unity callback functions

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
        ErrorCheck( system.createDSPByType( FMOD.DSP_TYPE.FFT, out visualizerDsp ) );
        ErrorCheck( visualizerDsp.setParameterInt( ( int )FMOD.DSP_FFT.WINDOWSIZE, 4096 ) );
        ErrorCheck( visualizerDsp.setParameterInt( ( int )FMOD.DSP_FFT.WINDOWTYPE, ( int )FMOD.DSP_FFT_WINDOW.BLACKMANHARRIS ) );
        ErrorCheck( masterChannelGroup.addDSP( FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, visualizerDsp ) );

        CreateLowEffectDsp();
        //ErrorCheck( masterChannelGroup.addDSP( FMOD.CHANNELCONTROL_DSP_INDEX.TAIL, lowEffectEQ ) );

        // Details
        Volume = 0.1f;

        Debug.Log( "SoundManager Initizlize Successful." );
    }
    private void Update()
    {
        system.update();
    }

    private void OnApplicationQuit()
    {
        // 생성한 역순으로 release
        if ( bgmSound != null )
        {
            ErrorCheck( bgmSound.Value.release() );
            bgmSound = null;
        }

        ErrorCheck( masterChannelGroup.removeDSP( visualizerDsp ) );
        ErrorCheck( visualizerDsp.release() );

        ErrorCheck( masterChannelGroup.removeDSP( lowEffectEQ ) );
        ErrorCheck( lowEffectEQ.release() );

        ErrorCheck( masterChannelGroup.release() );
        ErrorCheck( system.release() ); // 내부에서 close 함.
    }

    #endregion

    #region customize functions
    public FMOD.Sound Load( string _path, bool _loop = false )
    {
        FMOD.MODE mode;
        if ( _loop ) mode = FMOD.MODE.LOOP_NORMAL  | FMOD.MODE.ACCURATETIME;
        else         mode = FMOD.MODE.CREATESAMPLE | FMOD.MODE.ACCURATETIME;

        FMOD.Sound sound;
        // FMODUnity.RuntimeManager.CoreSystem.createSound( _path, mode, out sound );
        if ( ErrorCheck( system.createStream( _path, mode, out sound ) ) )
        {
            if ( bgmSound != null )
            {
                ErrorCheck( bgmSound.Value.release() );
                bgmSound = null;
            }

            bgmSound = sound;
        }

        return sound;
    }

    public void BGMPlay()
    {
        ErrorCheck( system.playSound( bgmSound.Value, masterChannelGroup, false, out bgmChannel ) );
        ErrorCheck( bgmChannel.setChannelGroup( masterChannelGroup ) );
        ErrorCheck( bgmChannel.setPitch( pitch ) );
    }

    public void BGMPlay( FMOD.Sound _sound )
    {
        ErrorCheck( system.playSound( _sound, masterChannelGroup, false, out bgmChannel ) );
        ErrorCheck( bgmChannel.setChannelGroup( masterChannelGroup ) );
        ErrorCheck( bgmChannel.setPitch( pitch ) );

        //int numChannels;
        //ErrorCheck( masterChannelGroup.getNumChannels( out numChannels ) );
        //Debug.Log( $"number of channels in the masterchannelgroup : {numChannels}" );
    }

    public void AllStop()
    {
        bool isPlay = false;
        ErrorCheck( masterChannelGroup.isPlaying( out isPlay ) );

        if ( isPlay )
        {
            ErrorCheck( masterChannelGroup.stop() );
            if ( bgmSound != null )
            {
                ErrorCheck( bgmSound.Value.release() );
                bgmSound = null;
            }
        }
    }

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
    private void AddLowEqualizer()
    {
        ErrorCheck( masterChannelGroup.addDSP( FMOD.CHANNELCONTROL_DSP_INDEX.TAIL, lowEffectEQ ) );

    }

    private void RemoveLowEqualizer()
    {
        ErrorCheck( masterChannelGroup.removeDSP( lowEffectEQ ) );
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
    #endregion
}
