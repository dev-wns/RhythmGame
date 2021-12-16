using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum KeyType { Down, Hold, Up, }
public class StaticSceneKeyAction
{
    public delegate void DelKeyAction();
    private Dictionary<KeyCode, Dictionary<KeyType, DelKeyAction>> keyActions = new Dictionary<KeyCode, Dictionary<KeyType, DelKeyAction>>();

    public void ActionCheck()
    {
        foreach ( var keyAction in keyActions )
        {
            var keyCode = keyAction.Key;
            foreach ( var action in keyAction.Value )
            {
                var keyType  = action.Key;
                var function = action.Value;
                switch ( keyType )
                {
                    case KeyType.Down: { if ( Input.GetKeyDown( keyCode ) ) { function(); } } break;
                    case KeyType.Hold: { if ( Input.GetKey( keyCode ) )     { function(); } } break;
                    case KeyType.Up:   { if ( Input.GetKeyUp( keyCode ) )   { function(); } } break;
                }
            }
        }
    }

    public void Bind( KeyCode _key, KeyType _type, DelKeyAction _action )
    {
        if ( _action == null ) return;

        if ( !keyActions.ContainsKey( _key ) )
        {
            keyActions.Add( _key, new Dictionary<KeyType, DelKeyAction>() );
        }

        if ( !keyActions[_key].ContainsKey( _type ) )
        {
            keyActions[_key].Add( _type, _action );
            return;
        }
    
        // 람다는 중복체크 안됨.
        foreach ( var action in keyActions[_key][_type].GetInvocationList() )
        {
            if ( Equals( action, _action ) )
            {
                Debug.Log( "Key Bind 중복 입니다. " );
                return; // 중복
            }
        }

        keyActions[_key][_type] += _action;
    }
}