using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyOptionSlider : SliderBase
{
    public TextMeshProUGUI valueText;
    private float value;

    protected override void Awake()
    {
        base.Awake();

        var valueObj = transform.Find( "Value" );
        valueText = valueObj.GetComponent<TextMeshProUGUI>();

        value = SoundManager.Inst.GetVolume( channelType );
        slider.value = Mathf.RoundToInt( value * 100f );
        valueText.text = slider.value.ToString();
    }

    public override void Process( int _value )
    {
        value = SoundManager.Inst.SetVolume( value + ( _value * .01f ), channelType );
        slider.value = Mathf.RoundToInt( value * 100f );
        valueText.text = slider.value.ToString();
    }
}
