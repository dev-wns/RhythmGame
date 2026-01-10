using System;
using UnityEngine;

public class InputRepeater
{
    private bool  isPressed = false;
    private float waitTime = .5f;
    private float interval = .075f; //.05f;
    private float elapsed;

    private event Action OnRepeat;
    
    public InputRepeater() { }
    public InputRepeater( Action _action ) 
    {
        if ( _action != null )
             OnRepeat = _action; 
    }

    public void Down()
    {
        elapsed   = 0f;
        isPressed = false;
        OnRepeat?.Invoke();
    }

    public void Hold()
    {
        elapsed += Time.deltaTime;
        if ( elapsed >= waitTime )
             isPressed = true;

        if ( isPressed && elapsed >= interval )
        {
            elapsed = 0f;
            OnRepeat?.Invoke();
        }
    }

    public void Up()
    {
        elapsed   = 0f;
        isPressed = false;
    }
}