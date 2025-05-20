using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SongPreview : MonoBehaviour
{
    public FreeStyleMainScroll scroller;
    public SoundPitchOption    pitchOption;
    public NoSliderOption      noSliderOption;
    public FixedBPMOption      fixedBPMOption;
    public KeyConversionOption keyConversionOption;

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

    //[Header("Vertical")]
    //public Image keySound;

    [Header("Background Type")]
    public Image  backgroundType;
    public Sprite bgImage;
    public Sprite bgSprite;
    public Sprite bgVideo;

    private Song song;
    private float pitch;
    private bool hasKeyConversion;

    private void Awake()
    {
        scroller.OnSelectSong += SelectChangedSoundInfo;
        pitchOption.OnPitchUpdate += UpdateInfo;
        fixedBPMOption.OnChangeOption += UpdateBPMInfo;
        noSliderOption.OnChangeOption += UpdateNoteInfo;
        keyConversionOption.OnChangeOption += UpdateButton;
    }

    private void SelectChangedSoundInfo( Song _song )
    {
        song = _song;
        backgroundType.sprite = song.hasVideo ? bgVideo :
                                song.hasSprite ? bgSprite :
                                                 bgImage;

        //keySound.color = song.hasKeySound ? Color.white : new Color( 1f, 1f, 1f, .25f );

        speed.text = $"{GameSetting.ScrollSpeed:F1}";
        random.text = $"{GameSetting.CurrentRandom.ToString().Split( '_' )[0]}";
        rate.text = $"x{GameSetting.CurrentPitch:F2}";
        rate.color = GameSetting.CurrentPitch < 1f ? new Color( .5f, .5f, 1f ) :
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

        UpdateButton();
        UpdateBPMInfo();
    }

    private void UpdateButton()
    {
        hasKeyConversion = GameSetting.CurrentGameMode.HasFlag( GameMode.KeyConversion ) && song.keyCount == 7;
        keyCount.color = hasKeyConversion ? new Color( .5f, 1f, .5f, 1f ) : Color.white;
        keyCount.text = hasKeyConversion ? $"{6}K" : $"{song.keyCount}K";

        UpdateNoteInfo();
    }

    private void UpdateNoteInfo()
    {
        bool hasNoSlider = song.sliderCount != 0 && GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider );
        var slider       = hasKeyConversion ? song.sliderCount - song.delSliderCount : song.sliderCount;
        var note         = hasKeyConversion ? song.noteCount   - song.delNoteCount   : song.noteCount;

        noteCount.text = hasNoSlider ? $"{note + slider}" : $"{note}";
        sliderCount.text = hasNoSlider ? $"{0}" : $"{slider}";

        noteCount.color = hasKeyConversion || hasNoSlider ? new Color( .5f, 1f, .5f ) : Color.white;
        sliderCount.color = hasKeyConversion || hasNoSlider ? new Color( .5f, 1f, .5f ) : Color.white;
    }

    private void UpdateBPMInfo()
    {
        var mainBPM = Mathf.RoundToInt( ( float )song.mainBPM * GameSetting.CurrentPitch );
        var minBpm    = Mathf.RoundToInt( ( float )song.minBpm    * GameSetting.CurrentPitch );
        var maxBpm    = Mathf.RoundToInt( ( float )song.maxBpm    * GameSetting.CurrentPitch );
        bpm.text = song.minBpm == song.maxBpm || GameSetting.CurrentGameMode.HasFlag( GameMode.FixedBPM ) ?
                   $"{mainBPM}" : $"{mainBPM} ({minBpm} ~ {maxBpm})";
        bpm.color = pitch < 1f ? new Color( .5f, .5f, 1f ) :
                    pitch > 1f ? new Color( 1f, .5f, .5f ) : Color.white;
    }
}
