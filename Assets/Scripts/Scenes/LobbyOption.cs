using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyOption : MonoBehaviour, IKeyBind
{
    public GameObject[] contents;
    public GameObject outline;

    public GameObject optionCanvas;

    public bool IsDuplicate { get; private set; }
    public GameObject curOption;
    public int curIndex;

    public Scene currentScene;

    private void Awake()
    {
        if ( contents.Length > 0 )
        {
            curIndex = 0;
            curOption = contents[0];

            outline.transform.SetParent( curOption.transform );
            RectTransform rt = outline.transform as RectTransform;
            rt.anchoredPosition = Vector2.zero;
        }

        currentScene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
        KeyBind();
    }

    private void PrevMove()
    {
        if ( curIndex == 0 )
        {
            IsDuplicate = true;
            return;
        }

        curOption = contents[--curIndex];
        outline.transform.SetParent( curOption.transform );
        RectTransform rt = outline.transform as RectTransform;
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

        curOption = contents[++curIndex];
        outline.transform.SetParent( curOption.transform );
        RectTransform rt = outline.transform as RectTransform;
        rt.anchoredPosition = Vector2.zero;

        IsDuplicate = false;
    }

    public void KeyBind()
    {
        StaticSceneKeyAction scene = new StaticSceneKeyAction();
        scene.Bind( KeyCode.UpArrow,   KeyType.Down, () => PrevMove() );
        scene.Bind( KeyCode.DownArrow, KeyType.Down, () => NextMove() );

        scene.Bind( KeyCode.Escape, KeyType.Down, () => currentScene.ChangeKeyAction( SceneAction.Lobby ) );
        scene.Bind( KeyCode.Escape, KeyType.Down, () => optionCanvas.SetActive( false ) );

        scene.Bind( KeyCode.Space,  KeyType.Down, () => currentScene.ChangeKeyAction( SceneAction.Lobby ) );
        scene.Bind( KeyCode.Space,  KeyType.Down, () => optionCanvas.SetActive( false ) );

        scene.Bind( KeyCode.Return, KeyType.Down, () => curOption.GetComponent<OptionBase>().Process() );


        currentScene.KeyBind( SceneAction.LobbyOption, scene );
    }
}
