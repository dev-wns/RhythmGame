using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OptionType { Button, Slider, CheckBox }
public interface IOption
{
    public OptionType type { get; }
}

public interface IOptionButton : IOption
{
    public void Process();
}

public interface IOptionSlider : IOption
{
    public void Process( int _value );
}
