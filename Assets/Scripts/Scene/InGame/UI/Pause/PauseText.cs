using TMPro;
using UnityEngine;

public class PauseInfomation : MonoBehaviour
{
    [SerializeField] private InGame scene;

    [Header( "Main Info" )]
    public TextMeshProUGUI speed;
    public TextMeshProUGUI rate;
    public TextMeshProUGUI random;

    [Header( "Game Mode" )]
    public TextMeshProUGUI autoPlay;
    public TextMeshProUGUI noFail;
    public TextMeshProUGUI noSlider;
    public TextMeshProUGUI fixedBPM;
    public TextMeshProUGUI hardJudge;
    public TextMeshProUGUI keyConversion;
    private Color disableColor = new Color( 1f, 1f, 1f, .25f );

    private void Awake()
    {
        GameSetting.OnChangedSpeed += UpdateScrollSpeed;

        // Main Info
        UpdateScrollSpeed( GameSetting.ScrollSpeed );
        random.SetText( GameSetting.ToString( GameSetting.CurrentRandom ) );
        rate.SetText( "x{0:2}", ( float )GameSetting.SoundPitch );
        rate.color = GameSetting.SoundPitch < 1f ? new Color( .5f, .5f,  1f ) :
                     GameSetting.SoundPitch > 1f ? new Color(  1f, .5f, .5f ) : Color.white;

        // Game Mode
        autoPlay.color      = GameSetting.CurrentGameMode.HasFlag( GameMode.AutoPlay )   ? Color.white : disableColor;
        noFail.color        = GameSetting.CurrentGameMode.HasFlag( GameMode.NoFail )     ? Color.white : disableColor;
        noSlider.color      = GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider )   ? Color.white : disableColor;
        fixedBPM.color      = GameSetting.CurrentGameMode.HasFlag( GameMode.FixedBPM )   ? Color.white : disableColor;
        keyConversion.color = GameSetting.CurrentGameMode.HasFlag( GameMode.ConvertKey ) ? Color.white : disableColor;
    }

    private void OnDestroy()
    {
        GameSetting.OnChangedSpeed -= UpdateScrollSpeed;
    }

    private void UpdateScrollSpeed( int _scrollSpeed )
    {
        speed.SetText( "{0}", _scrollSpeed );
    }
}