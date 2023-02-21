using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.CompilerServices;

public class SongPreview : MonoBehaviour
{
    public FreeStyleMainScroll scroller;
    public SoundPitchOption pitchOption;
    public NoSliderOption   noSliderOption;
    public FixedBPMOption   fixedBPMOption;

    [Header("Horizontal")]
    [Header("Line 0")]
    public TextMeshProUGUI keyCount;
    public TextMeshProUGUI speed;
    public TextMeshProUGUI rate;
    public TextMeshProUGUI random;

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

    private Song song;
    private float pitch;

    private void Awake()
    {
        scroller.OnSelectSong += SelectChangedSoundInfo;
        pitchOption.OnPitchUpdate += UpdateInfo;
        fixedBPMOption.OnChangeOption += UpdateBPMInfo;
        noSliderOption.OnChangeOption += UpdateNoteInfo;
    }

    private void SelectChangedSoundInfo( Song _song )
    {
        song = _song;
        backgroundType.sprite = song.hasVideo  ? bgVideo :
                                song.hasSprite ? bgSprite :
                                                 bgImage;

        keySound.color = song.hasKeySound ? Color.white : new Color( 1f, 1f, 1f, .25f );
        keyCount.text  = $"{song.keyCount}K";

        speed.text  = $"{GameSetting.ScrollSpeed:F1}";
        random.text = $"{GameSetting.CurrentRandom.ToString().Split( '_' )[0]}";
        rate.text   = $"x{GameSetting.CurrentPitch:F1}";
        rate.color  = GameSetting.CurrentPitch < 1f ? new Color( .5f, .5f, 1f ) :
                      GameSetting.CurrentPitch > 1f ? new Color( 1f, .5f, .5f ) : Color.white;

        UpdateInfo( GameSetting.CurrentPitch );
    }

    private void UpdateInfo( float _pitch )
    {
        pitch = _pitch;

        int second = ( int )( ( song.totalTime * .001f ) / pitch );
        int minute = second / 60;
        second = second % 60;

        length.text = $"{minute:00}:{second:00}";
        length.color = bpm.color = pitch < 1f ? new Color( .5f, .5f, 1f ) :
                                   pitch > 1f ? new Color( 1f, .5f, .5f ) : Color.white;

        UpdateNoteInfo();
        UpdateBPMInfo();
    }

    private void UpdateNoteInfo()
    {
        bool hasNoSlider = song.sliderCount != 0 && GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider );
        noteCount.text   = hasNoSlider ? $"{song.noteCount + song.sliderCount}" : $"{song.noteCount}";
        sliderCount.text = hasNoSlider ? $"{0}"                                 : $"{song.sliderCount}";

        noteCount.color   = hasNoSlider ? new Color( 1f, .5f, .5f ) : Color.white;
        sliderCount.color = hasNoSlider ? new Color( .5f, .5f, 1f ) : Color.white;
    }

    private void UpdateBPMInfo()
    {
        var medianBpm = Mathf.RoundToInt( ( float )song.medianBpm * GameSetting.CurrentPitch );
        var minBpm    = Mathf.RoundToInt( ( float )song.minBpm    * GameSetting.CurrentPitch );
        var maxBpm    = Mathf.RoundToInt( ( float )song.maxBpm    * GameSetting.CurrentPitch );
        bpm.text = song.minBpm == song.maxBpm || GameSetting.CurrentGameMode.HasFlag( GameMode.FixedBPM ) ? 
                   $"{medianBpm}" : $"{medianBpm} ({minBpm} ~ {maxBpm})";
        bpm.color = pitch < 1f ? new Color( .5f, .5f, 1f ) :
                    pitch > 1f ? new Color( 1f, .5f, .5f ) : Color.white;
    }
}
