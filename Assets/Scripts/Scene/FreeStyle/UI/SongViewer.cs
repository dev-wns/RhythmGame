using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SongViewer : MonoBehaviour
{
    [SerializeField] private FreeStyle         scene;
    [SerializeField] private SoundPitchOption  pitchOption;
    [SerializeField] private ScrollSpeedOption speedOption;
    [SerializeField] private NoSliderOption    noSliderOption;
    [SerializeField] private FixedBPMOption    fixedBPMOption;
    [SerializeField] private KeyConvertOption  keyConvertOption;

    [Header("Horizontal")]
    [Header("Line 0")]
    [SerializeField] private TextMeshProUGUI keyCount;
    [SerializeField] private TextMeshProUGUI speed;
    [SerializeField] private TextMeshProUGUI rate;
    [SerializeField] private TextMeshProUGUI random;

    [Header("Line 1")]
    [SerializeField] private TextMeshProUGUI length;
    [SerializeField] private TextMeshProUGUI noteCount;
    [SerializeField] private TextMeshProUGUI sliderCount;
    [SerializeField] private TextMeshProUGUI bpm;

    [Header("Background Type")]
    [SerializeField] private BGAIconSO bgaIconSO;
    [SerializeField] private Image     bgaIcon;

    private Song song;
    private float pitch;
    private bool hasKeyConvert;

    private void Awake()
    {
        scene.OnStart += SelectChangedSoundInfo;
        fixedBPMOption.OnChangedOption   += UpdateBPMInfo;
        noSliderOption.OnChangedOption   += UpdateNoteInfo;
        keyConvertOption.OnChangedOption += UpdateButton;
        pitchOption.OnUpdatePitch        += UpdatePitchInfo;
        GameSetting.OnChangedSpeed       += UpdateScrollSpeed;
    }

    private void OnDestroy()
    {
        scene.OnStart -= SelectChangedSoundInfo;
        fixedBPMOption.OnChangedOption   -= UpdateBPMInfo;
        noSliderOption.OnChangedOption   -= UpdateNoteInfo;
        keyConvertOption.OnChangedOption -= UpdateButton;
        pitchOption.OnUpdatePitch        -= UpdatePitchInfo;
        GameSetting.OnChangedSpeed       -= UpdateScrollSpeed;
    }

    private void SelectChangedSoundInfo( in Song _song )
    {
        song = _song;
        bgaIcon.sprite = bgaIconSO.GetSprite( in _song );

        speed.SetText( "{0}", GameSetting.ScrollSpeed );
        random.SetText( GameSetting.ToString( GameSetting.CurrentRandom ) );

        rate.SetText( "x{0:2}", ( float )GameSetting.SoundPitch );
        rate.color =     GameSetting.SoundPitch < 1f ? new Color( .5f, .5f, 1f ) :
                         GameSetting.SoundPitch > 1f ? new Color( 1f, .5f, .5f ) : Color.white;

        UpdatePitchInfo( GameSetting.SoundPitch );
    }

    private void UpdateScrollSpeed( int _scrollSpeed ) => speed.SetText( "{0}", _scrollSpeed );


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
        var mainBPM = Mathf.RoundToInt( ( float )song.mainBPM * GameSetting.SoundPitch );
        var minBpm  = Mathf.RoundToInt( ( float )song.minBpm  * GameSetting.SoundPitch );
        var maxBpm  = Mathf.RoundToInt( ( float )song.maxBpm  * GameSetting.SoundPitch );

        bool isSV = !( song.minBpm == song.maxBpm || GameSetting.HasFlag( GameMode.FixedBPM ) );
        if ( isSV ) bpm.SetText( "{0} ({1} ~ {2})", mainBPM, minBpm, maxBpm );
        else        bpm.SetText( "{0}", mainBPM );
        bpm.color = pitch < 1f ? new Color( .5f, .5f, 1f ) :
                    pitch > 1f ? new Color( 1f, .5f, .5f ) : Color.white;
    }
}
