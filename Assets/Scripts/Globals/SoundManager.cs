using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SoundManager : Singleton<SoundManager>
{

    #region variables
    public FMOD.ChannelGroup channelGroup = new FMOD.ChannelGroup();
    private FMOD.Channel[] channels = new FMOD.Channel[100];
    private readonly ushort bufferSize = 256;
    private FMOD.Sound sound;


    public delegate void DelSoundReleased();
    public static event DelSoundReleased OnRelease;
    #endregion

    #region properties
    private uint pos;
    private bool isPlay;
    private float volume;
    public float Volume
    {
        get 
        { 
            if ( volume == 0f )
            {
                channelGroup.getVolume( out volume );
            }

            return volume; 
        }
        set
        {
            if ( value < 0f || value > 1f )
            {
                Debug.Log( "out of range configurable values. value from 0 ~ 1 are allowed." );
                return;
            }

            channelGroup.setVolume( value );
            volume = value;
        }
    }
    public bool IsPlay
    {
        get
        {
            channels[0].isPlaying( out isPlay );
            return isPlay;
        }
    }
    public uint Position 
    { 
        get
        {
            if ( FMOD.RESULT.OK != channels[0].getPosition( out pos, FMOD.TIMEUNIT.MS ) ) return 0;
            return pos;
        }
        set
        {
            if ( IsPlay ) channels[0].setPosition( value, FMOD.TIMEUNIT.MS );
        }
    }
    public uint Length
    {
        get
        {
            if ( FMOD.RESULT.OK != sound.getLength( out pos, FMOD.TIMEUNIT.MS ) ) return 0;
            return pos;
        }
    }

    #endregion

    #region unity callback functions
    private void Awake()
    {
        int freq, numlowspeak;
        FMOD.SPEAKERMODE speakmode;
        FMODUnity.RuntimeManager.CoreSystem.setDSPBufferSize( bufferSize, 4 );
        FMODUnity.RuntimeManager.CoreSystem.getSoftwareFormat( out freq, out speakmode, out numlowspeak );
        FMODUnity.RuntimeManager.CoreSystem.getMasterChannelGroup( out channelGroup );

        for( int idx = 0; idx < 100; ++idx )
        {
            channels[idx].setChannelGroup( channelGroup );
        }

        Debug.Log( "SoundManager Initizlize Successful." );
    }

    private void OnApplicationQuit()
    {
        if ( ReferenceEquals( sound, null ) ) sound.release();

        OnRelease();
        //result = FMODUnity.RuntimeManager.CoreSystem.release();
    }
    #endregion

    #region customize functions

    public FMOD.Sound Load( string _path, bool _loop = false )
    {
        FMOD.RESULT result = FMOD.RESULT.OK;
        FMOD.Sound sound;// = new FMOD.Sound();
        
        FMOD.MODE mode;
        if ( _loop ) mode = FMOD.MODE.LOOP_NORMAL  | FMOD.MODE.ACCURATETIME;
        else         mode = FMOD.MODE.CREATESAMPLE | FMOD.MODE.ACCURATETIME;
        FMODUnity.RuntimeManager.CoreSystem.createSound( _path, mode, out sound );

        if ( result != FMOD.RESULT.OK )
        {
            Debug.LogError( string.Format( "failed to load sound. #Code : {0}", result.ToString() ) );
        }

        return sound;
    }

    public void LoadAndPlay( string _path, bool _loop = false )
    {
        Stop();
        Play( Load( _path, _loop ) );
    }

    public void Play( FMOD.Sound _sound )
    {
        if ( ReferenceEquals( sound, null ) ) sound.release();

        FMOD.RESULT result = FMOD.RESULT.OK;
        result = FMODUnity.RuntimeManager.CoreSystem.playSound( _sound, channelGroup, false, out channels[0] );

        if ( result != FMOD.RESULT.OK )
        {
            Debug.LogError( string.Format( "sound play failed. #Code : {0}", result ) );
            return;
        }

        sound = _sound;
    }

    public void Stop()
    {
        bool isPlay = false;
        channelGroup.isPlaying( out isPlay );

        if ( isPlay ) channelGroup.stop();
    }
    #endregion
}
