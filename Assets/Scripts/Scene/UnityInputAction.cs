using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityInputAction : MonoBehaviour
{
    private Dictionary<SceneAction, UnityInputHandler> actions = new ();

    public UnityInputHandler GetHandler( SceneAction _sceneAction )
    {
        actions.TryGetValue( _sceneAction, out UnityInputHandler handler );
        return handler;
    }

    public void Bind( SceneAction _sceneAction, KeyState _keyState, KeyCode _keyCode, Action _action )
    {
        if ( !actions.TryGetValue( _sceneAction, out UnityInputHandler handler ) )
        {
            handler = new UnityInputHandler();
            actions.Add( _sceneAction, handler );
        }

        handler.Bind( _keyCode, _keyState, _action );
    }

    /// <summary> The input type is KeyDown </summary>
    public void Bind( SceneAction _sceneAction, KeyCode _keyCode, Action _action )
    {
        Bind( _sceneAction, KeyState.Down, _keyCode, _action );
    }

    public void Bind( SceneAction _sceneAction, KeyCode _keyCode, InputRepeater _repeater )
    {
        Bind( _sceneAction, KeyState.Down, _keyCode, _repeater.Down );
        Bind( _sceneAction, KeyState.Hold, _keyCode, _repeater.Hold );
        Bind( _sceneAction, KeyState.Up,   _keyCode, _repeater.Up   );
    }

    public void Remove( SceneAction _sceneAction, KeyState _keyState, KeyCode _keyCode, Action _action )
    {
        if ( actions.TryGetValue( _sceneAction, out UnityInputHandler handler ) )
        {
            handler.Remove( _keyCode, _keyState, _action );

            if ( handler.IsEmpty )
                 actions.Remove( _sceneAction );
        }
    }
}
