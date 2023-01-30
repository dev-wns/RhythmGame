using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum OptionType { Title, Button, Slider, Text }
public interface IOption
{
    public OptionType type { get; }
    public void Process();
}

public abstract class OptionBase : MonoBehaviour, IOption
{
    public OptionType type { get; protected set; }

    private GameObject outline;

    protected virtual void Awake()
    {
        var outlineTf = transform.Find( "Outline" );
        if ( outlineTf )
        {
            outline = outlineTf.gameObject;
            ActiveOutline( false );
        }
    }

    public void ActiveOutline( bool _isActive )
    {
        if ( outline is null ) return;
        outline.SetActive( _isActive );
    }

    public abstract void Process();
    public abstract void InputProcess();
}
