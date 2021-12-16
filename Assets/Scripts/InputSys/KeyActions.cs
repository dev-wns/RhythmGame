using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SceneAction
{
    Lobby,
    FreeStyle, FreeStyleSetting,
    InGame, InGamePause,
    Result,
}

public class KeyActions 
{
    private Dictionary<SceneAction, StaticSceneKeyAction> keyActions = new Dictionary<SceneAction, StaticSceneKeyAction>();
    public StaticSceneKeyAction curAction;

    public void Bind( SceneAction _type, StaticSceneKeyAction _action )
    {
        if ( _action == null || keyActions.ContainsKey( _type ) )
            Debug.Log( "Duplicate binding." );

        keyActions.Add( _type, _action );
    }

    public void ChangeAction( SceneAction _type )
    {
        if ( !keyActions.ContainsKey( _type ) )
        {
            Debug.Log( "The bound key does not exist." );
            return;
        }

        curAction = keyActions[_type];
    }

    public void ActionCheck()
    {
        if ( curAction != null ) curAction.ActionCheck();
    }
}
