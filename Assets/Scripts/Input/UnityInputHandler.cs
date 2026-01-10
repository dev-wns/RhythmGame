using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityInputHandler
{
    // Action 내부에서 Bind/Remove를 호출하면, 순회도중 컬렉션이 수정되어 문제가 발생합니다.
    private Dictionary<KeyCode, Action> downActions = new ();
    private Dictionary<KeyCode, Action> holdActions = new ();
    private Dictionary<KeyCode, Action> upActions   = new ();

    private KeyCode activeKey = KeyCode.None;
    public bool IsEmpty => downActions.Count == 0 && holdActions.Count == 0 && upActions.Count == 0;

    public void Process()
    {
        // 중복 입력 시 가장 마지막에 누른 키만 처리
        foreach ( KeyCode keyCode in downActions.Keys )
        {
            if ( Input.GetKeyDown( keyCode ) )
            {
                activeKey = keyCode;
                downActions[keyCode]?.Invoke();
            }
        }

        if ( activeKey == KeyCode.None )
             return;

        if ( holdActions.TryGetValue(activeKey, out Action holdAction ) )
        {
            if ( Input.GetKey( activeKey ) )
                 holdAction?.Invoke();
        }

        if ( upActions.TryGetValue( activeKey, out Action upAction ) )
        {
            if ( Input.GetKeyUp( activeKey ) )
            {
                upAction?.Invoke();
                activeKey = KeyCode.None;
            }
        }
    }

    public void Bind( KeyCode _keyCode, KeyState _keyState, Action _action )
    {
        if ( _action == null ) 
             return;

        Dictionary<KeyCode, Action> map = GetActionMap( _keyState );
        if ( map == null ) return;

        if ( map.ContainsKey( _keyCode ) ) map[_keyCode] += _action;
        else                               map.Add( _keyCode, _action );
    }

    public void Remove( KeyCode _keyCode, KeyState _keyState, Action _action )
    {
        if ( _action == null )
             return;

        Dictionary<KeyCode, Action> map = GetActionMap( _keyState );
        if ( map == null ) return;
        if ( map.ContainsKey( _keyCode ) )
        {
            map[_keyCode] -= _action;
            if ( map[_keyCode] == null ) 
                 map.Remove( _keyCode );

            if ( activeKey == _keyCode )
                 activeKey = KeyCode.None;
        }
    }

    private Dictionary<KeyCode, Action> GetActionMap( KeyState _keyState )
    {
        return _keyState switch 
        {
            KeyState.Down => downActions,
            KeyState.Hold => holdActions,
            KeyState.Up   => upActions,
            _             => null 
        };
    }
}