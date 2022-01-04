using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollText : MonoBehaviour
{
    public string[] texts;
    protected int curIndex;
    public string curText;

    private Scene scene;
    private DelKeyAction keyLeftAction, keyRightAction;

    private void Awake()
    {
        scene ??= GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
        keyLeftAction += PrevMove;
        keyRightAction += NextMove;

        scene.AwakeBind( SceneAction.Lobby, KeyCode.LeftArrow );
        scene.AwakeBind( SceneAction.Lobby, KeyCode.RightArrow );
    }

    public void PrevMove()
    {
        if ( curIndex == 0 )
        {
            curIndex = texts.Length - 1;
            curText = texts[curIndex];
            return;
        }

        curText = texts[--curIndex];
    }

    public void NextMove()
    {
        if ( curIndex == texts.Length - 1 )
        {
            curIndex = 0;
            curText = texts[curIndex];
            return;
        }

        curText = texts[++curIndex];
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
