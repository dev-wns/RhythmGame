using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SceneAction
{
    Lobby, LobbyOption, LobbySubOption,
    FreeStyle, FreeStyleOption,
    InGame, InGamePause,
    Result,
    Exit,
}

public class SceneKeyAction : MonoBehaviour
{
    private Dictionary<SceneAction, KeyAction> keyActions = new Dictionary<SceneAction, KeyAction>();
    public SceneAction curAction { get; private set; }
    private bool IsLock = false;

    public void InputLock( bool _isLock ) => IsLock = _isLock;

    public void ActionCheck()
    {
        if ( IsLock || !keyActions.ContainsKey( curAction ) )
        {
            return;
        }

        keyActions[curAction].ActionCheck();
    }

    public void AwakeBind( SceneAction _type, KeyCode _code )
    {
        if ( keyActions.ContainsKey( _type ) )
        {
            if ( keyActions[_type] == null )
            {
                KeyAction keyAction = new KeyAction();
                keyAction.AwakeBind( _code, KeyType.Down );
                keyActions[_type] = keyAction;
            }
            else
            {
                keyActions[_type].AwakeBind( _code, KeyType.Down );
            }
        }
        else
        {
            KeyAction keyAction = new KeyAction();
            keyAction.AwakeBind( _code, KeyType.Down );
            keyActions.Add( _type, keyAction );
        }
    }

    public void Bind( SceneAction _type, KeyCode _code, DelKeyAction _action )
    {
        if ( keyActions.ContainsKey( _type ) )
        {
            keyActions[_type].Bind( _code, KeyType.Down, _action );
        }
        else
        {
            KeyAction keyAction = new KeyAction();
            keyAction.Bind( _code, KeyType.Down, _action );
            keyActions.Add( _type, keyAction );
        }
    }

    public void Remove( SceneAction _type, KeyCode _code, DelKeyAction _action )
    {
        if ( !keyActions.ContainsKey( _type ) ) return;

        keyActions[_type].Remove( _code, KeyType.Down, _action );
    }

    public void ChangeAction( SceneAction _type )
    {
        if ( !keyActions.ContainsKey( _type ) )
        {
            Debug.Log( "The bound key does not exist." );
            return;
        }

        curAction = _type;
    }
}
