using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    #region variables
    public FMOD.ChannelGroup channelGroup    = new FMOD.ChannelGroup();
    private FMOD.Channel     bgmChannel      = new FMOD.Channel();
    //private FMOD.Channel[]   sfxChannels     = new FMOD.Channel[sfxChannelCount];
    //private const int        sfxChannelCount = 100;

    private FMOD.Sound bgmSound;

    private int sampleRate, numLowSpeak;
    private FMOD.SPEAKERMODE speakMode;

    public FMOD.DSP VisualizerDsp { get { return visualizerDsp; } }
    private FMOD.DSP visualizerDsp, lowEffectEQ;

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
            if ( !ErrorCheck( channelGroup.getVolume( out volume ) ) ) return float.NaN;
            return volume; 
        }
        set
        {
            if ( value < 0f || value > 1f )
            {
                Debug.Log( "out of range configurable values. value from 0 ~ 1 are allowed." );
                return;
            }

            if ( !ErrorCheck( channelGroup.setVolume( value ) ) ) return;
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
                 !ErrorCheck( bgmSound.getLength( out bgmLength, FMOD.TIMEUNIT.MS ) ) )
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
        // Channel Initialize
        ErrorCheck( FMODUnity.RuntimeManager.CoreSystem.getSoftwareFormat( out sampleRate, out speakMode, out numLowSpeak ) );
        ErrorCheck( FMODUnity.RuntimeManager.CoreSystem.getMasterChannelGroup( out channelGroup ) );
        Debug.Log( $"SampleRate : {sampleRate.ToString()}, SpeakMode = {speakMode.ToString()}, NumLowSpeak : {numLowSpeak}" );
     
        bgmChannel.setChannelGroup( channelGroup );
        //for( int idx = 0; idx < sfxChannelCount; ++idx )
        //{
        //    sfxChannels[idx].setChannelGroup( channelGroup );
        //}

        // DSP Setting
        ErrorCheck( FMODUnity.RuntimeManager.CoreSystem.createDSPByType( FMOD.DSP_TYPE.FFT, out visualizerDsp ) );
        ErrorCheck( visualizerDsp.setParameterInt( ( int )FMOD.DSP_FFT.WINDOWSIZE, 4096 ) );
        ErrorCheck( visualizerDsp.setParameterInt( ( int )FMOD.DSP_FFT.WINDOWTYPE, ( int )FMOD.DSP_FFT_WINDOW.BLACKMANHARRIS ) );
        ErrorCheck( channelGroup.addDSP( FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, visualizerDsp ) );

        CreateLowEffectDsp();

        // Details
        Volume = 0.1f;

        Debug.Log( "SoundManager Initizlize Successful." );
    }
    #endregion

    #region customize functions

    public FMOD.Sound Load( string _path, bool _loop = false )
    {
        FMOD.MODE mode;
        if ( _loop ) mode = FMOD.MODE.LOOP_NORMAL  | FMOD.MODE.ACCURATETIME;
        else         mode = FMOD.MODE.CREATESAMPLE | FMOD.MODE.ACCURATETIME;
        
        ErrorCheck( FMODUnity.RuntimeManager.CoreSystem.createSound( _path, mode, out bgmSound ) );

        return bgmSound;
    }

    public void BGMPlay()
    {
        ErrorCheck( FMODUnity.RuntimeManager.CoreSystem.playSound( bgmSound, channelGroup, false, out bgmChannel ) );
        ErrorCheck( bgmChannel.setPitch( pitch ) );
    }

    public void BGMPlay( FMOD.Sound _sound )
    {
        ErrorCheck( FMODUnity.RuntimeManager.CoreSystem.playSound( _sound, channelGroup, false, out bgmChannel ) );
        ErrorCheck( bgmChannel.setPitch( pitch ) );
    }

    public void AllStop()
    {
        bool isPlay = false;
        ErrorCheck( channelGroup.isPlaying( out isPlay ) );

        if ( isPlay )
        {
            ErrorCheck( channelGroup.stop() );
            ErrorCheck( bgmSound.release() );
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
        ErrorCheck( FMODUnity.RuntimeManager.CoreSystem.createDSPByType( FMOD.DSP_TYPE.MULTIBAND_EQ, out lowEffectEQ ) );

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
        ErrorCheck( channelGroup.addDSP( FMOD.CHANNELCONTROL_DSP_INDEX.TAIL, lowEffectEQ ) );

    }

    private void RemoveLowEqualizer()
    {
        ErrorCheck( channelGroup.removeDSP( lowEffectEQ ) );
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
