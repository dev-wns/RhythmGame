using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionType : byte
{
    Main, Option, SubOption, Pause, Exit,
}

public abstract class SceneKeyAction : MonoBehaviour, IKeyBind
{
    private Dictionary<ActionType, KeyAction> keyActions = new Dictionary<ActionType, KeyAction>();
    public ActionType CurrentAction { get; private set; }
    public bool IsInputLock          { get; set; }

    protected virtual void Update()
    {
        if ( IsInputLock || !keyActions.ContainsKey( CurrentAction ) ) 
             return;

        keyActions[CurrentAction].ActionCheck();
    }
    public void Bind( ActionType _type, KeyCode _code, Action _action )
    {
        if ( keyActions.ContainsKey( _type ) )
        {
            keyActions[_type].Bind( _code, InputType.Down, _action );
        }
        else
        {
            KeyAction keyAction = new KeyAction();
            keyAction.Bind( _code, InputType.Down, _action );
            keyActions.Add( _type, keyAction );
        }
    }
    public void Bind( ActionType _type, InputType _keyType, KeyCode _code, Action _action )
    {
        if ( keyActions.ContainsKey( _type ) )
        {
            keyActions[_type].Bind( _code, _keyType, _action );
        }
        else
        {
            KeyAction keyAction = new KeyAction();
            keyAction.Bind( _code, _keyType, _action );
            keyActions.Add( _type, keyAction );
        }
    }
    public void Remove( ActionType _type, KeyCode _code, Action _action )
    {
        if ( !keyActions.ContainsKey( _type ) ) return;

        keyActions[_type].Remove( _code, InputType.Down, _action );
    }
    public void Remove( ActionType _type, InputType _keyType, KeyCode _code, Action _action )
    {
        if ( !keyActions.ContainsKey( _type ) ) return;

        keyActions[_type].Remove( _code, _keyType, _action );
    }
    public void ChangeAction( ActionType _type )
    {
        if ( !keyActions.ContainsKey( _type ) )
        {
            Debug.LogError( $"The bound key does not exist. {_type}" );
            return;
        }

        CurrentAction = _type;
    }
    public abstract void KeyBind();
}
