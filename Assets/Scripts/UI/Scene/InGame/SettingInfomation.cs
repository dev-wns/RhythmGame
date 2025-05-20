using TMPro;
using UnityEngine;

public class SettingInfomation : MonoBehaviour
{
    [Header("Argument")]
    public TextMeshProUGUI scrollSpeed, pitch, random, auto, noSlider, noFail;

    private void Awake()
    {
        var scene = GameObject.FindGameObjectWithTag( "Scene" );
        if ( scene.TryGetComponent( out InGame inGame ) )
            inGame.OnScrollChange += () => scrollSpeed.text = $"{GameSetting.ScrollSpeed:F1}";

        UpdateInfomation();
    }

    public void UpdateInfomation()
    {
        scrollSpeed.text = $"{GameSetting.ScrollSpeed:F1}";
        random.text = $"{GameSetting.CurrentRandom.ToString().Split( '_' )[0]}";
        pitch.text = $"x{GameSetting.CurrentPitch:F1}";
        pitch.color = GameSetting.CurrentPitch < 1f ? new Color( .5f, .5f, 1f ) :
                           GameSetting.CurrentPitch > 1f ? new Color( 1f, .5f, .5f ) : Color.white;

        if ( auto )
        {
            string temp = ( GameSetting.CurrentGameMode & GameMode.AutoPlay ) != 0 ? "On" : "Off";
            auto.text = $"{temp}";
        }

        if ( noSlider )
        {
            string temp = ( GameSetting.CurrentGameMode & GameMode.NoSlider ) != 0 ? "On" : "Off";
            noSlider.text = $"{temp}";
        }

        if ( noFail )
        {
            string temp = ( GameSetting.CurrentGameMode & GameMode.NoFail ) != 0 ? "On" : "Off";
            noFail.text = $"{temp}";
        }
    }
}
