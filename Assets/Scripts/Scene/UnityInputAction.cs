using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnityInputAction : MonoBehaviour
{
    private Dictionary<SceneAction, UnityInputHandler> actions = new ();
    public SceneAction CurrentAction { get; private set; }
    public bool IsInputLock { get; set; }

    protected virtual void LateUpdate()
    {
        if ( IsInputLock || !actions.ContainsKey( CurrentAction ) )
             return;

        actions[CurrentAction].Process();
    }

    public void ChangeAction( SceneAction _actionType )
    {
        if ( !actions.ContainsKey( _actionType ) )
        {
            Bind( _actionType, KeyCode.JoystickButton19, () => { } );
            //Debug.LogError( $"The bound key does not exist. {_actionType}" );
            return;
        }

        CurrentAction = _actionType;
    }

    /// <summary> The input type is KeyDown </summary>
    public void Bind( SceneAction _actionType, KeyCode _keyCode, Action _action )
    {
        if ( actions.ContainsKey( _actionType ) )
        {
            actions[_actionType].Bind( _keyCode, KeyState.Down, _action );
        }
        else
        {
            UnityInputHandler handler = new UnityInputHandler();
            handler.Bind( _keyCode, KeyState.Down, _action );
            actions.Add( _actionType, handler );
        }
    }

    public void Bind( SceneAction _actionType, KeyState _inputType, KeyCode _keyCode, Action _action )
    {
        if ( actions.ContainsKey( _actionType ) )
        {
            actions[_actionType].Bind( _keyCode, _inputType, _action );
        }
        else
        {
            UnityInputHandler handler = new UnityInputHandler();
            handler.Bind( _keyCode, _inputType, _action );
            actions.Add( _actionType, handler );
        }
    }
    public void Remove( SceneAction _actionType, KeyCode _keyCode, Action _action )
    {
        if ( !actions.ContainsKey( _actionType ) ) return;

        actions[_actionType].Remove( _keyCode, KeyState.Down, _action );
    }

    public void Remove( SceneAction _actionType, KeyState _keyType, KeyCode _keyCode, Action _action )
    {
        if ( !actions.ContainsKey( _actionType ) ) return;

        actions[_actionType].Remove( _keyCode, _keyType, _action );
    }

    public abstract void KeyBind();
}
