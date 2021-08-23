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
    private bool isDrag = false;
    private uint progressTime, totalTime;
    public TextMeshProUGUI progressTimer, totalTimer;
    public Slider progressBar;
    public GameObject musicInfoPrefab;
    public Transform scrollViewContent;

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
            TextMeshProUGUI[] texts = newInfo.GetComponentsInChildren<TextMeshProUGUI>();
            texts[0].text = file.Name.Substring( idx + 1, file.Name.Length - idx - 5 ).Trim();
            texts[1].text = file.Name.Substring( 0, idx ).Trim();
        }
    }
    public void Start()
    {
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
            int rnd = Random.Range( 0, musics.Count - 1 );
            Play( rnd );
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
    }

    private string IntToTime( uint _ms )
    {
        float ms = _ms / 1000f;
        int s = ( int )( ms % 3600f % 60f );
        int m = ( int )( ms % 3600f / 60f );

        return string.Format( "{0}:{1}", m.ToString( "D2" ), s.ToString( "D2" ) );
    }
}
