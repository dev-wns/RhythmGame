using TMPro;
using UnityEngine;

public class PauseInfomation : MonoBehaviour
{
    [Header( "Main Infomation" )]
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
        var scene = GameObject.FindGameObjectWithTag( "Scene" );
        if ( scene.TryGetComponent( out InGame inGame ) )
            inGame.OnScrollChange += UpdateScrollSpeedText;

        // Main Infomation
        UpdateScrollSpeedText();
        random.text = $"{GameSetting.CurrentRandom.ToString().Split( '_' )[0]}";
        rate.text = $"x{GameSetting.CurrentPitch:F2}";
        rate.color = GameSetting.CurrentPitch < 1f ? new Color( .5f, .5f, 1f ) :
                      GameSetting.CurrentPitch > 1f ? new Color( 1f, .5f, .5f ) : Color.white;

        // Game Mode
        autoPlay.color      = GameSetting.CurrentGameMode.HasFlag( GameMode.AutoPlay )   ? Color.white : disableColor;
        noFail.color        = GameSetting.CurrentGameMode.HasFlag( GameMode.NoFail )     ? Color.white : disableColor;
        noSlider.color      = GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider )   ? Color.white : disableColor;
        fixedBPM.color      = GameSetting.CurrentGameMode.HasFlag( GameMode.FixedBPM )   ? Color.white : disableColor;
        keyConversion.color = GameSetting.CurrentGameMode.HasFlag( GameMode.ConvertKey ) ? Color.white : disableColor;
    }

    private void UpdateScrollSpeedText()
    {
        speed.text = $"{GameSetting.ScrollSpeed}";
    }
}