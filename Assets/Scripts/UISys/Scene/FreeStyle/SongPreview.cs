using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SongPreview : MonoBehaviour
{
    public FreeStyleMainScroll scroller;
    public SoundPitchOption pitchOption;

    [Header("Line 0")]
    public TextMeshProUGUI scrollSpeed;
    public TextMeshProUGUI soundSpeed;
    public TextMeshProUGUI random;

    [Header("Line 1")]
    public TextMeshProUGUI noteCount;
    public TextMeshProUGUI sliderCount;
    public TextMeshProUGUI time;
    public TextMeshProUGUI bpm;

    [Header("Line 2")]
    public TextMeshProUGUI backgroundType;
    public TextMeshProUGUI keyCount;
    public TextMeshProUGUI hasKeySound;


    private void Awake()
    {
        scroller.OnSelectSong += SelectChangedSoundInfo;
        pitchOption.OnPitchUpdate += PitchUpdate;
    }

    private void SelectChangedSoundInfo( Song _song )
    {
        backgroundType.text = _song.hasVideo  ? "Video" :
                              _song.hasSprite ? "Sprite" :
                                                "Image";
        hasKeySound.text = _song.hasKeySound ? "O" : "X";


        noteCount.text   = _song.noteCount.ToString();
        keyCount.text    = _song.keyCount.ToString();
        sliderCount.text = _song.sliderCount.ToString();

        scrollSpeed.text = $"{GameSetting.ScrollSpeed:F1}";
        random.text      = $"{GameSetting.CurrentRandom.ToString().Split( '_' )[0]}";
        soundSpeed.text  = $"x{GameSetting.CurrentPitch:F1}";
        soundSpeed.color = GameSetting.CurrentPitch < 1f ? new Color( .5f, .5f, 1f ) :
                           GameSetting.CurrentPitch > 1f ? new Color( 1f, .5f, .5f ) : Color.white;

        PitchUpdate( GameSetting.CurrentPitch );
    }

    private void PitchUpdate( float _pitch )
    {
        Song song = NowPlaying.Inst.CurrentSong;
        int second = ( int )( ( song.totalTime * .001f ) / _pitch );
        int minute = second / 60;
        second = second % 60;

        if ( Global.Math.Abs( _pitch - 1f ) < .0001f )
        {
            time.text = $"{minute:00}:{second:00}";

            int medianBpm = Mathf.RoundToInt( ( float )song.medianBpm );
            if ( song.minBpm == song.maxBpm ) bpm.text = medianBpm.ToString();
            else                              bpm.text = $"{medianBpm} ({song.minBpm} ~ {song.maxBpm})";
        }
        else
        {
            time.text = $"{minute:00}:{second:00}";

            int medianBpm = Mathf.RoundToInt( ( float )song.medianBpm * _pitch  );
            if ( song.minBpm == song.maxBpm ) bpm.text = medianBpm.ToString();
            else                              bpm.text = $"{medianBpm} ({Mathf.RoundToInt( song.minBpm * _pitch )} ~ {Mathf.RoundToInt( song.maxBpm * _pitch )})";
        }

        time.color = bpm.color = _pitch < 1f ? new Color( .5f, .5f, 1f ) :
                                 _pitch > 1f ? new Color( 1f, .5f, .5f ) : Color.white;
    }
}
