using System;
using System.Collections.Generic;
using UnityEngine;

public enum KeyState { None, Down, Hold, Up, }
public class KeyAction
{
    private Dictionary<KeyCode, Dictionary<KeyState, Action>> keyActions = new Dictionary<KeyCode, Dictionary<KeyState, Action>>();
    public void ActionCheck()
    {
        foreach ( var code in keyActions.Keys )
        {
            foreach ( var type in keyActions[code].Keys )
            {
                switch ( type )
                {
                    case KeyState.Down: { if ( Input.GetKeyDown( code ) ) { keyActions[code][type]?.Invoke(); } } break;
                    case KeyState.Hold: { if ( Input.GetKey(     code ) ) { keyActions[code][type]?.Invoke(); } } break;
                    case KeyState.Up:   { if ( Input.GetKeyUp(   code ) ) { keyActions[code][type]?.Invoke(); } } break;
                }
            }
        }
    }

    public void Remove( KeyCode _code, KeyState _keyType, Action _action )
    {
        if ( !keyActions.ContainsKey( _code ) )           return;
        if ( !keyActions[_code].ContainsKey( _keyType ) ) return;
        if ( keyActions[_code][_keyType] == null )        return;

        foreach ( var action in keyActions[_code][_keyType].GetInvocationList() )
        {
            if ( Equals( action, _action ) )
            {
                keyActions[_code][_keyType] -= _action;
                break;
            }
        }
    }

    public void Bind( KeyCode _code, KeyState _type, Action _action )
    {
        //if ( _action == null || IsDuplicate( _code, _type, _action ) ) 
        //     return;

        //KeyAlloc( _code );
        //keyActions[_code][_type] += _action;

        if ( _action == null )
            return;

        if ( keyActions.ContainsKey( _code ) )
        {
            if ( keyActions[_code].ContainsKey( _type ) )
                keyActions[_code][_type] += _action;
            else
                keyActions[_code].Add( _type, _action );
        }
        else
        {
            var typeAction = new Dictionary<KeyState, Action>();
            typeAction.Add( _type, _action );
            keyActions.Add( _code, typeAction );
        }
    }

    private void KeyAlloc( KeyCode _code )
    {
        if ( !keyActions.ContainsKey( _code ) )
        {
            var typeAction = new Dictionary<KeyState, Action>();

            typeAction.Add( KeyState.Down, () => { } );
            typeAction.Add( KeyState.Hold, () => { } );
            typeAction.Add( KeyState.Up, () => { } );

            keyActions.Add( _code, typeAction );
        }
    }

    private bool IsDuplicate( KeyCode _code, KeyState _type, Action _action )
    {
        // KeyState은 KeyCode가 없으면 Down, Hold, Up을 전부 할당하기 때문에 체크 안해도 된다.
        if ( !keyActions.ContainsKey( _code ) )
            return false;

        foreach ( var action in keyActions[_code][_type].GetInvocationList() )
        {
            if ( Equals( action, _action ) )
            {
                Debug.LogWarning( "Key Bind Duplicate. " );
                return true; // 중복
            }
        }

        return false;
    }
}