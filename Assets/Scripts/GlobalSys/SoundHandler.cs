using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SoundHandler : MonoBehaviour
{
    private FMOD.ChannelGroup channelGroup = new FMOD.ChannelGroup();
    private FMOD.Channel[] channels = new FMOD.Channel[ 1000 ];
    private FMOD.Sound sound;
    private FMOD.Sound[] sfx = new FMOD.Sound[ 20 ];
    private FMOD.RESULT isLoadDone = FMOD.RESULT.ERR_FILE_NOTFOUND;

    private bool isPlaying;
    private uint length;
    private FMOD.Studio.EVENT_CALLBACK dialogueCallback;

    private int sampleChannelIdx = 26;
    private List<FMOD.Sound> keySoundList = new List<FMOD.Sound>();

    private void Start ()
    {
        // Sound
        FMOD.RESULT error = FMODUnity.RuntimeManager.CoreSystem.getMasterChannelGroup( out channelGroup );
        Debug.Log( error );
        for ( int i = 0; i < 1000; ++i )
        {
            channels[ i ] = new FMOD.Channel();
            channels[ i ].setChannelGroup( channelGroup );
        }

        // Sfx
        StreamReader srd = new StreamReader( Path.Combine( Application.streamingAssetsPath, "SFXs", "sfx.ini" ) );
        string line;
        int idx = 0;
        while ( ( line = srd.ReadLine() ) != null )
        {
            string root = Path.Combine( Application.streamingAssetsPath, "SFXs", line );
            if ( File.Exists( root ) )
                LoadSfx( idx++, root );
        }
        srd.Close();
    }

    private void LoadSfx( int _idx, string _path )
    {
        sfx[ _idx ] = new FMOD.Sound();
        FMODUnity.RuntimeManager.CoreSystem.createSound( _path, FMOD.MODE.CREATESAMPLE, out sfx[ _idx ] );
    }

    private void LoadSound( string _path )
    {
        sound = new FMOD.Sound();
        isLoadDone = FMODUnity.RuntimeManager.CoreSystem.createSound( _path, FMOD.MODE.CREATESAMPLE | FMOD.MODE.ACCURATETIME, out sound );
    }

    private void LoadKeySound( string _path )
    {
        FMOD.Sound key = new FMOD.Sound();
        FMODUnity.RuntimeManager.CoreSystem.createSound( _path, FMOD.MODE.CREATESAMPLE, out key );
        keySoundList.Add( key );
    }

    public void PlaySound()
    {
        FMODUnity.RuntimeManager.CoreSystem.playSound( sound, channelGroup, false, out channels[ 0 ] );
        sound.getLength( out length, FMOD.TIMEUNIT.MS );
        channelGroup.setVolume( GlobalSettings.volume );
        FMODUnity.RuntimeManager.CoreSystem.getDSPBufferSize( out uint a, out int b );
    }

    public void StopSound()
    {
        channelGroup.isPlaying( out isPlaying );
        if ( isPlaying )
        {
            channelGroup.stop();
        }
    }

    public void PlaySFX( int _idx )
    {
        FMODUnity.RuntimeManager.CoreSystem.playSound( sfx[ _idx ], channelGroup, false, out channels[ 1 ] );
        channelGroup.setVolume( GlobalSettings.volume );
    }

    public void PlaySample( int _idx )
    {
        bool isPlaying = true;
        sampleChannelIdx = 2;
        while ( !isPlaying )
        {
            channels[ sampleChannelIdx ].isPlaying( out isPlaying );
            ++sampleChannelIdx;
            if ( sampleChannelIdx == 1000 )
            {
                sampleChannelIdx = ( int )Random.Range( 2.0f, 1000.0f );
                channels[ sampleChannelIdx ].stop();
                isPlaying = false;
            }
        }
        FMODUnity.RuntimeManager.CoreSystem.playSound( keySoundList[ _idx ], channelGroup, false, out channels[ sampleChannelIdx ] );
    }

    public void ReleaseSound()
    {
        sound.release();
    }

    public void ReleaseKeySound()
    {
        for ( int i = 0; i < keySoundList.Count; ++i )
        {
            keySoundList[ i ].release();
        }

        keySoundList = new List<FMOD.Sound>();
    }

    public FMOD.OPENSTATE IsLoaded()
    {
        FMOD.OPENSTATE state;
        bool disk, starving;
        uint percent;
        sound.getOpenState( out state, out percent, out starving, out disk );
        return state;
    }

    private void OnApplicationQuit()
    {
        sound.release();
        ReleaseKeySound();
        for ( int i = 0; i < sfx.Length; ++i )
        {
            sfx[ i ].release();
        }

#if !UNITY_EDITOR
        FMODUnity.RuntimeManager.CoreSystem.release();
#endif
    }
}
