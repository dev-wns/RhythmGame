using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyOption : MonoBehaviour
{
    public GameObject[] contents;
    public GameObject selector;

    public bool IsDuplicate { get; private set; }
    public GameObject curObj;
    public int curIndex;

    public Scene CurrentScene;

    private void Start()
    {
        if ( contents.Length > 0 )
        {
            curIndex = 0;
            curObj = contents[0];

            selector.transform.SetParent( curObj.transform );
            RectTransform rt = selector.transform as RectTransform;
            rt.anchoredPosition = Vector2.zero;
        }

        CurrentScene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
        KeyBind();
    }

    private void PrevMove()
    {
        if ( curIndex == 0 )
        {
            IsDuplicate = true;
            return;
        }

        curObj = contents[--curIndex];
        selector.transform.SetParent( curObj.transform );
        RectTransform rt = selector.transform as RectTransform;
        rt.anchoredPosition = Vector2.zero;

        IsDuplicate = false;
    }

    private void NextMove()
    {
        if ( curIndex == contents.Length - 1 )
        {
            IsDuplicate = true;
            return;
        }

        curObj = contents[++curIndex];
        selector.transform.SetParent( curObj.transform );
        RectTransform rt = selector.transform as RectTransform;
        rt.anchoredPosition = Vector2.zero;

        IsDuplicate = false;
    }

    private void KeyBind()
    {
        StaticSceneKeyAction scene = new StaticSceneKeyAction();
        scene.Bind( KeyCode.UpArrow,   KeyType.Down, () => PrevMove() );
        scene.Bind( KeyCode.DownArrow, KeyType.Down, () => NextMove() );

        scene.Bind( KeyCode.Escape, KeyType.Down, () => CurrentScene.ChangeKeyAction( SceneAction.Lobby ) );
        scene.Bind( KeyCode.Space,  KeyType.Down, () => CurrentScene.ChangeKeyAction( SceneAction.Lobby ) );

        CurrentScene.KeyBind( SceneAction.LobbyOption, scene );
    }
}
