using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SettingInfomation : MonoBehaviour
{
    private InGame game;
    public TextMeshProUGUI speed, pitch, random, auto, noSlider, noFail;

    private void Awake()
    {
        game = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        game.OnScrollChanged += () => speed.text = Globals.Round( GameSetting.ScrollSpeed ).ToString();

        speed.text  = $"{Globals.Round( GameSetting.ScrollSpeed )}";
        pitch.text  = $"{Globals.Round( GameSetting.SoundPitch ):F1}";
        random.text = $"{GameSetting.CurrentRandom.ToString().Split( '_' )[0]}";
        
        string temp = ( GameSetting.CurrentGameMode & GameMode.AutoPlay ) != 0 ? "On" : "Off";
        auto.text = $"{temp}";

        temp = ( GameSetting.CurrentGameMode & GameMode.NoSlider ) != 0 ? "On" : "Off";
        noSlider.text = $"{temp}";

        temp = ( GameSetting.CurrentGameMode & GameMode.NoFail ) != 0 ? "On" : "Off";
        noFail.text = $"{temp}";
    }
}
