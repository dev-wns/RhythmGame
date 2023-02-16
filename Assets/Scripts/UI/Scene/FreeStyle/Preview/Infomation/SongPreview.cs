using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SongPreview : MonoBehaviour
{
    public FreeStyleMainScroll scroller;
    public SoundPitchOption pitchOption;

    public TextMeshProUGUI keyCount;
    public Image keySound;

    [Header("Line 0")]
    public TextMeshProUGUI speed;
    public TextMeshProUGUI rate;
    public TextMeshProUGUI mode;

    [Header("Line 1")]
    public TextMeshProUGUI length;
    public TextMeshProUGUI noteCount;
    public TextMeshProUGUI sliderCount;
    public TextMeshProUGUI bpm;

    [Header("Background Type")]
    public Image  backgroundType;
    public Sprite bgImage;
    public Sprite bgSprite;
    public Sprite bgVideo;


    private void Awake()
    {
        scroller.OnSelectSong += SelectChangedSoundInfo;
        pitchOption.OnPitchUpdate += PitchUpdate;
    }

    private void SelectChangedSoundInfo( Song _song )
    {
        backgroundType.sprite = _song.hasVideo  ? bgVideo :
                                _song.hasSprite ? bgSprite :
                                                  bgImage;

        keySound.color = _song.hasKeySound ? Color.white : new Color( 1f, 1f, 1f, .25f );


        noteCount.text   = _song.noteCount.ToString();
        sliderCount.text = _song.sliderCount.ToString();
        keyCount.text    = $"{_song.keyCount}K";

        speed.text = $"{GameSetting.ScrollSpeed:F1}";
        mode.text  = $"{GameSetting.CurrentRandom.ToString().Split( '_' )[0]}";
        rate.text  = $"x{GameSetting.CurrentPitch:F1}";
        rate.color = GameSetting.CurrentPitch < 1f ? new Color( .5f, .5f, 1f ) :
                           GameSetting.CurrentPitch > 1f ? new Color( 1f, .5f, .5f ) : Color.white;

        PitchUpdate( GameSetting.CurrentPitch );
    }

    private void PitchUpdate( float _pitch )
    {
        Song song = NowPlaying.CurrentSong;
        int second = ( int )( ( song.totalTime * .001f ) / _pitch );
        int minute = second / 60;
        second = second % 60;

        if ( Global.Math.Abs( _pitch - 1f ) < .0001f )
        {
            length.text = $"{minute:00}:{second:00}";

            int medianBpm = Mathf.RoundToInt( ( float )song.medianBpm );
            if ( song.minBpm == song.maxBpm ) bpm.text = medianBpm.ToString();
            else                              bpm.text = $"{medianBpm} ({song.minBpm} ~ {song.maxBpm})";
        }
        else
        {
            length.text = $"{minute:00}:{second:00}";

            int medianBpm = Mathf.RoundToInt( ( float )song.medianBpm * _pitch  );
            if ( song.minBpm == song.maxBpm ) bpm.text = medianBpm.ToString();
            else                              bpm.text = $"{medianBpm} ({Mathf.RoundToInt( song.minBpm * _pitch )} ~ {Mathf.RoundToInt( song.maxBpm * _pitch )})";
        }

        length.color = bpm.color = _pitch < 1f ? new Color( .5f, .5f, 1f ) :
                                   _pitch > 1f ? new Color( 1f, .5f, .5f ) : Color.white;
    }
}
