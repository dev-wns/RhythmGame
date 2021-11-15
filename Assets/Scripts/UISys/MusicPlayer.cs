using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sound = FMOD.Sound;

public class MusicPlayer : MonoBehaviour
{
    private bool isDrag = false, isPlay = false;
    private uint progressTime, totalTime;
    public TextMeshProUGUI progressTimer, totalTimer;
    public Slider progressBar;
    private Sound backgroundSound;

    private void Awake()
    {
        SoundManager.SoundRelease += SoundRelease;

        backgroundSound = SoundManager.Inst.Load( Application.streamingAssetsPath + "/Musics/O2i3 - ooi.mp3", true );
        SoundManager.Inst.Play( backgroundSound );

        backgroundSound.getLength( out totalTime, FMOD.TIMEUNIT.MS );
        totalTimer.text = IntToTime( totalTime );
        progressBar.maxValue = totalTime;
    }

    private void Update()
    {
        FMOD.Channel channel;
        SoundManager.Inst.channelGroup.getChannel( 0, out channel );
        channel.getPosition( out progressTime, FMOD.TIMEUNIT.MS );
        progressTimer.text = IntToTime( progressTime );
        
        if ( !isDrag ) { progressBar.value = progressTime; }
    }

    protected void SoundRelease()
    {
        backgroundSound.release();
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

    public void PrevPlay()
    {
        FMOD.Channel channel;
        SoundManager.Inst.channelGroup.getChannel( 0, out channel );
        channel.setPosition( 0, FMOD.TIMEUNIT.MS );
    }

    private string IntToTime( uint _ms )
    {
        float totalSecond = _ms / 1000f;
        int s = ( int )( totalSecond % 3600f % 60f );
        int m = ( int )( totalSecond % 3600f / 60f );

        return string.Format( "{0}:{1}", m.ToString( "D2" ), s.ToString( "D2" ) );
    }
}
