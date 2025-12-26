using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SongViewer : MonoBehaviour
{
    public FreeStyleMainScroll scroller;
    public NoSliderOption      noSliderOption;
    public FixedBPMOption      fixedBPMOption;
    public KeyConvertOption    keyConvertOption;

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

    [Header("Background Type")]
    public Image  bgTypeImage;
    public Sprite bgImage;
    public Sprite bgSprite;
    public Sprite bgVideo;

    private Song song;
    private float pitch;
    private bool hasKeyConvert;

    private void Awake()
    {
        scroller.OnSelectSong           += SelectChangedSoundInfo;
        fixedBPMOption.OnChangeOption   += UpdateBPMInfo;
        noSliderOption.OnChangeOption   += UpdateNoteInfo;
        keyConvertOption.OnChangeOption += UpdateButton;
        AudioManager.OnUpdatePitch      += UpdatePitchInfo;
    }

    private void OnDestroy()
    {
        scroller.OnSelectSong           -= SelectChangedSoundInfo;
        fixedBPMOption.OnChangeOption   -= UpdateBPMInfo;
        noSliderOption.OnChangeOption   -= UpdateNoteInfo;
        keyConvertOption.OnChangeOption -= UpdateButton;
        AudioManager.OnUpdatePitch      -= UpdatePitchInfo;
    }

    private void SelectChangedSoundInfo( Song _song )
    {
        song = _song;
        bgTypeImage.sprite = song.hasVideo  ? bgVideo  :
                             song.hasSprite ? bgSprite :
                                              bgImage;

        //keySound.color = song.hasKeySound ? Color.white : new Color( 1f, 1f, 1f, .25f );

        speed.SetText( "{0}", GameSetting.ScrollSpeed );
        random.SetText( GameSetting.ToString( GameSetting.CurrentRandom ) );

        rate.SetText( "x{0:2}", ( float )GameSetting.CurrentPitch );
        rate.color =     GameSetting.CurrentPitch < 1f ? new Color( .5f, .5f, 1f ) :
                         GameSetting.CurrentPitch > 1f ? new Color( 1f, .5f, .5f ) : Color.white;

        UpdatePitchInfo( GameSetting.CurrentPitch );
    }

    private void UpdatePitchInfo( float _pitch )
    {
        pitch = _pitch;

        int second = ( int )( ( song.totalTime * .001f ) / pitch );
        int minute = second / 60;
        second = second % 60;

        length.SetText( "{0:00}:{1:00}", minute, second );
        length.color = bpm.color = pitch < 1f ? new Color( .5f, .5f, 1f ) :
                                   pitch > 1f ? new Color( 1f, .5f, .5f ) : Color.white;

        UpdateButton();
        UpdateBPMInfo();
    }

    private void UpdateButton()
    {
        hasKeyConvert  = GameSetting.HasFlag( GameMode.ConvertKey ) && song.keyCount == 7;
        keyCount.color = hasKeyConvert ? new Color( .5f, 1f, .5f, 1f ) : Color.white;
        keyCount.SetText( "{0}K", hasKeyConvert ? 6 : song.keyCount );

        UpdateNoteInfo();
    }

    private void UpdateNoteInfo()
    {
        bool hasNoSlider = song.sliderCount != 0 && GameSetting.HasFlag( GameMode.NoSlider );
        var slider       = hasKeyConvert ? song.sliderCount - song.delSliderCount : song.sliderCount;
        var note         = hasKeyConvert ? song.noteCount   - song.delNoteCount   : song.noteCount;

        noteCount.SetText(   "{0}", hasNoSlider ? ( note + slider ) : note );
        sliderCount.SetText( "{0}", hasNoSlider ? 0 : slider );

        noteCount.color   = hasKeyConvert || hasNoSlider ? new Color( .5f, 1f, .5f ) : Color.white;
        sliderCount.color = hasKeyConvert || hasNoSlider ? new Color( .5f, 1f, .5f ) : Color.white;
    }

    private void UpdateBPMInfo()
    {
        var mainBPM = Mathf.RoundToInt( ( float )song.mainBPM * GameSetting.CurrentPitch );
        var minBpm  = Mathf.RoundToInt( ( float )song.minBpm  * GameSetting.CurrentPitch );
        var maxBpm  = Mathf.RoundToInt( ( float )song.maxBpm  * GameSetting.CurrentPitch );

        bool isSV = !( song.minBpm == song.maxBpm || GameSetting.HasFlag( GameMode.FixedBPM ) );
        if ( isSV ) bpm.SetText( "{0} ({1} ~ {2})", mainBPM, minBpm, maxBpm );
        else        bpm.SetText( "{0}", mainBPM );
        bpm.color = pitch < 1f ? new Color( .5f, .5f, 1f ) :
                    pitch > 1f ? new Color( 1f, .5f, .5f ) : Color.white;
    }
}
