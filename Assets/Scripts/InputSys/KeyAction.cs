using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum KeyType { Down, Hold, Up, }
public delegate void DelKeyAction();
public class KeyAction
{
    private Dictionary<KeyCode, Dictionary<KeyType, DelKeyAction>> keyActions = new Dictionary<KeyCode, Dictionary<KeyType, DelKeyAction>>();
    
    public void ActionCheck()
    {
        foreach ( var code in keyActions.Keys )
        {
            foreach ( var type in keyActions[code].Keys )
            {
                switch ( type )
                {
                    case KeyType.Down: { if ( Input.GetKeyDown( code ) ) { keyActions[code][type]?.Invoke(); } } break;
                    case KeyType.Hold: { if ( Input.GetKey( code ) ) { keyActions[code][type]?.Invoke(); } } break;
                    case KeyType.Up: { if ( Input.GetKeyUp( code ) ) { keyActions[code][type]?.Invoke(); } } break;
                }
            }
        }
    }

    public void Remove( KeyCode _code, KeyType _keyType, DelKeyAction _action )
    {
        if ( !keyActions.ContainsKey( _code ) ) return;
        if ( !keyActions[_code].ContainsKey( _keyType ) ) return;
        if ( keyActions[_code][_keyType] == null ) return;

        foreach ( var action in keyActions[_code][_keyType].GetInvocationList() )
        {
            if ( Equals( action, _action ) )
            {
                keyActions[_code][_keyType] -= _action;
                break;
            }
        }
    }
    public void AwakeBind( KeyCode _code, KeyType _keyType )
    {
        if ( keyActions.ContainsKey( _code ) )
        {
            if ( keyActions[_code] == null )
            {
                var typeAction = new Dictionary<KeyType, DelKeyAction>();
                typeAction.Add( _keyType, null );
                keyActions[_code] = typeAction;
            }
        }
        else
        {
            var typeAction = new Dictionary<KeyType, DelKeyAction>();
            typeAction.Add( _keyType, null );
            keyActions.Add( _code, typeAction );
        }
    }

    public void Bind( KeyAction _action )
    {
        foreach ( var code in _action.keyActions.Keys )
        {
            foreach ( var type in _action.keyActions[code].Keys )
            {
                var action = _action.keyActions[code][type];
                Bind( code, type, action );
            }
        }
    }

    public void Bind( KeyCode _code, KeyType _type, DelKeyAction _action )
    {
        if ( _action == null ) return;

        if ( keyActions.ContainsKey( _code ) )
        {
            if ( keyActions[_code].ContainsKey( _type ) )
            {
                if ( IsDuplicate( _code, _type, _action ) )
                    return;

                keyActions[_code][_type] += _action;
            }
            else
            {
                var typeAction = new Dictionary<KeyType, DelKeyAction>();
                typeAction.Add( _type, _action );
                keyActions[_code] = typeAction;
            }
        }
        else
        {
            var typeAction = new Dictionary<KeyType, DelKeyAction>();
            typeAction.Add( _type, _action );
            keyActions.Add( _code, typeAction );
        }
    }

    private bool IsDuplicate( KeyCode _code, KeyType _type, DelKeyAction _action )
    {
        if ( keyActions[_code][_type] == null )
            return false;

        foreach ( var action in keyActions[_code][_type].GetInvocationList() )
        {
            if ( Equals( action, _action ) )
            {
                Debug.Log( "Key Bind 중복 입니다. " );
                return true; // 중복
            }
        }

        return false;
    }
}