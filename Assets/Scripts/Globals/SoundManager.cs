using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SoundManager : Singleton<SoundManager>
{

    #region variables
    private Dictionary<string /* file name */, Sound> sounds = new Dictionary<string, Sound>();

    private FMOD.ChannelGroup channelGroup = new FMOD.ChannelGroup();
    public int frequency { get; private set; }
    private readonly ushort bufferSize = 256;

    private FMOD.RESULT result;
    private bool isGroupActive = false;
    #endregion

    #region structures
    public struct Sound
    {
        public FMOD.Sound sound;
        public FMOD.Channel channel;

        public Sound( FMOD.Sound _sound, FMOD.Channel _channel )
        {
            sound = _sound;
            channel = _channel;
        }
    }
    #endregion

    #region properties

    public float volume
    {
        get { return volume; }
        set
        {
            if ( !isGroupActive )
            {
                Debug.Log( "master channel group is not assigned." );
                return;
            }

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
        GameManager.GameInit += Initialize;
    }
    #endregion

    #region customize functions
    private void Initialize()
    {
        int freq, numlowspeak;
        FMOD.SPEAKERMODE speakmode;
        FMODUnity.RuntimeManager.CoreSystem.setDSPBufferSize( bufferSize, 4 );
        FMODUnity.RuntimeManager.CoreSystem.getSoftwareFormat( out freq, out speakmode, out numlowspeak );

        result = FMODUnity.RuntimeManager.CoreSystem.getMasterChannelGroup( out channelGroup );
        if ( result == FMOD.RESULT.OK ) isGroupActive = true;

        Debug.Log( "SoundManager Initizlize Successful." );
    }

    public Sound Load( string _path, bool _loop = false )
    {
        FMOD.Sound sound = new FMOD.Sound();
        FMOD.Channel channel = new FMOD.Channel();
        channel.setChannelGroup( channelGroup );

        FMOD.MODE mode;
        if ( _loop ) mode = FMOD.MODE.LOOP_NORMAL  | FMOD.MODE.ACCURATETIME;
        else         mode = FMOD.MODE.CREATESAMPLE | FMOD.MODE.ACCURATETIME;
        result = FMODUnity.RuntimeManager.CoreSystem.createSound( _path, mode, out sound );

        if ( result != FMOD.RESULT.OK )
        {
            Debug.LogError( "failed to load sound. #Code : " + result.ToString() );
        }
        string name = Path.GetFileNameWithoutExtension( _path );
        Sound newSound = new Sound( sound, channel );
        sounds.Add( name, newSound );

        return newSound;
    }

    // _name : name with removed extenstion.
    public void Play( string _name )
    {
        if ( !sounds.ContainsKey( _name ) )
        {
            Debug.LogError( "the sound was not loaded. #Name : " + _name );
        }
        
        FMOD.Sound sound = sounds[ _name ].sound;
        FMOD.Channel channel = sounds[ _name ].channel;
        
        result = FMODUnity.RuntimeManager.CoreSystem.playSound( sound, channelGroup, false, out channel );

        if ( result != FMOD.RESULT.OK )
        {
            Debug.LogError( "sound playback failed. #Code : " + result );
            return;
        }
    }

    public void Play( Sound _sound )
    {
        result = FMODUnity.RuntimeManager.CoreSystem.playSound( _sound.sound, channelGroup, false, out _sound.channel );

        if ( result != FMOD.RESULT.OK )
        {
            Debug.LogError( "sound playback failed. #Code : " + result );
            return;
        }
    }

    public void Stop( string _name )
    {
        if ( !sounds.ContainsKey( _name ) )
        {
            Debug.LogError( "the sound was not loaded. #Name : " + _name );
        }

        FMOD.Sound sound = sounds[ _name ].sound;
        FMOD.Channel channel = sounds[ _name ].channel;

        bool isPlay = false;
        channel.isPlaying( out isPlay );

        if ( isPlay )
        {
            channel.stop();
        }
    }

    public void AllStop()
    {
        if ( isGroupActive )
            channelGroup.stop();
    }
    #endregion
}
