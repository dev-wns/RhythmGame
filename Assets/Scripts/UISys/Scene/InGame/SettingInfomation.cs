using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SettingInfomation : MonoBehaviour
{
    public TextMeshProUGUI scrollSpeed, pitch, random, auto, noSlider, noFail;

    private void Awake()
    {
        var scene = GameObject.FindGameObjectWithTag( "Scene" );
        if ( scene )
        {
            InGame game;
            if ( scene.TryGetComponent( out game ) ) 
                 game.OnScrollChange += () => scrollSpeed.text = $"{GameSetting.ScrollSpeed:F1}";
        }

        UpdateInfomation();
    }

    public void UpdateInfomation()
    {
        scrollSpeed.text = $"{GameSetting.ScrollSpeed:F1}";
        random.text      = $"{GameSetting.CurrentRandom.ToString().Split( '_' )[0]}";
        pitch.text       = $"x{GameSetting.CurrentPitch:F1}";
        pitch.color      = GameSetting.CurrentPitch < 1f ? new Color( .5f, .5f, 1f ) :
                           GameSetting.CurrentPitch > 1f ? new Color( 1f, .5f, .5f ) : Color.white;

        string temp = ( GameSetting.CurrentGameMode & GameMode.AutoPlay ) != 0 ? "On" : "Off";
        auto.text   = $"{temp}";

        temp          = ( GameSetting.CurrentGameMode & GameMode.NoSlider ) != 0 ? "On" : "Off";
        noSlider.text = $"{temp}";

        temp        = ( GameSetting.CurrentGameMode & GameMode.NoFail ) != 0 ? "On" : "Off";
        noFail.text = $"{temp}";
    }
}
