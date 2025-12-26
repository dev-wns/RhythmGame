using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityInputHandler
{
    private Dictionary<KeyCode, Dictionary<KeyState, Action>> actions = new Dictionary<KeyCode, Dictionary<KeyState, Action>>();
    public void Process()
    {
        foreach ( var code in actions.Keys )
        {
            foreach ( var type in actions[code].Keys )
            {
                switch ( type )
                {
                    case KeyState.Down: { if ( Input.GetKeyDown( code ) ) { actions[code][type]?.Invoke(); } } break;
                    case KeyState.Hold: { if ( Input.GetKey(     code ) ) { actions[code][type]?.Invoke(); } } break;
                    case KeyState.Up:   { if ( Input.GetKeyUp(   code ) ) { actions[code][type]?.Invoke(); } } break;
                }
            }
        }
    }

    public void Remove( KeyCode _code, KeyState _keyType, Action _action )
    {
        if ( !actions.ContainsKey( _code ) )           return;
        if ( !actions[_code].ContainsKey( _keyType ) ) return;
        if ( actions[_code][_keyType] == null )        return;

        foreach ( var action in actions[_code][_keyType].GetInvocationList() )
        {
            if ( Equals( action, _action ) )
            {
                actions[_code][_keyType] -= _action;
                break;
            }
        }
    }

    public void Bind( KeyCode _code, KeyState _type, Action _action )
    {
        if ( _action == null )
             return;

        if ( actions.ContainsKey( _code ) )
        {
            if ( actions[_code].ContainsKey( _type ) )
                 actions[_code][_type] += _action;
            else
                actions[_code].Add( _type, _action );
        }
        else
        {
            Dictionary<KeyState, Action> typeAction = new ();
            typeAction.Add( _type, _action );
            actions.Add( _code, typeAction );
        }
    }
}