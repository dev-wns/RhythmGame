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
    }

    public override void Process( int _value )
    {
        value = SoundManager.Inst.GetVolume() + ( _value * .01f );
        SoundManager.Inst.SetVolume( value, channelType );

        slider.value = value * 100;
        valueText.text = ( Mathf.RoundToInt( value * 100 ) ).ToString();
    }
}
