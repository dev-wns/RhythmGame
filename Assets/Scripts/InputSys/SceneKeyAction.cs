using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionType : byte { Main, GameOption, SystemOption, KeySetting, Pause, Exit, }
public abstract class SceneKeyAction : MonoBehaviour
{
    private Dictionary<ActionType, KeyAction> keyActions = new Dictionary<ActionType, KeyAction>();
    public ActionType CurrentAction { get; private set; }
    public bool IsInputLock         { get; set; }

    protected virtual void Update()
    {
        if ( IsInputLock || !keyActions.ContainsKey( CurrentAction ) )
             return;

        keyActions[CurrentAction].ActionCheck();
    }
    /// <summary> The input type is KeyDown </summary>
    /// 

    public void Bind( ActionType _actionType, KeyCode _keyCode, Action _action )
    {
        if ( keyActions.ContainsKey( _actionType ) )
        {
            keyActions[_actionType].Bind( _keyCode, InputType.Down, _action );
        }
        else
        {
            KeyAction keyAction = new KeyAction();
            keyAction.Bind( _keyCode, InputType.Down, _action );
            keyActions.Add( _actionType, keyAction );
        }
    }
    public void Bind( ActionType _actionType, InputType _inputType, KeyCode _keyCode, Action _action ) {
        if ( keyActions.ContainsKey( _actionType ) ) {
            keyActions[_actionType].Bind( _keyCode, _inputType, _action );
        }
        else {
            KeyAction keyAction = new KeyAction();
            keyAction.Bind( _keyCode, _inputType, _action );
            keyActions.Add( _actionType, keyAction );
        }
    }
    public void Remove( ActionType _actionType, KeyCode _keyCode, Action _action )
    {
        if ( !keyActions.ContainsKey( _actionType ) ) return;

        keyActions[_actionType].Remove( _keyCode, InputType.Down, _action );
    }
    public void Remove( ActionType _actionType, InputType _keyType, KeyCode _keyCode, Action _action )
    {
        if ( !keyActions.ContainsKey( _actionType ) ) return;

        keyActions[_actionType].Remove( _keyCode, _keyType, _action );
    }
    public void ChangeAction( ActionType _actionType )
    {
        if ( !keyActions.ContainsKey( _actionType ) )
        {
            Debug.LogError( $"The bound key does not exist. {_actionType}" );
            return;
        }

        CurrentAction = _actionType;
    }
    public abstract void KeyBind();
}
