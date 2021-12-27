using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class SliderBase : MonoBehaviour, IOptionSlider
{
    public Slider slider;

    public OptionType type { get; } = OptionType.Slider;

    public Sound.ChannelType channelType = Sound.ChannelType.MasterGroup;

    //public abstract void KeyBind();

    protected virtual void Awake()
    {
        slider = GetComponentInChildren<Slider>();

        if ( slider == null )
        {
            slider = GetComponent<Slider>() ?? gameObject.AddComponent<Slider>();
        }
    }

    public abstract void Process( int _value );
}