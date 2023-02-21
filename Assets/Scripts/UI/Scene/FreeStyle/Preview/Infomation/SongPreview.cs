using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SongPreview : MonoBehaviour
{
    public FreeStyleMainScroll scroller;
    public SoundPitchOption pitchOption;
    public FixedBPMOption fixedBPMOption;

    [Header("Horizontal")]
    [Header("Line 0")]
    public TextMeshProUGUI keyCount;
    public TextMeshProUGUI speed;
    public TextMeshProUGUI rate;
    public TextMeshProUGUI mode;

    [Header("Line 1")]
    public TextMeshProUGUI length;
    public TextMeshProUGUI noteCount;
    public TextMeshProUGUI sliderCount;
    public TextMeshProUGUI bpm;

    [Header("Vertical")]
    public Image keySound;

    [Header("Background Type")]
    public Image  backgroundType;
    public Sprite bgImage;
    public Sprite bgSprite;
    public Sprite bgVideo;


    private void Awake()
    {
        scroller.OnSelectSong += SelectChangedSoundInfo;
        pitchOption.OnPitchUpdate += PitchUpdate;
        fixedBPMOption.OnChangeOption += ChangeFixedBPM;
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

        length.text = $"{minute:00}:{second:00}";
        length.color = bpm.color = _pitch < 1f ? new Color( .5f, .5f, 1f ) :
                                   _pitch > 1f ? new Color( 1f, .5f, .5f ) : Color.white;

        int medianBpm = Mathf.RoundToInt( ( float )song.medianBpm * _pitch  );
        int minBpm    = Mathf.RoundToInt( ( float )song.minBpm    * _pitch  );
        int maxBpm    = Mathf.RoundToInt( ( float )song.maxBpm    * _pitch  );
        bpm.text = song.minBpm == song.maxBpm || GameSetting.CurrentGameMode.HasFlag( GameMode.FixedBPM ) ? 
                   $"{medianBpm}" : $"{medianBpm} ({minBpm} ~ {maxBpm})";
    }

    private void ChangeFixedBPM()
    {
        var song = NowPlaying.CurrentSong;
        var medianBpm = Mathf.RoundToInt( ( float )song.medianBpm * GameSetting.CurrentPitch );
        var minBpm    = Mathf.RoundToInt( ( float )song.minBpm    * GameSetting.CurrentPitch );
        var maxBpm    = Mathf.RoundToInt( ( float )song.maxBpm    * GameSetting.CurrentPitch );
        bpm.text = GameSetting.CurrentGameMode.HasFlag( GameMode.FixedBPM ) ? 
                   $"{medianBpm}" : $"{medianBpm} ({minBpm} ~ {maxBpm})";
    }
}
