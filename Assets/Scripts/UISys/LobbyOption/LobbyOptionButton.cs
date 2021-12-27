using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LobbyOptionButton : MonoBehaviour, IOptionButton
{
    public OptionType type { get; } = OptionType.Button;
    public int key;

    public abstract void Process();
}
