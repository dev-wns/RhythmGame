using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sound = FMOD.Sound;

public class MusicPlayer : MonoBehaviour
{
    private List<Sound> musics = new List<Sound>();
    private int curSoundIdx;
    private uint progressTime, totalTime;
    private bool isDrag = false, isPlay = false;
    public TextMeshProUGUI progressTimer, totalTimer;
    public Slider progressBar;
    public GameObject musicInfoPrefab;
    public Transform scrollViewContent;

    public delegate void OnMusicListClick( int _idx );
    public static OnMusicListClick LobbyMusicSelect;

    private void Awake()
    {
        // music files loading.
        System.IO.DirectoryInfo info = new System.IO.DirectoryInfo( Application.streamingAssetsPath + "/Musics" );

        foreach ( var file in info.GetFiles( "*.mp3" ) )
        {
            Sound sound = SoundManager.Inst.Load( file.FullName );
            if ( ReferenceEquals( null, sound ) )
            {
                Debug.Log( "failed to load music.  #Path : " + file.FullName );
                return;
            }
            musics.Add( sound );

            int idx = file.Name.IndexOf( "-" );
            GameObject newInfo = Instantiate( musicInfoPrefab, scrollViewContent );
            newInfo.name = ( musics.Count - 1 ).ToString();
            TextMeshProUGUI[] texts = newInfo.GetComponentsInChildren<TextMeshProUGUI>();
            texts[0].text = file.Name.Substring( idx + 1, file.Name.Length - idx - 5 ).Trim();
            texts[1].text = file.Name.Substring( 0, idx ).Trim();
        }

        LobbyMusicSelect = Play;
        SoundManager.SoundRelease += Release;
    }

    private void Update()
    {
        FMOD.Channel channel;
        SoundManager.Inst.channelGroup.getChannel( 0, out channel );
        channel.getPosition( out progressTime, FMOD.TIMEUNIT.MS );
        progressTimer.text = IntToTime( progressTime );
        
        if ( !isDrag ) { progressBar.value = progressTime; }

        if ( progressTime >= totalTime )
        {
            curSoundIdx = Random.Range( 0, musics.Count - 1 );
            Play( curSoundIdx );
        }
    }

    public void SliderDownEvent() { isDrag = true; }

    public void SliderUpEvent()
    {
        FMOD.Channel channel;
        SoundManager.Inst.channelGroup.getChannel( 0, out channel );
        channel.setPosition( ( uint )progressBar.value, FMOD.TIMEUNIT.MS );
        isDrag = false;
    }

    public void Pause()
    {
        if ( !isPlay ) return;

        FMOD.Channel channel;
        SoundManager.Inst.channelGroup.getChannel( 0, out channel );

        channel.setPaused( true );
        isPlay = false;
    }

    public void NextPlay()
    {
        curSoundIdx = Random.Range( 0, musics.Count - 1 );
        Play( curSoundIdx );
    }

    public void PrevPlay()
    {
        FMOD.Channel channel;
        SoundManager.Inst.channelGroup.getChannel( 0, out channel );
        channel.setPosition( 0, FMOD.TIMEUNIT.MS );
    }
    public void Play()
    {
        if ( isPlay ) return;

        FMOD.Channel channel;
        SoundManager.Inst.channelGroup.getChannel( 0, out channel );

        channel.setPaused( false );
        isPlay = true;
    }

    public void Play( int _idx )
    {
        if ( _idx > musics.Count )
        {
            Debug.Log( "out of range play musics" );
            return;
        }
        SoundManager.Inst.Stop();
        curSoundIdx = _idx;
        FMOD.RESULT res = musics[curSoundIdx].getLength( out totalTime, FMOD.TIMEUNIT.MS );
        if ( res == FMOD.RESULT.OK )
        {
            totalTimer.text = IntToTime( totalTime );
        }
        SoundManager.Inst.Play( musics[curSoundIdx] );
        progressBar.maxValue = totalTime;
        isPlay = true;
    }

    private string IntToTime( uint _ms )
    {
        float totalSecond = _ms / 1000f;
        int s = ( int )( totalSecond % 3600f % 60f );
        int m = ( int )( totalSecond % 3600f / 60f );

        return string.Format( "{0}:{1}", m.ToString( "D2" ), s.ToString( "D2" ) );
    }

    private void Release()
    {
        foreach( var music in musics )
        {
            FMOD.RESULT res = music.release();
            if ( res != FMOD.RESULT.OK )
            {
                Debug.LogError( "sound release failed." );
            }
        }
        Debug.Log( "Lobby MusicPlayer sound release" );
    }
}
