using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OptionType { Title, Button, Slider, Text }

public interface IOption
{
    public OptionType type { get; }
    public void Process();
}