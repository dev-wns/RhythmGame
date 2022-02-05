using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SettingInfomation : MonoBehaviour
{
    public TextMeshProUGUI speed, offset, random, auto, noSlider, noFail;

    private void Awake()
    {
        var scene = GameObject.FindGameObjectWithTag( "Scene" );
        if ( scene )
        {
            InGame game;
            if ( scene.TryGetComponent( out game ) ) 
                 game.OnScrollChanged += () => speed.text = $"{GameSetting.ScrollSpeed:F1}";
        }

        speed.text  = $"{GameSetting.ScrollSpeed:F1}";
        offset.text = $"{Globals.Round( GameSetting.SoundOffset )}";
        random.text = $"{GameSetting.CurrentRandom.ToString().Split( '_' )[0]}";
        
        string temp = ( GameSetting.CurrentGameMode & GameMode.AutoPlay ) != 0 ? "On" : "Off";
        auto.text = $"{temp}";

        temp = ( GameSetting.CurrentGameMode & GameMode.NoSlider ) != 0 ? "On" : "Off";
        noSlider.text = $"{temp}";

        temp = ( GameSetting.CurrentGameMode & GameMode.NoFail ) != 0 ? "On" : "Off";
        noFail.text = $"{temp}";
    }
}
