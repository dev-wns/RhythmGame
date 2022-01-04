using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollText : ScrollBase
{
    public string[] texts;
    public string curText;

    private Scene scene;
    private DelKeyAction keyLeftAction, keyRightAction;

    private void Awake()
    {
        IsLoop = true;
        maxIndex = texts.Length;

        scene ??= GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
        keyLeftAction += PrevMove;
        keyRightAction += NextMove;

        scene.AwakeBind( SceneAction.Lobby, KeyCode.LeftArrow );
        scene.AwakeBind( SceneAction.Lobby, KeyCode.RightArrow );
    }

    public override void PrevMove()
    {
        base.PrevMove();
        curText = texts[curIndex];
    }

    public override void NextMove()
    {
        base.NextMove();
        curText = texts[curIndex];
    }

    public void KeyBind()
    {
        scene.Bind( SceneAction.Lobby, KeyCode.LeftArrow,  keyLeftAction );
        scene.Bind( SceneAction.Lobby, KeyCode.RightArrow, keyRightAction );
    }

    public void KeyRemove()
    {
        scene.Remove( SceneAction.Lobby, KeyCode.LeftArrow,  keyLeftAction );
        scene.Remove( SceneAction.Lobby, KeyCode.RightArrow, keyRightAction );
    }    
}
