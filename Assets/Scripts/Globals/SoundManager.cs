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
    private FMOD.RESULT result;

    public int frequency { get; private set; }
    public FMOD.ChannelGroup group { get { return channelGroup; } }

    public delegate void OnQuitSoundRelease();
    public static event OnQuitSoundRelease SoundRelease;
    #endregion

    #region properties

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
    #endregion

    #region unity callback functions
    private void Awake()
    {
        int freq, numlowspeak;
        FMOD.SPEAKERMODE speakmode;
        FMODUnity.RuntimeManager.CoreSystem.setDSPBufferSize( bufferSize, 4 );
        FMODUnity.RuntimeManager.CoreSystem.getSoftwareFormat( out freq, out speakmode, out numlowspeak );

        result = FMODUnity.RuntimeManager.CoreSystem.getMasterChannelGroup( out channelGroup );

        for( int idx = 0; idx < 100; ++idx )
        {
            channels[idx].setChannelGroup( channelGroup );
        }

        Debug.Log( "SoundManager Initizlize Successful." );
    }

    private void OnApplicationQuit()
    {
        SoundRelease();
        //result = FMODUnity.RuntimeManager.CoreSystem.release();
    }
    #endregion

    #region customize functions
    private void Initialize()
    {

    }

    public FMOD.Sound Load( string _path, bool _loop = false )
    {
        FMOD.Sound sound;// = new FMOD.Sound();
        
        FMOD.MODE mode;
        if ( _loop ) mode = FMOD.MODE.LOOP_NORMAL  | FMOD.MODE.ACCURATETIME;
        else         mode = FMOD.MODE.CREATESAMPLE | FMOD.MODE.ACCURATETIME;
        result = FMODUnity.RuntimeManager.CoreSystem.createSound( _path, mode, out sound );

        if ( result != FMOD.RESULT.OK )
        {
            Debug.LogError( "failed to load sound. #Code : " + result.ToString() );
        }

        return sound;
    }

    public void Play( FMOD.Sound _sound )
    {
        result = FMODUnity.RuntimeManager.CoreSystem.playSound( _sound, channelGroup, false, out channels[0] );

        if ( result != FMOD.RESULT.OK )
        {
            Debug.LogError( "sound playback failed. #Code : " + result );
            return;
        }
    }

    public void Stop()
    {
        bool isPlay = false;
        channelGroup.isPlaying( out isPlay );

        if ( isPlay ) channelGroup.stop();
    }
    #endregion
}
