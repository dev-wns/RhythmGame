using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VolumeControl : MonoBehaviour, IOptionSlider
{
    public Slider slider;
    public TextMeshProUGUI valueText;
    public CHANNEL_GROUP_TYPE groupType = CHANNEL_GROUP_TYPE.MASTER;
    public OptionType type { get; } = OptionType.Slider;
    private float volume_0_1;
    private int volume_0_100;

    protected void Awake()
    {
        slider = GetComponentInChildren<Slider>();

        if ( slider == null )
        {
            slider = GetComponent<Slider>() ?? gameObject.AddComponent<Slider>();
        }

        volume_0_1     = SoundManager.Inst.GetVolume( groupType );
        volume_0_100   = Mathf.RoundToInt( volume_0_1 * 100 );
        slider.value   = volume_0_100;
        valueText.text = volume_0_100.ToString();
    }

    public void Process( int _value )
    {
        SoundManager.Inst.SetVolume( volume_0_1 + ( _value * .01f ), groupType );

        volume_0_1   = SoundManager.Inst.GetVolume( groupType );
        volume_0_100 = Mathf.RoundToInt( volume_0_1 * 100 );

        slider.value   = volume_0_100;
        valueText.text = volume_0_100.ToString();
    }
}
